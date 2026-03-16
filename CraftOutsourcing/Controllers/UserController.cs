using System.Security.Claims;
using CraftOutsourcing.Data;
using CraftOutsourcing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CraftOutsourcing.Controllers
{
    [Authorize(Roles = "User")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }

        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();

            ViewBag.ActiveAssignments = await _context.Assignments
                .CountAsync(a => a.UserId == userId && a.Status == "InProgress");

            ViewBag.TotalEarnings = await _context.Payments
                .Where(p => p.UserId == userId)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            ViewBag.PendingSubmissions = await _context.Submissions
                .CountAsync(s => s.Assignment.UserId == userId && s.Status == "Pending");

            // Tính tổng tiền phạt (cả Active và Deducted)
            ViewBag.TotalPenalties = await _context.Penalties
                .Where(p => p.UserId == userId && (p.Status == "Active" || p.Status == "Deducted"))
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            return View();
        }

        // Trang xem cong thuc lam do
        public IActionResult Recipes() => View();

        // Trang tien do
        public IActionResult MyProgress() => View();

        // ==========================================
        // API CHO USER AJAX
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetMyAssignments()
        {
            int userId = GetCurrentUserId();

            var assignments = await _context.Assignments
                .Include(a => a.Product)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AssignedDate)
                .Select(a => new
                {
                    a.Id,
                    Product = a.Product.Name,
                    a.QuantityAssigned,
                    AssignedDate = a.AssignedDate.ToString("dd/MM/yyyy"),
                    DueDate = a.DueDate.ToString("dd/MM/yyyy"),
                    a.Status,
                    IsOverdue = a.DueDate < DateTime.Now && (a.Status == "InProgress" || a.Status == "Overdue"),
                    DaysLeft = (a.DueDate - DateTime.Now).Days
                }).ToListAsync();

            return Json(assignments);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitWork(int assignmentId, int quantity)
        {
            int userId = GetCurrentUserId();

            var assignment = await _context.Assignments
                .Include(a => a.Product)
                .FirstOrDefaultAsync(a => a.Id == assignmentId && a.UserId == userId);

            if (assignment == null) return NotFound("Không tìm thấy đơn giao việc");
            
            // Kiểm tra nếu assignment đã hoàn tất
            if (assignment.Status == "Completed")
                return BadRequest("Đơn giao việc này đã hoàn tất. Bạn không thể nộp sản phẩm thêm.");
            
            // Không được nộp quá số lượng giao
            if (quantity <= 0 || quantity > assignment.QuantityAssigned)
                return BadRequest($"Số lượng nộp phải từ 1-{assignment.QuantityAssigned}");
            
            // Không được nộp quá tổng số lượng đã nộp + số nộp lần này  
            int totalSubmitted = await _context.Submissions
                .Where(s => s.AssignmentId == assignmentId)
                .SumAsync(s => (int?)s.QuantitySubmitted) ?? 0;
            
            if (totalSubmitted + quantity > assignment.QuantityAssigned)
                return BadRequest($"Tổng đã nộp: {totalSubmitted}. Còn có thể nộp: {assignment.QuantityAssigned - totalSubmitted}");

            // Lấy submission number (lần nộp thứ mấy)
            int submissionNumber = await _context.Submissions
                .Where(s => s.AssignmentId == assignmentId)
                .CountAsync() + 1;

            var submission = new Submission
            {
                AssignmentId = assignmentId,
                SubmissionNumber = submissionNumber,
                QuantitySubmitted = quantity,
                QuantityGood = 0, // Sẽ được setup khi Admin duyệt
                QuantityDefect = 0,
                SubmittedDate = DateTime.Now,
                Status = "Pending"
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            // Tính tiến độ
            int newTotal = totalSubmitted + quantity;
            int progressPercent = (newTotal * 100) / assignment.QuantityAssigned;

            return Ok(new { 
                success = true, 
                message = $"✓ Nộp {quantity} sản phẩm thành công (Lần {submissionNumber})",
                progressPercent,
                totalSubmitted = newTotal
            });
        }

        // Xem cong thuc (BOM) cua cac san pham duoc giao
        [HttpGet]
        public async Task<IActionResult> GetMyRecipes()
        {
            int userId = GetCurrentUserId();

            // Lay danh sach san pham ma user duoc giao
            var productIds = await _context.Assignments
                .Where(a => a.UserId == userId)
                .Select(a => a.ProductId)
                .Distinct()
                .ToListAsync();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    Materials = p.ProductMaterials.Select(pm => new
                    {
                        pm.Material.Name,
                        pm.QuantityRequired,
                        pm.Material.Unit
                    }).ToList()
                }).ToListAsync();

            return Json(products);
        }

        // Tien do ca nhan
        [HttpGet]
        public async Task<IActionResult> GetMyProgress()
        {
            int userId = GetCurrentUserId();

            var assignments = await _context.Assignments
                .Include(a => a.Product)
                .Include(a => a.Submissions)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AssignedDate)
                .ToListAsync();

            var assignmentData = assignments.Select(a => new
            {
                a.Id,
                ProductName = a.Product.Name,
                a.QuantityAssigned,
                a.CompletedQuantity,
                ProgressPercent = a.QuantityAssigned > 0 ? (a.CompletedQuantity * 100) / a.QuantityAssigned : 0,
                AssignedDate = a.AssignedDate.ToString("dd/MM/yyyy"),
                DueDate = a.DueDate.ToString("dd/MM/yyyy"),
                a.Status,
                IsOverdue = a.DueDate < DateTime.Now && (a.Status == "InProgress" || a.Status == "Overdue"),
                DaysLeft = (a.DueDate - DateTime.Now).Days,
                // Submission history
                Submissions = a.Submissions.OrderBy(s => s.SubmissionNumber).Select(s => new
                {
                    s.SubmissionNumber,
                    s.QuantitySubmitted,
                    s.QuantityGood,
                    s.QuantityDefect,
                    s.Status,
                    s.ReviewNote,
                    SubmittedDate = s.SubmittedDate.ToString("dd/MM/yyyy HH:mm")
                }).ToList(),
                ApprovedQuantity = a.Submissions.Where(s => s.Status == "Approved").Sum(s => (int?)s.QuantityGood) ?? 0,
                TotalSubmittedGood = a.Submissions.Sum(s => (int?)s.QuantityGood) ?? 0,
                TotalSubmittedDefect = a.Submissions.Sum(s => (int?)s.QuantityDefect) ?? 0
            }).ToList();

            var totalEarnings = await _context.Payments
                .Where(p => p.UserId == userId && p.Status == "Paid")
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            // Tính tổng tiền phạt (cả Active và Deducted để hiển thị tổng tiền phạt phải chịu)
            var totalPenalties = await _context.Penalties
                .Where(p => p.UserId == userId && (p.Status == "Active" || p.Status == "Deducted"))
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            return Json(new { assignments = assignmentData, totalEarnings, penalties = totalPenalties });
        }

        // Lấy thông báo không đọc
        [HttpGet]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            int userId = GetCurrentUserId();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            // Mark all as read
            foreach (var n in notifications)
            {
                n.IsRead = true;
            }
            await _context.SaveChangesAsync();

            return Json(notifications.Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                n.Type,
                CreatedDate = n.CreatedDate.ToString("dd/MM/yyyy HH:mm")
            }).ToList());
        }

        // Lấy danh sách phạt của user
        [HttpGet]
        public async Task<IActionResult> GetMyPenalties()
        {
            int userId = GetCurrentUserId();

            var penalties = await _context.Penalties
                .Include(p => p.Assignment).ThenInclude(a => a.Product)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedDate)
                .Select(p => new
                {
                    p.Id,
                    p.AssignmentId,
                    ProductName = p.Assignment.Product.Name,
                    p.Reason,
                    p.Amount,
                    p.Status,
                    CreatedDate = p.CreatedDate.ToString("dd/MM/yyyy"),
                    p.Note
                })
                .ToListAsync();

            return Json(penalties);
        }

        // Thanh toán phạt
        [HttpPost]
        public async Task<IActionResult> PayPenalty(int penaltyId)
        {
            int userId = GetCurrentUserId();

            var penalty = await _context.Penalties
                .Include(p => p.Assignment).ThenInclude(a => a.Product)
                .Include(p => p.Assignment).ThenInclude(a => a.Submissions)
                .Include(p => p.Assignment).ThenInclude(a => a.SampleOrder)
                .FirstOrDefaultAsync(p => p.Id == penaltyId && p.UserId == userId);

            if (penalty == null) return NotFound("Không tìm thấy khoản phạt");
            if (penalty.Status != "Active") return BadRequest("Khoản phạt này đã được thanh toán hoặy hủy bỏ");

            penalty.Status = "Deducted";
            penalty.PaidDate = DateTime.Now;

            // ========== TỰ ĐỘNG THA BỔNG LỖI ==========
            var assignment = penalty.Assignment;
            
            // Tính tổng số lỗi từ tất cả Submission được duyệt của Assignment này
            int totalDefectQuantity = assignment.Submissions
                .Where(s => s.Status == "Approved")
                .Sum(s => s.QuantityDefect);

            if (totalDefectQuantity > 0)
            {
                // Tạo Submission mới để "tha bổng" các lỗi
                int maxSubmissionNumber = assignment.Submissions.Max(s => (int?)s.SubmissionNumber) ?? 0;
                
                var bonusSubmission = new Submission
                {
                    AssignmentId = assignment.Id,
                    SubmittedDate = DateTime.Now,
                    SubmissionNumber = maxSubmissionNumber + 1,
                    QuantitySubmitted = totalDefectQuantity,
                    QuantityGood = totalDefectQuantity,  // Tất cả lỗi được coi là đạt
                    QuantityDefect = 0,
                    Status = "Approved",
                    ReviewNote = "Tự động tha bổng lỗi sau khi hộ dân đóng tiền phạt"
                };
                _context.Submissions.Add(bonusSubmission);
                await _context.SaveChangesAsync();  // Save để có SubmissionId
                
                // Cập nhật CompletedQuantity của Assignment
                assignment.CompletedQuantity += totalDefectQuantity;
                
                // Tạo Payment cho Submission "tha bổng"
                decimal bonusAmount = (decimal)totalDefectQuantity * assignment.Product.UnitPrice;
                var bonusPayment = new Payment
                {
                    UserId = assignment.UserId,
                    SubmissionId = bonusSubmission.Id,
                    Amount = bonusAmount,
                    PaymentDate = DateTime.Now,
                    Status = "Paid"
                };
                _context.Payments.Add(bonusPayment);
            }

            // Kiểm tra nếu tất cả penalties của assignment đã thanh toán
            var pendingPenalties = await _context.Penalties
                .Where(p => p.AssignmentId == assignment.Id && p.Status == "Active")
                .CountAsync();

            // Nếu không còn penalty chưa thanh toán, cập nhật assignment thành Completed
            if (pendingPenalties == 0)
            {
                assignment.Status = "Completed";
            }

            // ========== CẬP NHẬT SAMPLEORDER ==========
            if (assignment.SampleOrderId.HasValue)
            {
                var sampleOrder = await _context.SampleOrders
                    .Include(so => so.Assignments).ThenInclude(a => a.Submissions)
                    .FirstOrDefaultAsync(so => so.Id == assignment.SampleOrderId.Value);
                
                if (sampleOrder != null)
                {
                    // Cập nhật CompletedQuantity của SampleOrder
                    sampleOrder.CompletedQuantity = sampleOrder.Assignments
                        .SelectMany(a => a.Submissions.Where(s => s.Status == "Approved"))
                        .Sum(s => (int?)s.QuantityGood) ?? 0;
                    
                    // Kiểm tra nếu đơn hàng mẫu đã hoàn thành đủ số lượng
                    if (sampleOrder.CompletedQuantity >= sampleOrder.TotalQuantity)
                    {
                        sampleOrder.Status = "Completed";
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { 
                success = true, 
                message = $"✓ Đã thanh toán phạt {penalty.Amount.ToString("N0")} VND. {(totalDefectQuantity > 0 ? $" Tất cả {totalDefectQuantity} sản phẩm lỗi đã được coi là hoàn thành." : "")}",
                assignmentCompleted = pendingPenalties == 0
            });
        }
    }
}
