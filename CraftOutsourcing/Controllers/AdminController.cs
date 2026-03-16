using CraftOutsourcing.Data;
using CraftOutsourcing.Models;
using CraftOutsourcing.Models.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CraftOutsourcing.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // PAGE VIEWS
        // ==========================================

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync(u => u.RoleId == 2 && u.IsActive);
            ViewBag.TotalMaterials = await _context.Materials.CountAsync();
            ViewBag.TotalAssignments = await _context.Assignments.CountAsync();
            ViewBag.TotalPendingSubmissions = await _context.Submissions.CountAsync(s => s.Status == "Pending");
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalSampleOrders = await _context.SampleOrders.CountAsync(so => so.Status != "Cancelled");
            ViewBag.TotalPenalties = await _context.Penalties.CountAsync(p => p.Status == "Active");

            ViewBag.TotalPayments = await _context.Payments.Where(p => p.Status == "Paid").SumAsync(p => (decimal?)p.Amount) ?? 0;

            // Lợi nhuận thực tế = (Giá Bán - Giá Thành/SP) × Số Lượng Hoàn Thành Tốt
            var sampleOrders = await _context.SampleOrders
                .Include(so => so.Product).ThenInclude(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
                .Include(so => so.Assignments).ThenInclude(a => a.Submissions)
                .ToListAsync();

            decimal actualProfit = sampleOrders.Sum(so => {
                var costPerUnit = so.Product.UnitPrice + so.Product.ProductMaterials.Sum(pm => (decimal)pm.QuantityRequired * pm.Material.UnitPrice);
                var completedQty = so.Assignments
                    .SelectMany(a => a.Submissions)
                    .Where(s => s.Status == "Approved")
                    .Sum(s => s.QuantityGood);
                return (so.SellingPrice - costPerUnit) * completedQty;
            });

            ViewBag.SalesRevenue = actualProfit;

            // Đơn quá hạn
            ViewBag.OverdueAssignments = await _context.Assignments
                .CountAsync(a => a.DueDate < DateTime.Now && a.Status == "InProgress");

            // Cảnh báo tồn kho thấp
            ViewBag.LowStockCount = await _context.Materials.CountAsync(m => m.StockQuantity <= m.MinStock);

            // Admin cho duyet
            ViewBag.PendingAdmins = await _context.Users.CountAsync(u => u.RoleId == 1 && !u.IsApproved);

            return View();
        }

        public IActionResult Users() => View();
        public IActionResult Materials()
        {
            ViewBag.Units = UnitType.GetAllUnits();
            return View();
        }
        public IActionResult Assignments() => View();
        public IActionResult Submissions() => View();
        public IActionResult Products() => View();
        public IActionResult SampleOrders() => View();
        public IActionResult Inventory() => View();
        public IActionResult CostEstimation() => View();
        public IActionResult Payments() => View();
        public IActionResult Penalties() => View();
        public IActionResult AdminAccounts() => View();
        public IActionResult FinishedProducts() => View();

        // ==========================================
        // DTOs
        // ==========================================

        public class ProductMaterialDto
        {
            public int MaterialId { get; set; }
            public double QuantityRequired { get; set; }
        }

        public class ProductCreateDto
        {
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public decimal UnitPrice { get; set; }
            public List<ProductMaterialDto> Materials { get; set; } = new();
        }

        public class SampleOrderDto
        {
            public string OrderCode { get; set; } = null!;
            public string CustomerName { get; set; } = null!;
            public string? Description { get; set; }
            public int ProductId { get; set; }
            public int TotalQuantity { get; set; }
            public decimal SellingPrice { get; set; }
            public DateTime? TargetDate { get; set; }
        }

        public class MaterialDto
        {
            public string Name { get; set; } = null!;
            public string Unit { get; set; } = null!;
            public decimal UnitPrice { get; set; }
            public double MinStock { get; set; }
        }

        // ==========================================
        // API: USERS (Ho dan)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Payments)
                .Include(u => u.Penalties)
                .Where(u => u.RoleId == 2)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Username,
                    u.Phone,
                    u.Address,
                    u.IsActive,
                    CreatedAt = u.CreatedAt.ToString("dd/MM/yyyy"),
                    TotalAssignments = u.Assignments.Count,
                    CompletedAssignments = u.Assignments.Count(a => a.Status == "Completed"),
                    TotalEarnings = u.Payments.Where(p => p.Status == "Paid").Sum(p => (decimal?)p.Amount) ?? 0,
                    TotalPenalties = u.Penalties.Where(p => p.Status == "Active" || p.Status == "Deducted").Sum(p => (decimal?)p.Amount) ?? 0,
                    NetIncome = (u.Payments.Where(p => p.Status == "Paid").Sum(p => (decimal?)p.Amount) ?? 0) - (u.Penalties.Where(p => p.Status == "Active" || p.Status == "Deducted").Sum(p => (decimal?)p.Amount) ?? 0)
                })
                .ToListAsync();
            return Json(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserActive(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, isActive = user.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Check if user has any in-progress or pending assignments
            var hasActiveAssignments = await _context.Assignments
                .AnyAsync(a => a.UserId == id && 
                    (a.Status == "InProgress" || a.Status == "PendingVerification" || a.Status == "Pending" || a.Status == "Overdue"));
            
            if (hasActiveAssignments)
                return BadRequest("Hộ gia công đang trong quá trình làm việc, không thể xóa được.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Đã xóa hộ gia công và tài khoản đăng nhập." });
        }

        // ==========================================
        // API: ADMIN ACCOUNTS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAdminAccounts()
        {
            var admins = await _context.Users
                .Where(u => u.RoleId == 1)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Username,
                    u.Phone,
                    u.IsApproved,
                    u.IsActive,
                    u.Balance,
                    CreatedAt = u.CreatedAt.ToString("dd/MM/yyyy")
                }).ToListAsync();
            return Json(admins);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveAdmin(int id)
        {
            var admin = await _context.Users.FindAsync(id);
            if (admin == null || admin.RoleId != 1) return NotFound();
            admin.IsApproved = true;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Đã phê duyệt tài khoản Admin." });
        }

        [HttpPost]
        public async Task<IActionResult> RejectAdmin(int id)
        {
            var admin = await _context.Users.FindAsync(id);
            if (admin == null || admin.RoleId != 1) return NotFound();
            _context.Users.Remove(admin);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Đã từ chối và xóa tài khoản." });
        }

        // ==========================================
        // API: MATERIALS (Vat tu)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetMaterials()
        {
            var materials = await _context.Materials
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Unit,
                    m.StockQuantity,
                    m.UnitPrice,
                    m.MinStock,
                    IsLowStock = m.StockQuantity <= m.MinStock
                }).ToListAsync();
            return Json(materials);
        }

        [HttpPost]
        public async Task<IActionResult> AddMaterial([FromBody] MaterialDto model)
        {
            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.Unit))
                return BadRequest("Thieu thong tin vat tu");

            var material = new Material
            {
                Name = model.Name,
                Unit = model.Unit,
                UnitPrice = model.UnitPrice,
                MinStock = model.MinStock,
                StockQuantity = 0
            };
            _context.Materials.Add(material);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Them vat tu thanh cong!" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMaterial(int id, [FromBody] MaterialDto model)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null) return NotFound();
            material.Name = model.Name;
            material.Unit = model.Unit;
            material.UnitPrice = model.UnitPrice;
            material.MinStock = model.MinStock;
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null) return NotFound();
            var inUse = await _context.ProductMaterials.AnyAsync(pm => pm.MaterialId == id);
            if (inUse) return BadRequest("Vật tư đang được sử dụng trong sản phẩm, không thể xóa.");
            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ImportMaterial(int id, [FromBody] double quantity)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null) return NotFound();

            material.StockQuantity += quantity;

            _context.MaterialTransactions.Add(new MaterialTransaction
            {
                MaterialId = id,
                TransactionType = "Import",
                Quantity = quantity,
                TransactionDate = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return Ok(new { success = true, newStock = material.StockQuantity });
        }

        // ==========================================
        // API: INVENTORY (Tồn kho)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetInventoryReport()
        {
            var materials = await _context.Materials
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Unit,
                    m.StockQuantity,
                    m.UnitPrice,
                    m.MinStock,
                    IsLowStock = m.StockQuantity <= m.MinStock,
                    StockValue = (decimal)m.StockQuantity * m.UnitPrice
                }).ToListAsync();

            var transactions = await _context.MaterialTransactions
                .Include(t => t.Material)
                .OrderByDescending(t => t.TransactionDate)
                .Take(50)
                .Select(t => new
                {
                    t.Id,
                    MaterialName = t.Material.Name,
                    t.TransactionType,
                    t.Quantity,
                    Unit = t.Material.Unit,
                    TransactionDate = t.TransactionDate.ToString("dd/MM/yyyy HH:mm"),
                    t.ReferenceId
                }).ToListAsync();

            var finishedProducts = await _context.Products
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.FinishedStock,
                    p.UnitPrice
                }).ToListAsync();

            return Json(new { materials, transactions, finishedProducts });
        }

        // ==========================================
        // API: PRODUCTS (San pham + Quy trinh BOM)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.ProductMaterials)
                .ThenInclude(pm => pm.Material)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.UnitPrice,
                    p.FinishedStock,
                    Materials = p.ProductMaterials.Select(pm => new
                    {
                        pm.Material.Id,
                        pm.Material.Name,
                        pm.Material.Unit,
                        pm.QuantityRequired,
                        pm.Material.UnitPrice
                    }).ToList(),
                    MaterialCost = p.ProductMaterials.Sum(pm => (decimal)pm.QuantityRequired * pm.Material.UnitPrice),
                    TotalCostPerUnit = p.ProductMaterials.Sum(pm => (decimal)pm.QuantityRequired * pm.Material.UnitPrice) + p.UnitPrice
                }).ToListAsync();
            return Json(products);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] ProductCreateDto model)
        {
            if (string.IsNullOrEmpty(model.Name)) return BadRequest("Tên sản phẩm không được trống");
            if (model.Materials == null || !model.Materials.Any()) return BadRequest("Vui long chon it nhat 1 nguyen lieu");

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                UnitPrice = model.UnitPrice
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            foreach (var m in model.Materials)
            {
                _context.ProductMaterials.Add(new ProductMaterial
                {
                    ProductId = product.Id,
                    MaterialId = m.MaterialId,
                    QuantityRequired = m.QuantityRequired
                });
            }
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Them san pham thanh cong!" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct([FromBody] ProductCreateDto model, int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Name = model.Name;
            product.Description = model.Description;
            product.UnitPrice = model.UnitPrice;

            // Xoa dinh muc cu, them moi
            var oldMaterials = await _context.ProductMaterials.Where(pm => pm.ProductId == id).ToListAsync();
            _context.ProductMaterials.RemoveRange(oldMaterials);

            foreach (var m in model.Materials)
            {
                _context.ProductMaterials.Add(new ProductMaterial
                {
                    ProductId = id,
                    MaterialId = m.MaterialId,
                    QuantityRequired = m.QuantityRequired
                });
            }
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Cap nhat san pham thanh cong!" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            var inUse = await _context.Assignments.AnyAsync(a => a.ProductId == id);
            if (inUse) return BadRequest("San pham da co don giao viec, khong the xoa.");
            var mats = await _context.ProductMaterials.Where(pm => pm.ProductId == id).ToListAsync();
            _context.ProductMaterials.RemoveRange(mats);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // ==========================================
        // API: SAMPLE ORDERS (Don hang mau)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetSampleOrders()
        {
            var orders = await _context.SampleOrders
                .Include(so => so.Product)
                .Include(so => so.Assignments).ThenInclude(a => a.Submissions)
                .OrderByDescending(so => so.CreatedDate)
                .ToListAsync();
                
            var orderList = orders.Select(so => {
                // Tính toán lại CompletedQuantity từ tất cả Submissions được duyệt
                int completedQty = so.Assignments
                    .SelectMany(a => a.Submissions.Where(s => s.Status == "Approved"))
                    .Sum(s => (int?)s.QuantityGood) ?? 0;
                    
                return new
                {
                    so.Id,
                    so.OrderCode,
                    so.CustomerName,
                    so.Description,
                    ProductName = so.Product.Name,
                    so.ProductId,
                    so.TotalQuantity,
                    CompletedQuantity = completedQty,
                    so.EstimatedCost,
                    so.ActualCost,
                    so.SellingPrice,
                    so.Status,
                    CreatedDate = so.CreatedDate.ToString("dd/MM/yyyy"),
                    TargetDate = so.TargetDate.HasValue ? so.TargetDate.Value.ToString("dd/MM/yyyy") : "",
                    ProgressPercent = so.TotalQuantity > 0 ? (int)((double)completedQty / so.TotalQuantity * 100) : 0,
                    AssignedQuantity = so.Assignments.Sum(a => a.QuantityAssigned)
                };
            }).ToList();
            return Json(orderList);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSampleOrder([FromBody] SampleOrderDto model)
        {
            if (string.IsNullOrEmpty(model.CustomerName))
                return BadRequest("Thiếu tên khách hàng.");

            // 1. TỰ ĐỘNG SINH MÃ ĐƠN HÀNG NẾU ĐỂ TRỐNG
            string orderCode = model.OrderCode?.Trim();
            
            if (string.IsNullOrEmpty(orderCode))
            {
                // Auto-generate: SO + số thứ tự
                var lastOrder = await _context.SampleOrders
                    .OrderByDescending(so => so.Id)
                    .FirstOrDefaultAsync();
                
                int nextNumber = (lastOrder?.Id ?? 0) + 1;
                orderCode = $"SO{nextNumber:D4}";
            }
            
            // 2. KIỂM TRA TRÙNG MÃ ĐƠN
            var existingOrder = await _context.SampleOrders
                .FirstOrDefaultAsync(so => so.OrderCode == orderCode);
            
            if (existingOrder != null)
                return BadRequest($"Mã đơn '{orderCode}' đã tồn tại. Vui lòng sử dụng mã khác.");

            // Tính giá thành dự kiến
            var product = await _context.Products
                .Include(p => p.ProductMaterials)
                .ThenInclude(pm => pm.Material)
                .FirstOrDefaultAsync(p => p.Id == model.ProductId);

            if (product == null) return BadRequest("Sản phẩm không tồn tại.");

            decimal materialCost = product.ProductMaterials.Sum(pm => (decimal)pm.QuantityRequired * pm.Material.UnitPrice);
            decimal laborCost = product.UnitPrice;
            decimal estimatedCost = (materialCost + laborCost) * model.TotalQuantity;

            var order = new SampleOrder
            {
                OrderCode = orderCode,
                CustomerName = model.CustomerName,
                Description = model.Description,
                ProductId = model.ProductId,
                TotalQuantity = model.TotalQuantity,
                SellingPrice = model.SellingPrice,
                TargetDate = model.TargetDate,
                EstimatedCost = estimatedCost,
                Status = "Draft"
            };

            _context.SampleOrders.Add(order);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Tạo đơn hàng mẫu thành công!", orderCode, estimatedCost });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSampleOrderStatus(int id, [FromBody] string status)
        {
            var order = await _context.SampleOrders.FindAsync(id);
            if (order == null) return NotFound();
            order.Status = status;
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // ==========================================
        // API: ASSIGNMENTS (Giao viec)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAssignments()
        {
            var assignments = await _context.Assignments
                .Include(a => a.User)
                .Include(a => a.Product)
                .Include(a => a.SampleOrder)
                .OrderByDescending(a => a.AssignedDate)
                .Select(a => new
                {
                    a.Id,
                    User = a.User.FullName,
                    UserId = a.UserId,
                    Product = a.Product.Name,
                    ProductId = a.ProductId,
                    a.QuantityAssigned,
                    AssignedDate = a.AssignedDate.ToString("dd/MM/yyyy"),
                    DueDate = a.DueDate.ToString("dd/MM/yyyy"),
                    DueDateRaw = a.DueDate,
                    a.Status,
                    SampleOrderCode = a.SampleOrder != null ? a.SampleOrder.OrderCode : "",
                    IsOverdue = a.DueDate < DateTime.Now && a.Status == "InProgress"
                }).ToListAsync();
            return Json(assignments);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAssignment(int userId, int productId, int quantity, DateTime dueDate, int? sampleOrderId)
        {
            // 1. AUTO-FILL SỐ LƯỢNG TỪ ĐƠNHÀNG NẾU CÒN THIẾU
            if (quantity <= 0 && sampleOrderId.HasValue)
            {
                var sampleOrder = await _context.SampleOrders.FindAsync(sampleOrderId.Value);
                if (sampleOrder != null)
                {
                    // Lấy tổng số lượng đã giao cho đơn này
                    int assignedQty = await _context.Assignments
                        .Where(a => a.SampleOrderId == sampleOrderId)
                        .SumAsync(a => a.QuantityAssigned);
                    
                    // Auto-fill = TotalQuantity - đã giao
                    quantity = Math.Max(0, sampleOrder.TotalQuantity - assignedQty);
                }
            }

            if (quantity <= 0) return BadRequest("Số lượng không hợp lệ hoặc đơn hàng đã giao đủ.");

            // Kiểm tra tồn kho
            var requiredMaterials = await _context.ProductMaterials
                .Where(pm => pm.ProductId == productId)
                .Include(pm => pm.Material)
                .ToListAsync();

            if (!requiredMaterials.Any())
                return BadRequest("Sản phẩm chưa có định mức nguyên liệu (BOM).");

            foreach (var req in requiredMaterials)
            {
                double totalNeeded = req.QuantityRequired * quantity;
                if (req.Material.StockQuantity < totalNeeded)
                    return BadRequest($"Không đủ '{req.Material.Name}'. Cần: {totalNeeded} {req.Material.Unit}, Tồn: {req.Material.StockQuantity}");
            }

            // Tạo đơn giao việc
            var assignment = new Assignment
            {
                UserId = userId,
                ProductId = productId,
                QuantityAssigned = quantity,
                AssignedDate = DateTime.Now,
                DueDate = dueDate,
                Status = "InProgress",
                SampleOrderId = sampleOrderId,
                CompletedQuantity = 0
            };
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Trừ kho và lưu lịch sử
            foreach (var req in requiredMaterials)
            {
                double totalNeeded = req.QuantityRequired * quantity;

                var material = await _context.Materials.FindAsync(req.MaterialId);
                material!.StockQuantity -= totalNeeded;

                _context.AssignmentMaterials.Add(new AssignmentMaterial
                {
                    AssignmentId = assignment.Id,
                    MaterialId = req.MaterialId,
                    QuantityGiven = totalNeeded
                });

                _context.MaterialTransactions.Add(new MaterialTransaction
                {
                    MaterialId = req.MaterialId,
                    TransactionType = "Export",
                    Quantity = totalNeeded,
                    TransactionDate = DateTime.Now,
                    ReferenceId = assignment.Id
                });
            }

            // Cập nhật SampleOrder status
            if (sampleOrderId.HasValue)
            {
                var sampleOrder = await _context.SampleOrders.FindAsync(sampleOrderId.Value);
                if (sampleOrder != null && sampleOrder.Status == "Draft")
                {
                    sampleOrder.Status = "InProduction";
                }
            }

            // GỬI THÔNG BÁO CHO HỘ DÂN
            var notification = new Notification
            {
                UserId = userId,
                Title = "🎉 Bạn Vừa Nhận Giao Việc Mới!",
                Message = $"Bạn có {quantity} sản phẩm {await _context.Products.AsNoTracking().Where(p => p.Id == productId).Select(p => p.Name).FirstOrDefaultAsync()} cần hoàn thành trước ngày {dueDate:dd/MM/yyyy}",
                Type = "success",
                CreatedDate = DateTime.Now
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Đã giao việc và tự động trừ kho nguyên liệu.", quantity });
        }

        [HttpGet]
        public async Task<IActionResult> GetAssignmentDetails(int assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.User)
                .Include(a => a.Product).ThenInclude(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
                .Include(a => a.SampleOrder)
                .Include(a => a.Submissions)
                .Include(a => a.Penalties)
                .Include(a => a.AssignmentMaterials).ThenInclude(am => am.Material)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null) return NotFound("Không tìm thấy giao việc");

            var submissionDetails = assignment.Submissions.Select(s => new
            {
                s.Id,
                s.SubmissionNumber,
                s.QuantitySubmitted,
                s.QuantityGood,
                s.QuantityDefect,
                s.Status,
                s.ReviewNote,
                SubmittedDate = s.SubmittedDate.ToString("dd/MM/yyyy HH:mm")
            }).ToList();

            var penaltyDetails = assignment.Penalties.Select(p => new
            {
                p.Id,
                p.Reason,
                p.Amount,
                p.Status,
                p.DefectiveQuantity,
                p.Note,
                CreatedDate = p.CreatedDate.ToString("dd/MM/yyyy"),
                PaidDate = p.PaidDate.HasValue ? p.PaidDate.Value.ToString("dd/MM/yyyy HH:mm") : ""
            }).ToList();

            int totalSubmitted = assignment.Submissions.Sum(s => s.QuantitySubmitted);
            int totalGood = assignment.Submissions.Where(s => s.Status == "Approved").Sum(s => s.QuantityGood);
            int totalDefect = assignment.Submissions.Where(s => s.Status == "Approved").Sum(s => s.QuantityDefect);
            int pendingApproval = assignment.Submissions.Where(s => s.Status == "Pending").Sum(s => s.QuantitySubmitted);

            return Json(new
            {
                assignment = new
                {
                    assignment.Id,
                    UserName = assignment.User.FullName,
                    ProductName = assignment.Product.Name,
                    assignment.QuantityAssigned,
                    assignment.CompletedQuantity,
                    ProgressPercent = assignment.QuantityAssigned > 0 ? (assignment.CompletedQuantity * 100) / assignment.QuantityAssigned : 0,
                    AssignedDate = assignment.AssignedDate.ToString("dd/MM/yyyy"),
                    DueDate = assignment.DueDate.ToString("dd/MM/yyyy"),
                    assignment.Status,
                    IsOverdue = assignment.DueDate < DateTime.Now && assignment.Status == "InProgress",
                    SampleOrderCode = assignment.SampleOrder != null ? assignment.SampleOrder.OrderCode : "",
                    // Submission summary
                    TotalSubmitted = totalSubmitted,
                    TotalGood = totalGood,
                    TotalDefect = totalDefect,
                    PendingApproval = pendingApproval,
                    RemainingToSubmit = Math.Max(0, assignment.QuantityAssigned - totalSubmitted),
                    Submissions = submissionDetails,
                    // Penalty summary
                    ActivePenalties = assignment.Penalties.Where(p => p.Status == "Active").Sum(p => p.Amount),
                    ResolvePenalties = assignment.Penalties.Where(p => p.Status == "Resolved").Sum(p => p.Amount),
                    Penalties = penaltyDetails
                }
            });
        }

        // ==========================================
        // API: SUBMISSIONS (Nop hang + KCS)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetSubmissions()
        {
            var submissions = await _context.Submissions
                .Include(s => s.Assignment).ThenInclude(a => a.User)
                .Include(s => s.Assignment).ThenInclude(a => a.Product)
                .OrderByDescending(s => s.SubmittedDate)
                .Select(s => new
                {
                    s.Id,
                    s.AssignmentId,
                    User = s.Assignment.User.FullName,
                    Product = s.Assignment.Product.Name,
                    s.QuantitySubmitted,
                    s.QuantityGood,
                    s.QuantityDefect,
                    QuantityAssigned = s.Assignment.QuantityAssigned,
                    SubmittedDate = s.SubmittedDate.ToString("dd/MM/yyyy HH:mm"),
                    s.Status,
                    s.SubmissionNumber,
                    UnitPrice = s.Assignment.Product.UnitPrice,
                    SuccessRate = s.QuantityGood + s.QuantityDefect > 0
                        ? Math.Round((double)s.QuantityGood / (s.QuantityGood + s.QuantityDefect) * 100, 1) : 0
                }).ToListAsync();
            return Json(submissions);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveSubmission(int id, int good, int defect, string? reviewNote)
        {
            try
            {
                var submission = await _context.Submissions
                    .Include(s => s.Assignment).ThenInclude(a => a.Product).ThenInclude(p => p.ProductMaterials)
                    .Include(s => s.Assignment).ThenInclude(a => a.SampleOrder)
                    .Include(s => s.Assignment).ThenInclude(a => a.Submissions)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (submission == null) return NotFound(new { success = false, message = "Không tìm thấy submission" });
                if (submission.Status == "Approved") return BadRequest(new { success = false, message = "Submission đã duyệt rồi!" });
                if (good + defect != submission.QuantitySubmitted) 
                    return BadRequest(new { success = false, message = $"Tổng (Đạt + Lỗi) = {good + defect} != số nộp {submission.QuantitySubmitted}" });

                // Cập nhật chi tiết submission
                submission.QuantityGood = good;
                submission.QuantityDefect = defect;
                submission.ReviewNote = reviewNote;
                submission.Status = "Approved";

                // Cập nhật CompletedQuantity của Assignment
                submission.Assignment.CompletedQuantity += good;

                // Kiểm tra nếu Assignment đã hoàn thành hết (CompletedQuantity >= QuantityAssigned)
                bool assignmentComplete = submission.Assignment.CompletedQuantity >= submission.Assignment.QuantityAssigned;
                if (assignmentComplete)
                {
                    submission.Assignment.Status = "Completed";
                }

                // Tính tiền trả công dựa trên số lượng đạt
                decimal amount = (decimal)good * submission.Assignment.Product.UnitPrice;

                var payment = new Payment
                {
                    UserId = submission.Assignment.UserId,
                    SubmissionId = submission.Id,
                    Amount = amount,
                    PaymentDate = DateTime.Now,
                    Status = "Paid"
                };
                _context.Payments.Add(payment);

                // TẠO PENALTY KHI CÓ SẢN PHẨM LỖI
                if (defect > 0)
                {
                    // Tinh phạt = số lỗi × (giá nguyên liệu mỗi sản phẩm)
                    decimal materialCostPerUnit = 0;
                    if (submission.Assignment.Product.ProductMaterials.Any())
                    {
                        foreach (var pm in submission.Assignment.Product.ProductMaterials)
                        {
                            var material = await _context.Materials.FindAsync(pm.MaterialId);
                            if (material != null)
                            {
                                materialCostPerUnit += (decimal)pm.QuantityRequired * material.UnitPrice;
                            }
                        }
                    }
                    else
                    {
                        // Nếu không có ProductMaterials, dùng UnitPrice / 2 làm chi phí tương đương
                        materialCostPerUnit = submission.Assignment.Product.UnitPrice / 2;
                    }
                    
                    decimal defectCost = (decimal)defect * materialCostPerUnit;
                    
                    var penalty = new Penalty
                    {
                        AssignmentId = submission.AssignmentId,
                        UserId = submission.Assignment.UserId,
                        Reason = "QualityFail",
                        Amount = defectCost,
                        Note = $"{defect} sản phẩm lỗi từ submission lần {submission.SubmissionNumber}",
                        Status = "Active",
                        CreatedDate = DateTime.Now,
                        DefectiveQuantity = defect,
                        SubmissionId = submission.Id
                    };
                    _context.Penalties.Add(penalty);
                }

                // Cộng thành phẩm vào kho
                var product = await _context.Products.FindAsync(submission.Assignment.ProductId);
                if (product != null)
                {
                    product.FinishedStock += good;
                }

                // CẬP NHẬT SAMPLEORDER & AUTO TẠO PENALTY KHI HOÀN THÀNH
                if (submission.Assignment.SampleOrderId.HasValue)
                {
                    var sampleOrder = await _context.SampleOrders
                        .Include(so => so.Assignments).ThenInclude(a => a.Submissions)
                        .FirstOrDefaultAsync(so => so.Id == submission.Assignment.SampleOrderId.Value);
                    
                    if (sampleOrder != null)
                    {
                        sampleOrder.CompletedQuantity = sampleOrder.Assignments
                            .SelectMany(a => a.Submissions.Where(s => s.Status == "Approved"))
                            .Sum(s => (int?)s.QuantityGood) ?? 0;
                        
                        sampleOrder.ActualCost += amount;

                        // Nếu đã hoàn thành đủ số lượng
                        if (sampleOrder.CompletedQuantity >= sampleOrder.TotalQuantity)
                        {
                            sampleOrder.Status = "Completed";
                            
                            // TỰ ĐỘNG GỬI PHẠT CHO HỘ DÂN DỰA TRÊN TỔNG SỐ LỖI CỦA SAMPLE ORDER
                            int totalDefect = sampleOrder.Assignments
                                .SelectMany(a => a.Submissions.Where(s => s.Status == "Approved"))
                                .Sum(s => (int?)s.QuantityDefect) ?? 0;
                            
                            if (totalDefect > 0)
                            {
                                decimal totalDefectCost = (decimal)totalDefect * (submission.Assignment.Product.UnitPrice / submission.Assignment.QuantityAssigned);
                                
                                var sampleOrderPenalty = new Penalty
                                {
                                    AssignmentId = submission.AssignmentId,
                                    UserId = submission.Assignment.UserId,
                                    Reason = "QualityFail",
                                    Amount = totalDefectCost,
                                    Note = $"Phạt cho {totalDefect} lỗi của đơn hàng {sampleOrder.OrderCode}",
                                    Status = "Active",
                                    CreatedDate = DateTime.Now
                                };
                                _context.Penalties.Add(sampleOrderPenalty);
                            }

                            // Cộng lợi nhuận cho Admin Chính
                            var mainAdmin = await _context.Users
                                .Where(u => u.RoleId == 1 && u.IsActive)
                                .OrderBy(u => u.Id)
                                .FirstOrDefaultAsync();
                            
                            if (mainAdmin != null)
                            {
                                decimal profit = sampleOrder.SellingPrice * sampleOrder.CompletedQuantity - sampleOrder.ActualCost;
                                if (profit > 0)
                                {
                                    mainAdmin.Balance += profit;
                                }
                            }
                        }
                    }
                }

                // GỬI THÔNG BÁO CHO HỘ DÂN KHI DUYỆT THÀNH PHẨM
                var approvalNotification = new Notification
                {
                    UserId = submission.Assignment.UserId,
                    Title = "✓ Nộp Hàng Được Phê Duyệt!",
                    Message = $"Admin đã duyệt {good} sản phẩm đạt. Tiền công: {amount:N0} VND" + (defect > 0 ? $". Bạn có {defect} sản phẩm lỗi bị phạt." : ""),
                    Type = good > 0 ? "success" : "warning",
                    CreatedDate = DateTime.Now
                };
                _context.Notifications.Add(approvalNotification);

                // GỬI THÔNG BÁO CẬP NHẬT TIẾN ĐỘ
                if (assignmentComplete)
                {
                    var completionNotification = new Notification
                    {
                        UserId = submission.Assignment.UserId,
                        Title = "🎉 Đơn Giao Việc Hoàn Tất!",
                        Message = $"Bạn đã hoàn thành đơn giao việc. Admin sẽ xuất kho và giao hàng tiếp theo.",
                        Type = "success",
                        CreatedDate = DateTime.Now
                    };
                    _context.Notifications.Add(completionNotification);
                }

                await _context.SaveChangesAsync();
                return Ok(new { 
                    success = true, 
                    amount, 
                    message = $"✓ Duyệt thành công. Đạt: {good}, Lỗi: {defect}. Trả công: {amount:N0} VND",
                    assignmentComplete
                });
            }
            catch (DbUpdateException dbEx)
            {
                var errorMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                return StatusCode(500, new { success = false, message = $"Lỗi cơ sở dữ liệu: {errorMsg}", error = dbEx.InnerException?.InnerException?.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi server: {ex.Message}", error = ex.InnerException?.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RejectSubmission(int id, [FromBody] string? reason)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null) return NotFound();
            submission.Status = "Rejected";
            submission.Assignment.Status = "InProgress"; // Cho lam lai
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Da tu choi. Ho dan co the nop lai." });
        }

        // ==========================================
        // API: MATERIAL REQUESTS (Yêu cầu thêm NL)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetMaterialRequests()
        {
            var requests = await _context.MaterialRequests
                .Include(mr => mr.Assignment).ThenInclude(a => a.User)
                .Include(mr => mr.Assignment).ThenInclude(a => a.Product)
                .Include(mr => mr.Material)
                .OrderByDescending(mr => mr.CreatedDate)
                .Select(mr => new
                {
                    mr.Id,
                    mr.AssignmentId,
                    User = mr.Assignment.User.FullName,
                    Product = mr.Assignment.Product.Name,
                    Material = mr.Material.Name,
                    mr.QuantityRequested,
                    Unit = mr.Material.Unit,
                    CurrentStock = mr.Material.StockQuantity,
                    mr.Status,
                    Reason = mr.Reason,
                    CreatedDate = mr.CreatedDate.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            return Json(requests);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveMaterialRequest(int id)
        {
            var request = await _context.MaterialRequests
                .Include(mr => mr.Material)
                .Include(mr => mr.Assignment)
                .FirstOrDefaultAsync(mr => mr.Id == id);

            if (request == null) return NotFound("Không tìm thấy yêu cầu");
            if (request.Status != "Pending") return BadRequest("Yêu cầu này đã được xử lý rồi");

            var material = request.Material;

            // Kiểm tra tồn kho
            if (material.StockQuantity < request.QuantityRequested)
                return BadRequest($"Không đủ {material.Name}. Tồn: {material.StockQuantity}, Cần: {request.QuantityRequested}");

            // Trừ kho
            material.StockQuantity -= request.QuantityRequested;

            // Cập nhật trạng thái
            request.Status = "Approved";
            request.ApprovedDate = DateTime.Now;

            // Lưu lịch sử giao dịch
            _context.MaterialTransactions.Add(new MaterialTransaction
            {
                MaterialId = request.MaterialId,
                TransactionType = "Export",
                Quantity = request.QuantityRequested,
                TransactionDate = DateTime.Now,
                ReferenceId = request.AssignmentId
            });

            await _context.SaveChangesAsync();

            return Ok(new { 
                success = true, 
                message = $"✓ Cấp {request.QuantityRequested} {material.Unit} {material.Name} cho hộ dân {request.Assignment.User.FullName}" 
            });
        }

        [HttpPost]
        public async Task<IActionResult> RejectMaterialRequest(int id, string? reason)
        {
            var request = await _context.MaterialRequests
                .FirstOrDefaultAsync(mr => mr.Id == id);

            if (request == null) return NotFound();
            if (request.Status != "Pending") return BadRequest("Yêu cầu này đã được xử lý");

            request.Status = "Rejected";
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "✓ Từ chối yêu cầu cấp nguyên liệu" });
        }

        // ==========================================
        // API: COST ESTIMATION (Du tinh gia thanh)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetCostEstimation()
        {
            var products = await _context.Products
                .Include(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    LaborCostPerUnit = p.UnitPrice,
                    MaterialCostPerUnit = p.ProductMaterials.Sum(pm => (decimal)pm.QuantityRequired * pm.Material.UnitPrice),
                    TotalCostPerUnit = p.UnitPrice + p.ProductMaterials.Sum(pm => (decimal)pm.QuantityRequired * pm.Material.UnitPrice),
                    MaterialBreakdown = p.ProductMaterials.Select(pm => new
                    {
                        pm.Material.Name,
                        pm.QuantityRequired,
                        pm.Material.Unit,
                        pm.Material.UnitPrice,
                        Cost = (decimal)pm.QuantityRequired * pm.Material.UnitPrice
                    }).ToList()
                }).ToListAsync();

            var sampleOrders = await _context.SampleOrders
                .Include(so => so.Product).ThenInclude(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
                .Include(so => so.Assignments).ThenInclude(a => a.Submissions)
                .Where(so => so.Status != "Cancelled")
                .Select(so => new
                {
                    so.Id,
                    so.OrderCode,
                    so.CustomerName,
                    ProductName = so.Product.Name,
                    so.TotalQuantity,
                    // Tính Giá Thành/SP = Tiền Công + Chi Phí Nguyên Liệu
                    TotalCostPerUnit = so.Product.UnitPrice + so.Product.ProductMaterials.Sum(pm => (decimal)pm.QuantityRequired * pm.Material.UnitPrice),
                    // Tính số lượng hoàn thành tốt từ submissions (chỉ Approved)
                    QuantityGood = so.Assignments.SelectMany(a => a.Submissions).Where(s => s.Status == "Approved").Sum(s => s.QuantityGood),
                    so.EstimatedCost,
                    so.ActualCost,
                    so.SellingPrice,
                    // Giả định EstimatedProfit = (Giá Bán - Giá Thành/SP) × Tổng Số Lượng
                    EstimatedProfit = (so.SellingPrice - (so.Product.UnitPrice + so.Product.ProductMaterials.Sum(pm => (decimal)pm.QuantityRequired * pm.Material.UnitPrice))) * so.TotalQuantity,
                    // ActualProfit = (Giá Bán - Giá Thành/SP) × Số Lượng Hoàn Thành Tốt
                    ActualProfit = (so.SellingPrice - (so.Product.UnitPrice + so.Product.ProductMaterials.Sum(pm => (decimal)pm.QuantityRequired * pm.Material.UnitPrice))) * so.Assignments.SelectMany(a => a.Submissions).Where(s => s.Status == "Approved").Sum(s => s.QuantityGood),
                    so.Status
                }).ToListAsync();

            return Json(new { products, sampleOrders });
        }

        // ==========================================
        // API: PAYMENTS (Thanh toan)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetPayments()
        {
            var payments = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Submission).ThenInclude(s => s.Assignment).ThenInclude(a => a.Product)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new
                {
                    p.Id,
                    UserName = p.User.FullName,
                    ProductName = p.Submission.Assignment.Product.Name,
                    QuantityGood = p.Submission.QuantityGood,
                    UnitPrice = p.Submission.Assignment.Product.UnitPrice,
                    p.Amount,
                    PaymentDate = p.PaymentDate.ToString("dd/MM/yyyy HH:mm"),
                    p.Status
                }).ToListAsync();
            return Json(payments);
        }

        // ==========================================
        // API: PENALTIES (Phat)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetPenalties()
        {
            var penalties = await _context.Penalties
                .Include(p => p.User)
                .Include(p => p.Assignment).ThenInclude(a => a.Product)
                .Include(p => p.Submission)
                .OrderByDescending(p => p.CreatedDate)
                .Select(p => new
                {
                    p.Id,
                    UserName = p.User.FullName,
                    ProductName = p.Assignment.Product.Name,
                    AssignmentId = p.AssignmentId,
                    p.Reason,
                    p.Amount,
                    p.Note,
                    p.Status,
                    p.DefectiveQuantity,
                    SubmissionNumber = p.Submission != null ? p.Submission.SubmissionNumber : 0,
                    CreatedDate = p.CreatedDate.ToString("dd/MM/yyyy"),
                    PaidDate = p.PaidDate.HasValue ? p.PaidDate.Value.ToString("dd/MM/yyyy HH:mm") : ""
                }).ToListAsync();
            return Json(penalties);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePenalty(int assignmentId, decimal amount, string reason, string? note)
        {
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null) return NotFound();

            var penalty = new Penalty
            {
                AssignmentId = assignmentId,
                UserId = assignment.UserId,
                Reason = reason,
                Amount = amount,
                Note = note,
                Status = "Active"
            };
            _context.Penalties.Add(penalty);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Da tao don phat." });
        }

        [HttpPost]
        public async Task<IActionResult> WaivePenalty(int id)
        {
            var penalty = await _context.Penalties.FindAsync(id);
            if (penalty == null) return NotFound();
            penalty.Status = "Waived";
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Da mien phat." });
        }

        [HttpPost]
        public async Task<IActionResult> DeductPenalty(int id, [FromBody] decimal deductedAmount)
        {
            var penalty = await _context.Penalties
                .Include(p => p.User)
                .Include(p => p.Assignment).ThenInclude(a => a.Product).ThenInclude(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
                .Include(p => p.Submission)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (penalty == null) return NotFound("Không tìm thấy phạt");
            if (penalty.Status == "Resolved" || penalty.Status == "Paid") 
                return BadRequest("Phạt này đã được giải quyết rồi.");

            // Kiểm tra số tiền
            if (deductedAmount <= 0 || deductedAmount > penalty.Amount)
                return BadRequest($"Số tiền phạt phải từ 1-{penalty.Amount}");

            penalty.Status = deductedAmount >= penalty.Amount ? "Paid" : "Deducted";
            penalty.PaidDate = DateTime.Now;

            // ============================================
            // AUTO-CONVERT DEFECTS TO QUALIFIED ITEMS
            // ============================================
            if (penalty.Reason == "QualityFail" && penalty.DefectiveQuantity > 0)
            {
                // Lấy submission liên quan nếu có
                Submission? submission = penalty.Submission;
                
                if (submission != null)
                {
                    // Cập nhật submission: chuyển một số lượng từ defect thành good
                    int convertQuantity = Math.Min(penalty.DefectiveQuantity, submission.QuantityDefect);
                    
                    submission.QuantityGood += convertQuantity;
                    submission.QuantityDefect -= convertQuantity;

                    // Cập nhật Assignment CompletedQuantity
                    int prevCompleted = submission.Assignment.CompletedQuantity;
                    submission.Assignment.CompletedQuantity += convertQuantity;

                    // Kiểm tra nếu assignment hoàn thành
                    bool assignmentComplete = submission.Assignment.CompletedQuantity >= submission.Assignment.QuantityAssigned;
                    if (assignmentComplete && submission.Assignment.Status != "Completed")
                    {
                        submission.Assignment.Status = "Completed";
                    }

                    // Cộng vào kho thành phẩm
                    var product = submission.Assignment.Product;
                    product.FinishedStock += convertQuantity;

                    // Gửi thông báo cho hộ dân về giải quyết phạt
                    var resolutionNotification = new Notification
                    {
                        UserId = penalty.UserId,
                        Title = "✓ Phạt Được Giải Quyết!",
                        Message = $"Bạn đã thanh toán phạt. {convertQuantity} sản phẩm lỗi đã được chuyển thành sản phẩm đạt tiêu chuẩn.",
                        Type = "success",
                        CreatedDate = DateTime.Now
                    };
                    _context.Notifications.Add(resolutionNotification);

                    // Cập nhật tiến độ nếu SampleOrder có
                    if (submission.Assignment.SampleOrderId.HasValue)
                    {
                        var sampleOrder = await _context.SampleOrders
                            .Include(so => so.Assignments).ThenInclude(a => a.Submissions)
                            .FirstOrDefaultAsync(so => so.Id == submission.Assignment.SampleOrderId.Value);

                        if (sampleOrder != null)
                        {
                            int newCompletedQty = sampleOrder.Assignments
                                .SelectMany(a => a.Submissions.Where(s => s.Status == "Approved"))
                                .Sum(s => (int?)s.QuantityGood) ?? 0;

                            sampleOrder.CompletedQuantity = newCompletedQty;

                            // Gửi thông báo cập nhật tiến độ
                            var progressNotification = new Notification
                            {
                                UserId = penalty.UserId,
                                Title = "📊 Tiến Độ Đơn Hàng Cập Nhật",
                                Message = $"Đơn hàng {sampleOrder.OrderCode} đã cập nhật tiến độ: {newCompletedQty}/{sampleOrder.TotalQuantity}",
                                Type = "info",
                                CreatedDate = DateTime.Now
                            };
                            _context.Notifications.Add(progressNotification);
                        }
                    }
                }

                penalty.Status = "Resolved";
            }

            // Trừ tiền từ tài khoản người dùng (nếu có)
            penalty.User.Balance -= deductedAmount;

            await _context.SaveChangesAsync();

            return Ok(new { 
                success = true, 
                message = deductedAmount >= penalty.Amount 
                    ? "✓ Đã thanh toán xong phạt và chuyển lỗi → đạt." 
                    : $"✓ Đã trừ {deductedAmount:N0} VND. Còn {penalty.Amount - deductedAmount:N0} VND.",
                newStatus = penalty.Status
            });
        }

        // ==========================================
        // API: CHECK OVERDUE (Kiem tra qua han, tu dong tao phat)
        // ==========================================

        [HttpPost]
        public async Task<IActionResult> CheckOverdue()
        {
            var overdueAssignments = await _context.Assignments
                .Where(a => a.DueDate < DateTime.Now && a.Status == "InProgress")
                .ToListAsync();

            int count = 0;
            foreach (var a in overdueAssignments)
            {
                a.Status = "Overdue";

                // Kiem tra da co penalty chua
                var hasPenalty = await _context.Penalties.AnyAsync(p => p.AssignmentId == a.Id && p.Reason == "Overdue");
                if (!hasPenalty)
                {
                    _context.Penalties.Add(new Penalty
                    {
                        AssignmentId = a.Id,
                        UserId = a.UserId,
                        Reason = "Overdue",
                        Amount = 0, // Admin tu set
                        Note = "Don qua han tu dong tao",
                        Status = "Active"
                    });
                    count++;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = $"Da kiem tra. {count} don phat moi duoc tao.", overdueCount = overdueAssignments.Count });
        }

        // ==========================================
        // API: FINISHED PRODUCTS (Thanh pham)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetFinishedProducts()
        {
            var products = await _context.Products
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.FinishedStock,
                    p.UnitPrice,
                    TotalProduced = p.Assignments
                        .SelectMany(a => a.Submissions)
                        .Where(s => s.Status == "Approved")
                        .Sum(s => (int?)s.QuantityGood) ?? 0,
                    TotalDefect = p.Assignments
                        .SelectMany(a => a.Submissions)
                        .Where(s => s.Status == "Approved")
                        .Sum(s => (int?)s.QuantityDefect) ?? 0
                }).ToListAsync();
            return Json(products);
        }

        // ==========================================
        // ADMIN UTILITIES: Reset Database
        // ==========================================

        [HttpPost]
        public async Task<IActionResult> ResetDatabase()
        {
            try
            {
                // Xoá dữ liệu theo thứ tự (để tránh FK constraint)
                await _context.Payments.ExecuteDeleteAsync();
                await _context.Penalties.ExecuteDeleteAsync();
                await _context.Submissions.ExecuteDeleteAsync();
                await _context.AssignmentMaterials.ExecuteDeleteAsync();
                await _context.Assignments.ExecuteDeleteAsync();
                await _context.MaterialTransactions.ExecuteDeleteAsync();
                await _context.SampleOrders.ExecuteDeleteAsync();
                await _context.ProductMaterials.ExecuteDeleteAsync();
                await _context.Materials.ExecuteDeleteAsync();
                await _context.Products.ExecuteDeleteAsync();
                
                // Xoá tất cả users trừ admin đầu tiên (ID = 1)
                await _context.Users.Where(u => u.Id != 1).ExecuteDeleteAsync();

                // Reset IDENTITY về 1 cho tất cả bảng
                var tables = new[] { "Payments", "Penalties", "Submissions", "AssignmentMaterials", 
                                    "Assignments", "MaterialTransactions", "SampleOrders", 
                                    "ProductMaterials", "Materials", "Products" };
                
                foreach (var table in tables)
                {
                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync($"DBCC CHECKIDENT ('{table}', RESEED, 0)");
                    }
                    catch { }
                }

                return Ok(new { message = "✓ Đã xoá toàn bộ dữ liệu. Hệ thống sẵn sàng bắt đầu lại." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Lỗi khi xoá dữ liệu: " + ex.Message });
            }
        }
    }
}
