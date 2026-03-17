# BÁO CÁO BÀI TẬP LỚN
## XÂY DỰNG HỆ THỐNG QUẢN LÝ GIA CÔNG THỦ CÔNG Mỹ NGHỆ


**Hệ thống:** CraftOutsourcing Management System  
**Công nghệ:** ASP.NET Core 10.0 + Entity Framework + SQL Server  
**Số sinh viên:** 2  
**Thời gian:** 4 tuần (Tháng 3-4/2026)


---

## PHẦN I: MỞ ĐẦU

### 1.1. Tính cấp thiết của đề tài

Các làng nghề thủ công mỹ nghệ tại Việt Nam thường gia công theo hình thức phân phối nguyên liệu đến các hộ dân. Quy trình này gặp nhiều khó khăn:

- ❌ **Quản lý nguyên liệu thủ công:** Khó kiểm soát tồn kho, thất thoát NL
- ❌ **Giao việc & tiến độ:** Không có hệ thống tập trung theo dõi
- ❌ **Kiểm soát chất lượng:** Quy trình duyệt sản phẩm không minh bạch
- ❌ **Tài chính:** Tính lương, phạt thủ công dễ nhầm lẫn
- ❌ **Đơn hàng:** Khó theo dõi tiến độ hoàn thành

**Giải pháp:** Xây dựng Backend MVC để tự động hóa toàn bộ quy trình gia công.

### 1.2. Mục tiêu chính

1. ✅ Thiết kế **15 bảng dữ liệu** đầy đủ
2. ✅ Xây dựng **50 API endpoints** hỗ trợ toàn bộ chức năng
3. ✅ Triển khai logic nghiệp vụ tự động: giao việc → trừ kho → nộp hàng → duyệt KCS → trả công → phạt
4. ✅ Hệ thống thông báo real-time (Notifications)
5. ✅ Xác thực & phân quyền (Admin/User)
6. ✅ Báo cáo tài chính & lợi nhuận

### 1.3. Đối tượng sử dụng

| **Vai trò** | **Chức năng** |
|-----------|-------------|
| **Admin** | Quản lý toàn bộ hệ thống (sản phẩm, NL, giao việc, duyệt KCS, trả lương) |
| **User (Hộ gia công)** | Nhận giao việc, nộp sản phẩm, xem tiến độ, thanh toán phạt |

---

## PHẦN II: PHÂN TÍCH YÊU CẦU CHỨC NĂNG

### 2.1. Các nhóm chức năng chính

#### **Nhóm A: Quản lý nền tảng (Platform Management)**

| **Chức năng** | **Mô tả** | **Chi tiết** |
|-----------|---------|---------|
| **A1. Quản lý User** | Tạo/sửa/xóa hộ gia công | GET/POST/DELETE user, toggle active, approve admin |
| **A2. Quản lý Sản phẩm** | Quản lý sản phẩm & BOM | CRUD product, định mức NL cho sản phẩm |
| **A3. Quản lý Nguyên liệu** | Quản lý kho NL | CRUD material, nhập kho, cảnh báo tồn thấp |
| **A4. Xác thực & Phân quyền** | Login, Register, phân quyền | Cookie Auth, BCrypt password, role-based access |

---

#### **Nhóm B: Quản lý sản xuất & giao công (Production Management)**

| **Chức năng** | **Mô tả** | **Chi tiết** |
|-----------|---------|---------|
| **B1. Quản lý Đơn hàng** | Tạo & quản lý đơn hàng mẫu | Tạo, xác nhận, theo dõi tiến độ đơn |
| **B2. Giao công** | Giao việc + auto trừ kho | Tạo assignment, auto cấp NL theo BOM, lưu lịch sử |
| **B3. Nộp sản phẩm** | Hộ gia công nộp từng đợt | Submission CRUD, hỗ trợ nộp nhiều đợt |
| **B4. Duyệt KCS** | Admin duyệt chất lượng | Phân loại Đạt/Lỗi, auto tạo Payment & Penalty |
| **B5. Yêu cầu thêm NL** | Hộ gia công yêu cầu cấp NL | MaterialRequest CRUD, admin phê duyệt |

---

#### **Nhóm C: Tài chính & phạt (Finance Management)**

| **Chức năng** | **Mô tả** | **Chi tiết** |
|-----------|---------|---------|
| **C1. Trả lương** | Thanh toán tiền công | Auto tính lương = Số lượng đạt × Đơn giá |
| **C2. Quản lý phạt** | Phạt lỗi + phạt quá hạn | Auto tạo penalty, miễn phạt, thanh toán phạt |
| **C3. Tha bổng lỗi** | Auto chuyển lỗi → đạt khi thanh toán phạt | Cộng lương & tồn kho thành phẩm |
| **C4. Báo cáo lợi nhuận** | Tính lợi nhuận theo đơn hàng | ActualProfit = (Giá bán - Giá vốn) × Số lượng đạt |

---

#### **Nhóm D: Hỗ trợ & tiện ích (Support)**

| **Chức năng** | **Mô tả** | **Chi tiết** |
|-----------|---------|---------|
| **D1. Hệ thống thông báo** | Gửi thông báo cho hộ dân | Khi giao việc, duyệt KCS, phạt, hoàn thành |
| **D2. Lịch sử kho** | Theo dõi nhập/xuất NL | MaterialTransactions log |
| **D3. Kiểm tra quá hạn** | Auto phạt đơn giao việc quá hạn | CheckOverdue scan |
| **D4. Tiện ích Admin** | Reset database | ResetDatabase utility |

---

## PHẦN III: THIẾT KẾ CƠ SỞ DỮ LIỆU

### 3.1. Sơ đồ ER (Entity-Relationship Diagram) - Kỹ Lưỡng & Nối Hết

```
╔════════════════════════════════════════════════════════════════════════════════════════╗
║                         CRAFTOUTSOURCING DATABASE SCHEMA                              ║
║                     (15 Entities, All relationships connected)                         ║
╚════════════════════════════════════════════════════════════════════════════════════════╝

┌─────────────────────┐
│  ROLES  [PK: Id]    │
│  Name               │  
└──────────┬──────────┘
           │
           │ 1:N (↓)
           │
           ↓
┌────────────────────────────────┐
│  USER  [PK: Id]                │
│  Username, PasswordHash         │
│  FK: RoleId → Roles             │
│  IsActive, IsApproved           │
└────┬────────┬────────┬─────┬────┘
     │        │        │     │
  1:N│     1:N│     1:N│  1:N│
     │        │        │     │
     ↓        ↓        ↓     ↓
     │        │        │     └─────────────────────────────────┐
     │        │        │                                       │
     │        │        ├─→ ┌──────────────────────────┐        │
     │        │        │   │ PENALTY [PK: Id]        │        │
     │        │        │   │ FK: UserId → User       │        │
     │        │        │   │ FK: AssignmentId → Asg  │        │
     │        │        │   │ Amount, Status          │        │
     │        │        │   └──────────────────────────┘        │
     │        │        │                                       │
     │        │        ├─→ ┌──────────────────────────┐        │
     │        │        │   │ NOTIFICATION [PK: Id]   │        │
     │        │        │   │ FK: UserId → User       │        │
     │        │        │   │ Title, Message, Type    │        │
     │        │        │   └──────────────────────────┘        │
     │        │        │                                       │
     │        │        └─→ ┌──────────────────────────┐        │
     │        │            │ MATERIAL REQUEST [PK:Id]│        │
     │        │            │ FK: UserId→User         │        │
     │        │            │ (cho phép user mới)     │        │
     │        │            └──────────────────────────┘        │
     │        │                                                 │
     │        └─→ ┌────────────────────────────────────────────┘
     │            │
     │            │ 1:N (↓)
     │            │
     │            ↓
     │        ┌──────────────────────────────────┐
     │        │ PAYMENT [PK: Id]                 │
     │        │ FK: UserId → User                │
     │        │ FK: SubmissionId → Submission 1:1│
     │        │ Amount, PaymentDate, Status      │
     │        └────────────┬──────────────────────┘
     │                     │
     │                     │ 1:1 (↑)
     │                     │
     └─→ ┌────────────────────────────────┐
         │ ASSIGNMENT [PK: Id]            │
         │ FK: UserId → User (1:N)        │
         │ FK: ProductId → Product (1:N)  │
         │ FK: SampleOrderId → SampleOrder│
         │ QuantityAssigned, DueDate      │
         │ Status, AssignedDate           │
         └────┬──────────┬─────────────────┘
              │          │
           1:N│       1:N│
              │          │
              ↓          ↓
    ┌──────────────────────────────────────┐
    │ SUBMISSION [PK: Id]                  │
    │ FK: AssignmentId → Assignment (1:N)  │
    │ QuantityGood, QuantityDefect         │
    │ QuantitySubmitted, Status            │
    │ SubmittedDate, SubmissionNumber      │
    └────────────────────────────────────┬─┘
                                        │
                                     1:1│ (→ PAYMENT)
                                        │
         ┌──────────────────────────────────────────────┐
         │                                              │
         ↓                                              │
    ┌──────────────────────────────┐                  │
    │ PRODUCT [PK: Id]             │                  │
    │ Name, Description, UnitPrice │                  │
    │ (tiền công)                  │                  │
    │ FinishedStock                │                  │
    └────┬──────────────┬──────────┘                  │
         │              │                             │
      1:N│           N:M│                             │
         │              │                             │
         │              ↓                             │
         │      ┌──────────────────────────────┐     │
         │      │ PRODUCTMATERIAL [PK: Id]    │     │
         │      │ FK: ProductId → Product     │     │
         │      │ FK: MaterialId → Material   │     │
         │      │ QuantityRequired            │     │
         │      └──────────────┬───────────────┘     │
         │                     │                     │
         │                  1:N│                     │
         │                     │                     │
         │                     ↓                     │
         │              ┌──────────────────────────┐ │
         │              │ MATERIAL [PK: Id]       │ │
         │              │ Name, Unit              │ │
         │              │ StockQuantity, MinStock │ │
         │              │ UnitPrice               │ │
         │              └────┬──────────┬─────┬────┘ │
         │                   │          │     │      │
         │                1:N│       1:N│  1:N│      │
         │                   │          │     │      │
         │                   ↓          ↓     ↓      │
         │         ┌──────────────────────────┐     │
         │         │ MATERIAL TRANSACTION[PK] │     │
         │         │ FK: MaterialId→Material  │     │
         │         │ TransactionType (IN/OUT) │     │
         │         │ Quantity, TransactionDate│     │
         │         │ (log nhập xuất kho)      │     │
         │         └──────────────────────────┘     │
         │                                           │
         │         ┌──────────────────────────┐     │
         │         │ ASSIGNMENT MATERIAL[PK] │     │
         │         │ FK:AssignmentId→Assign  │     │
         │         │ FK:MaterialId→Material  │     │
         │         │ QuantityAllocated       │     │
         │         │ (NL cấp cho giao công)  │     │
         │         └──────────────────────────┘     │
         │                                           │
         │         ┌──────────────────────────┐     │
         │         │ MATERIAL REQUEST [PK:Id]│     │
         │         │ FK:AssignmentId→Assign  │     │
         │         │ FK:MaterialId→Material  │     │
         │         │ QuantityRequested       │     │
         │         │ Status (yêu cầu cấp NL) │     │
         │         └──────────────────────────┘     │
         │                                           │
         ↓                                           │
    ┌──────────────────────────────────┐           │
    │ SAMPLE ORDER [PK: Id]            │           │
    │ FK: ProductId → Product          │           │
    │ OrderCode, TotalQuantity         │           │
    │ CompletedQuantity, Status        │           │
    │ SellingPrice (giá bán khách)     │           │
    │ CreatedDate                      │           │
    └────────────┬─────────────────────┘           │
                 │                                 │
              1:N│                                 │
                 │                                 │
                 └──────→ ASSIGNMENT ← ─ ─ ─ ─ ─┘
                    (SampleOrder 1:N Assignment)
                    
                 (Tất cả ASSIGNMENT thành công
                  → Submission Approved
                  → Payment 1:1)
                  
                  
    ┌──────────────────────────────┐
    │ PROFIT [PK: Id]              │
    │ FK: SampleOrderId→SampleOrder│
    │ ActualProfit (lợi nhuận)     │
    │ (tính từ lương + phạt)       │
    └──────────────────────────────┘

═══════════════════════════════════════════════════════════════════

RELATIONSHIP SUMMARY:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 Role             →  1:N  → User
 User             →  1:N  → Assignment, Payment, Penalty, Notification
 Product          →  1:N  → SampleOrder
 Product          →  N:M  → Material (via ProductMaterial)
 Material         →  1:N  → ProductMaterial, MaterialTransaction
 Material         →  1:N  → AssignmentMaterial, MaterialRequest
 SampleOrder      →  1:N  → Assignment
 Assignment       →  1:N  → Submission, AssignmentMaterial, MaterialRequest, Penalty
 Submission       →  1:1  → Payment
 User(Payment)    →  1:N  → Payment (Thanh toán lương)
 SampleOrder      →  1:1  → Profit (Tính lợi nhuận)
───────────────────────────────────────────────────────────────────
```

### 3.2. Danh sách 15 Bảng - Chi tiết SQL, Mô tả & Đánh dấu SV

**Chú thích:** 
- 🔵 **[SV1]** = Sinh viên 1 (Khung & CRUD) 
- 🔴 **[SV2]** = Sinh viên 2 (Logic & Test)

---

#### **1. Roles - 🔵 [SV1]**

```sql
CREATE TABLE Roles (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(50) NOT NULL
);
```

**Mô tả**: Lưu trữ vai trò người dùng (Admin, User). Id: Mã vai trò (khóa chính, tự tăng). Seed data: Admin (Id=1), User (Id=2)

---

#### **2. Users - 🔵 [SV1]**

```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    Address NVARCHAR(255),
    IsApproved BIT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    Balance DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL,
    RoleId INT NOT NULL FOREIGN KEY REFERENCES Roles(Id)
);
```

**Mô tả**: Lưu trữ thông tin người dùng (Admin + Hộ gia công). PasswordHash: Mật khẩu mã hóa BCrypt. IsApproved: Admin mới cần được phê duyệt bởi Admin hiện tại. IsActive: Trạng thái hoạt động (có thể vô hiệu hóa). Balance: Số dư tài khoản (dùng cho trừ phạt).

---

#### **3. Materials - 🔵 [SV1]**

```sql
CREATE TABLE Materials (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL,
    Unit NVARCHAR(20) NOT NULL,
    StockQuantity FLOAT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    MinStock FLOAT NOT NULL
);
```

**Mô tả**: Lưu trữ thông tin nguyên liệu/vật tư. StockQuantity: Số lượng tồn kho hiện tại. UnitPrice: Giá 1 đơn vị nguyên liệu. MinStock: Mức tồn kho tối thiểu, dưới mức này sẽ cảnh báo. Hỗ trợ 16 đơn vị tính: kg, g, l, ml, m, cm, mm, cái, cuộn, bộ, hộp, túi, chiếc, sợi, đoạn, lớp.

---

#### **4. MaterialTransactions - 🔵 [SV1]**

```sql
CREATE TABLE MaterialTransactions (
    Id INT PRIMARY KEY IDENTITY,
    MaterialId INT NOT NULL FOREIGN KEY REFERENCES Materials(Id),
    TransactionType NVARCHAR(20) NOT NULL,
    Quantity FLOAT NOT NULL,
    TransactionDate DATETIME2 NOT NULL,
    ReferenceId INT
);
```

**Mô tả**: Ghi nhận lịch sử nhập/xuất kho nguyên liệu. TransactionType: Import (nhập), Export (xuất). ReferenceId: ID Assignment nếu xuất kho cho giao việc.

---

#### **5. Products - 🔵 [SV1]**

```sql
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    UnitPrice DECIMAL(18,2) NOT NULL,
    FinishedStock INT NOT NULL DEFAULT 0
);
```

**Mô tả**: Lưu trữ thông tin sản phẩm thủ công mỹ nghệ. UnitPrice: Đơn giá trả công cho thợ / 1 sản phẩm. FinishedStock: Tồn kho thành phẩm (tự động cộng khi duyệt KCS).

---

#### **6. ProductMaterials - 🔵 [SV1]**

```sql
CREATE TABLE ProductMaterials (
    Id INT PRIMARY KEY IDENTITY,
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(Id) ON DELETE CASCADE,
    MaterialId INT NOT NULL FOREIGN KEY REFERENCES Materials(Id) ON DELETE NO ACTION,
    QuantityRequired FLOAT NOT NULL
);
```

**Mô tả**: Bảng trung gian lưu định mức nguyên liệu cho mỗi sản phẩm (Bill of Materials). QuantityRequired: Số lượng nguyên liệu cần cho **1 sản phẩm**. Dùng để: tự động trừ kho khi giao việc, tính giá thành sản phẩm.

---

#### **7. SampleOrders - 🔵 [SV1]**

```sql
CREATE TABLE SampleOrders (
    Id INT PRIMARY KEY IDENTITY,
    OrderCode NVARCHAR(150) NOT NULL,
    CustomerName NVARCHAR(200) NOT NULL,
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(Id) ON DELETE NO ACTION,
    TotalQuantity INT NOT NULL,
    CompletedQuantity INT NOT NULL DEFAULT 0,
    Description NVARCHAR(500),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Draft',
    CreatedDate DATETIME2 NOT NULL,
    TargetDate DATETIME2,
    EstimatedCost DECIMAL(18,2) NOT NULL,
    ActualCost DECIMAL(18,2) NOT NULL DEFAULT 0,
    SellingPrice DECIMAL(18,2) NOT NULL
);
```

**Mô tả**: Lưu trữ đơn hàng mẫu từ khách hàng. OrderCode: Tự động sinh (SO0001, SO0002...) hoặc nhập tay. EstimatedCost: Tự động tính = (Chi phí NL + Tiền công) × Số lượng. Status: Theo dõi vòng đời đơn hàng từ Draft → InProduction → Completed.

---

#### **8. Assignments - 🔵 [SV1]**

```sql
CREATE TABLE Assignments (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE NO ACTION,
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(Id) ON DELETE NO ACTION,
    QuantityAssigned INT NOT NULL,
    CompletedQuantity INT NOT NULL DEFAULT 0,
    AssignedDate DATETIME2 NOT NULL,
    DueDate DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    SampleOrderId INT FOREIGN KEY REFERENCES SampleOrders(Id) ON DELETE SET NULL
);
```

**Mô tả**: Ghi nhận việc giao công cho hộ dân. Khi tạo Assignment: tự động trừ kho nguyên liệu theo BOM. CompletedQuantity: Cập nhật khi Admin duyệt KCS. Liên kết với SampleOrder (optional) để theo dõi tiến độ đơn hàng.

---

#### **9. AssignmentMaterials - 🔴 [SV2]**

```sql
CREATE TABLE AssignmentMaterials (
    Id INT PRIMARY KEY IDENTITY,
    AssignmentId INT NOT NULL FOREIGN KEY REFERENCES Assignments(Id) ON DELETE CASCADE,
    MaterialId INT NOT NULL FOREIGN KEY REFERENCES Materials(Id) ON DELETE NO ACTION,
    QuantityGiven FLOAT NOT NULL
);
```

**Mô tả**: Lưu chi tiết nguyên liệu đã cấp cho mỗi lần giao việc. Tự động tạo khi Admin giao việc. QuantityGiven: Số lượng nguyên liệu thực tế cấp cho thợ.

---

#### **10. Submissions - 🔴 [SV2]**

```sql
CREATE TABLE Submissions (
    Id INT PRIMARY KEY IDENTITY,
    AssignmentId INT NOT NULL FOREIGN KEY REFERENCES Assignments(Id) ON DELETE CASCADE,
    SubmittedDate DATETIME2 NOT NULL,
    SubmissionNumber INT NOT NULL,
    QuantitySubmitted INT NOT NULL,
    QuantityGood INT NOT NULL DEFAULT 0,
    QuantityDefect INT NOT NULL DEFAULT 0,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    ReviewNote NVARCHAR(200)
);
```

**Mô tả**: Ghi nhận mỗi lần hộ dân nộp sản phẩm. Hỗ trợ nộp nhiều đợt (SubmissionNumber = 1, 2, 3...). Admin duyệt KCS: phân loại QuantityGood và QuantityDefect. Quan hệ One-to-One với Payment.

---

#### **11. Payments - 🔴 [SV2]**

```sql
CREATE TABLE Payments (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE NO ACTION,
    SubmissionId INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Submissions(Id) ON DELETE NO ACTION,
    Amount DECIMAL(18,2) NOT NULL,
    PaymentDate DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Paid'
);
```

**Mô tả**: Ghi nhận thanh toán tiền công cho hộ gia công. Tự động tạo khi Admin duyệt KCS. **Công thức**: Amount = Tổng tiền công = QuantityGood × UnitPrice.

---

#### **12. Penalties - 🔴 [SV2]**

```sql
CREATE TABLE Penalties (
    Id INT PRIMARY KEY IDENTITY,
    AssignmentId INT NOT NULL FOREIGN KEY REFERENCES Assignments(Id) ON DELETE NO ACTION,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE NO ACTION,
    Reason NVARCHAR(20) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Note NVARCHAR(500),
    CreatedDate DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active',
    PaidDate DATETIME2,
    DefectiveQuantity INT NOT NULL DEFAULT 0,
    SubmissionId INT FOREIGN KEY REFERENCES Submissions(Id) ON DELETE SET NULL
);
```

**Mô tả**: Quản lý tiền phạt cho hộ gia công. Reason: Overdue (quá hạn) hoặc QualityFail (lỗi chất lượng). **Phạt lỗi**: Tự động tạo khi duyệt KCS có sản phẩm lỗi, Amount = Số lỗi × Chi phí NL/SP. **Phạt quá hạn**: Tự động tạo khi kiểm tra đơn quá hạn. Khi thanh toán phạt: tự động chuyển sản phẩm lỗi → đạt (tha bổng).

---

#### **13. MaterialRequests - 🔴 [SV2]**

```sql
CREATE TABLE MaterialRequests (
    Id INT PRIMARY KEY IDENTITY,
    AssignmentId INT NOT NULL FOREIGN KEY REFERENCES Assignments(Id) ON DELETE CASCADE,
    MaterialId INT NOT NULL FOREIGN KEY REFERENCES Materials(Id) ON DELETE CASCADE,
    QuantityRequested INT NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    CreatedDate DATETIME2 NOT NULL,
    ApprovedDate DATETIME2,
    Reason NVARCHAR(200)
);
```

**Mô tả**: Hộ dân yêu cầu cấp thêm nguyên liệu khi thiếu. Admin duyệt → tự động trừ kho.

---

#### **14. Notifications - 🔴 [SV2]**

```sql
CREATE TABLE Notifications (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    Title NVARCHAR(MAX) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Type NVARCHAR(MAX) NOT NULL DEFAULT 'info',
    RelatedLink NVARCHAR(MAX),
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL
);
```

**Mô tả**: Hệ thống thông báo cho hộ gia công. Tự động gửi khi: giao việc mới, duyệt KCS, phạt, hoàn thành đơn.

---

#### **15. Profits - 🔴 [SV2]**

```sql
CREATE TABLE Profits (
    Id INT PRIMARY KEY IDENTITY,
    SampleOrderId INT NOT NULL FOREIGN KEY REFERENCES SampleOrders(Id) ON DELETE CASCADE,
    QuantityGood INT NOT NULL,
    QuantityDefect INT NOT NULL,
    SellingPrice DECIMAL(18,2) NOT NULL,
    CostPrice DECIMAL(18,2) NOT NULL,
    SalesProfit DECIMAL(18,2) NOT NULL,
    PenaltyRevenue DECIMAL(18,2) NOT NULL,
    TotalProfit DECIMAL(18,2) NOT NULL,
    RecordDate DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active'
);
```

**Mô tả**: Ghi nhận lợi nhuận theo đơn hàng mẫu. **Công thức lợi nhuận**: SalesProfit = (Số lượng đạt × Giá bán) - (Số lượng đạt × Giá vốn). TotalProfit = SalesProfit + PenaltyRevenue.

---

### 3.3. Tóm tắt 15 Bảng (Phân công SV)

| # | Bảng | Loại | SV | FK | Mô tả |
|----|------|------|----|----|-------|
| 1 | Roles | Identity | 🔵 SV1 | — | Vai trò (Admin=1, User=2) |
| 2 | Users | Identity | 🔵 SV1 | Role | Tài khoản người dùng |
| 3 | Materials | Inventory | 🔵 SV1 | — | Nguyên liệu/vật tư |
| 4 | MaterialTransactions | Inventory | 🔵 SV1 | Material | Lịch sử nhập/xuất kho |
| 5 | Products | Production | 🔵 SV1 | — | Sản phẩm thủ công |
| 6 | ProductMaterials | Production | 🔵 SV1 | Product, Material | Định mức NL (BOM) |
| 7 | SampleOrders | Production | � SV2 | Product | Đơn hàng mẫu |
| 8 | Assignments | Production | 🔴 SV2 | User, Product, SampleOrder | Giao việc |
| 9 | AssignmentMaterials | Production | 🔴 SV2 | Assignment, Material | NL cấp cho giao việc |
| 10 | Submissions | Production | 🔴 SV2 | Assignment | Nộp sản phẩm |
| 11 | Payments | Finance | 🔴 SV2 | User, Submission | Trả lương |
| 12 | Penalties | Finance | 🔴 SV2 | User, Assignment, Submission | Phạt |
| 13 | MaterialRequests | Production | 🔵 SV1 | Assignment, Material | Yêu cầu cấp NL thêm |
| 14 | Notifications | Support | 🔵 SV1 | User | Thông báo |
| 15 | Profits | Finance | 🔴 SV2 | SampleOrder | Lợi nhuận |

**Total:** 15 bảng
- 🔵 **SV1:** 8 bảng (Roles, Users, Materials, MaterialTransactions, Products, ProductMaterials, MaterialRequests, Notifications)
- 🔴 **SV2:** 7 bảng (SampleOrders, Assignments, AssignmentMaterials, Submissions, Payments, Penalties, Profits)

---

## PHẦN IV: KIẾN TRÚC BACKEND

### 4.1. Kiến trúc MVC và Luồng Dữ Liệu

```
┌──────────────────────────────────────────────────────────────────┐
│            PRESENTATION LAYER (Client)                          │
├──────────────────────────────────────────────────────────────────┤
│  Browser                                                        │
│  ├─ Razor Views (.cshtml)  [Views/Auth/, Views/Admin/, ...]   │
│  ├─ Layout Master (_Layout.cshtml, _AdminLayout.cshtml)       │
│  └─ JavaScript (jQuery AJAX, Bootstrap 5)                     │
└──────────────────────────────────────────────────────────────────┘
                           │
                           ↓ HTTP Request
┌──────────────────────────────────────────────────────────────────┐
│         MIDDLEWARE LAYER (Authentication & Authorization)       │
├──────────────────────────────────────────────────────────────────┤
│  Cookie Authentication Middleware                               │
│  ├─ HttpContext.User identity                                 │
│  ├─ [Authorize] attribute check                                │
│  ├─ Role validation (Admin / User)                             │
│  └─ Password: BCrypt hashing (BCrypt.Net)                     │
│                                                                 │
│  3 Roles:  🔴 Admin (quản lý) | 🔵 User (hộ gia công)        │
└──────────────────────────────────────────────────────────────────┘
                           │
                           ↓ Route to Controller
┌──────────────────────────────────────────────────────────────────┐
│           CONTROLLER LAYER (Request Handling)                   │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│ 1️⃣ AuthController (Public - Không cần [Authorize])            │
│    ├─ Login GET/POST        → Xác thực tài khoản              │
│    ├─ Register GET/POST     → Tạo User mới (hộ gia công)    │
│    ├─ RegisterAdmin GET/POST → Tạo Admin mới                  │
│    ├─ Logout                → Xóa cookie                       │
│    └─ AccessDenied          → Trang quyền hạn                 │
│                                                                  │
│ 2️⃣ AdminController [Authorize(Roles="Admin")] - 50 methods   │
│    ├─ PAGE VIEWS (14):                                         │
│    │  ├─ Index              → Dashboard (thống kê)            │
│    │  ├─ Users              → Danh sách hộ gia công          │
│    │  ├─ Materials          → Quản lý nguyên liệu            │
│    │  ├─ Products           → Quản lý sản phẩm               │
│    │  ├─ SampleOrders       → Quản lý đơn hàng              │
│    │  ├─ Assignments        → Giao công                       │
│    │  ├─ Submissions        → Duyệt sản phẩm nộp            │
│    │  ├─ Inventory          → Tồn kho NL                    │
│    │  ├─ Payments           → Thanh toán lương              │
│    │  ├─ Penalties          → Quản lý phạt                  │
│    │  ├─ AdminAccounts      → Duyệt Admin mới               │
│    │  ├─ CostEstimation     → Ước tính giá thành            │
│    │  ├─ FinishedProducts   → Tồn kho thành phẩm            │
│    │  └─ OrderProgress      → Theo dõi tiến độ               │
│    │                                                           │
│    └─ API ENDPOINTS (36):                                      │
│       ├─ GetUsers, DisableUser, ApproveAdmin                 │
│       ├─ GetMaterials, AddMaterial, UpdateMaterial           │
│       ├─ GetProducts, AddProduct, DeleteProduct              │
│       ├─ GetSampleOrders, CreateSampleOrder                  │
│       ├─ GetAssignments, CreateAssignment                    │
│       │  ⚙️ (Auto-tạo AssignmentMaterial, trừ kho)         │
│       ├─ GetSubmissions, ApproveSubmission, RejectSubmission│
│       │  ⚙️ (Auto-tạo Payment + Penalty)                    │
│       ├─ GetPayments, GetPenalties, PayPenalty              │
│       ├─ CheckMaterialRequest, ApproveMaterialRequest       │
│       ├─ GetMaterialTransactionHistory                      │
│       ├─ CalculateProfit, ResetDatabase                     │
│       └─ ... (36 endpoints)                                   │
│                                                                  │
│ 3️⃣ UserController [Authorize(Roles="User")] - 10 methods     │
│    ├─ PAGE VIEWS (3):                                          │
│    │  ├─ Index             → Dashboard (tiến độ công việc)   │
│    │  ├─ MyProgress        → Chi tiết tiến độ từng assignment│
│    │  └─ Recipes           → Công thức/BOM sản phẩm         │
│    │                                                           │
│    └─ API ENDPOINTS (7):                                       │
│       ├─ GetMyAssignments                                     │
│       ├─ GetAssignmentMaterials                               │
│       ├─ SubmitWork        → Nộp sản phẩm tạo Submission   │
│       ├─ GetMyProgress     → Tiến độ chi tiết               │
│       ├─ GetMyRecipes      → BOM sau khi trừ kho            │
│       ├─ GetUnreadNotifications                              │
│       └─ PayPenalty        → Thanh toán phạt                │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
                           │
                           ↓ Process Business Logic
┌──────────────────────────────────────────────────────────────────┐
│      APPLICATION LAYER (Models & Business Logic)                │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│ DOMAIN MODELS (Entity Classes):                                │
│ ├─ Identity/Role, Identity/User                               │
│ ├─ Production/Product, Production/ProductMaterial             │
│ ├─ Production/Assignment, Production/AssignmentMaterial       │
│ ├─ Production/Submission, Production/SampleOrder              │
│ ├─ Inventory/Material, Inventory/MaterialTransaction          │
│ ├─ Inventory/MaterialRequest, Inventory/UnitType              │
│ ├─ Finance/Payment, Finance/Penalty, Finance/Profit          │
│ └─ Notification                                                │
│                                                                  │
│ KEY BUSINESS LOGIC FLOWS:                                       │
│                                                                  │
│ 🔴 FLOW 1: Admin Giao Công (CreateAssignment)                 │
│    Input: UserId, ProductId, QuantityAssigned, DueDate        │
│    Process:                                                     │
│    ├─ ✅ Check: Product BOM exists                             │
│    ├─ ✅ Check: Material stock ≥ BOM × QuantityAssigned      │
│    ├─ 📝 Create Assignment                                     │
│    ├─ 📝 Create AssignmentMaterial (từ ProductMaterial)       │
│    ├─ ➖ Deduct Material.StockQuantity                        │
│    ├─ 📝 Create MaterialTransaction (log)                     │
│    └─ 🔔 Send Notification to User                            │
│                                                                  │
│ 🟢 FLOW 2: User Nộp Sản Phẩm (SubmitWork)                     │
│    Input: AssignmentId, QuantityGood, QuantityDefect          │
│    Process:                                                     │
│    ├─ ✅ Check: QuantityGood + QuantityDefect = Assigned Qty  │
│    ├─ 📝 Create Submission (Status="Pending")                 │
│    ├─ ➕ Add QuantityGood to SampleOrder.CompletedQuantity   │
│    └─ 🔔 Send Notification to Admin                           │
│                                                                  │
│ 🔵 FLOW 3: Admin Duyệt KCS (ApproveSubmission)                │
│    Input: SubmissionId, Status (Approved/Rejected)            │
│    Process:                                                     │
│    ├─ 📝 Update Submission.Status                              │
│    ├─ IF Approved:                                             │
│    │  ├─ 💰 Create Payment                                    │
│    │  │   PaymentAmount = QuantityGood × Product.UnitPrice   │
│    │  ├─ 🚫 Create Penalty (nếu QuantityDefect > 0)          │
│    │  │   PenaltyAmount = QuantityDefect × UnitPrice × 0.5  │
│    │  └─ ➕ Add QuantityGood to FinishedProducts             │
│    ├─ ELSE (Rejected):                                         │
│    │  └─ ➕ Add QuantitySubmitted back to Material stock      │
│    └─ 🔔 Send Notification to User                            │
│                                                                  │
│ 💵 FLOW 4: Calculate Profit                                    │
│    Formula: Profit = (SellingPrice - CostPerUnit) × Qty_Good  │
│    Cost Per Unit = Product.UnitPrice + Σ(Material.UnitPrice × │
│                    ProductMaterial.QuantityRequired)           │
│                                                                  │
│ DTOs (Data Transfer):                                           │
│ ├─ ProductCreateDto {name, unitPrice, materials[]}            │
│ ├─ ProductMaterialDto {materialId, quantityRequired}          │
│ ├─ AssignmentDto {userId, productId, quantityAssigned, ...}  │
│ └─ SubmissionDto {assignmentId, quantityGood, defect, ...}   │
│                                                                  │
│ VALIDATION RULES:                                               │
│ ├─ Material.StockQuantity > 0 before assignment               │
│ ├─ Submission.QuantityGood ≥ 0                                 │
│ ├─ Payment.Amount = QuantityGood × UnitPrice                  │
│ ├─ Penalty.Status ∈ ["Active", "Waived", "Paid"]             │
│ └─ Assignment.Status ∈ ["InProgress", "Submitted", "Done"]    │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
                           │
                           ↓ Query/Persist Data
┌──────────────────────────────────────────────────────────────────┐
│            DATA ACCESS LAYER (Entity Framework)                 │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│ ApplicationDbContext : DbContext                                │
│                                                                  │
│ DbSets (15 entities):                                            │
│ ├─ DbSet<Role> Roles                                            │
│ ├─ DbSet<User> Users                                            │
│ ├─ DbSet<Product> Products                                      │
│ ├─ DbSet<ProductMaterial> ProductMaterials (N:M)               │
│ ├─ DbSet<Material> Materials                                    │
│ ├─ DbSet<MaterialTransaction> MaterialTransactions              │
│ ├─ DbSet<MaterialRequest> MaterialRequests                      │
│ ├─ DbSet<SampleOrder> SampleOrders                              │
│ ├─ DbSet<Assignment> Assignments                                │
│ ├─ DbSet<AssignmentMaterial> AssignmentMaterials (N:M)         │
│ ├─ DbSet<Submission> Submissions                                │
│ ├─ DbSet<Payment> Payments                                      │
│ ├─ DbSet<Penalty> Penalties                                     │
│ ├─ DbSet<Notification> Notifications                            │
│ └─ DbSet<Profit> Profits                                        │
│                                                                  │
│ OnModelCreating() - Relationships:                              │
│ ├─ Assignment 1:N Submission 1:1 Payment                       │
│ ├─ Assignment 1:N AssignmentMaterial :N Material              │
│ ├─ Product 1:N ProductMaterial :N Material                     │
│ ├─ User 1:N Assignment, Payment, Penalty, Notifications       │
│ ├─ SampleOrder 1:N Assignment                                  │
│ ├─ User 1:1 Role (via RoleId)                                 │
│ └─ DeleteBehavior: Restrict (tránh xóa dùng chéo)            │
│                                                                  │
│ Key Methods:                                                     │
│ ├─ SaveChangesAsync() → Persist changes                        │
│ ├─ Include() → Eager load related entities                     │
│ ├─ Where().CountAsync() → Query thống kê                       │
│ └─ .Find() → Lookup by primary key                             │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
                           │
                           ↓ Transactions
┌──────────────────────────────────────────────────────────────────┐
│           DATABASE LAYER (SQL Server)                           │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│ SQL Server LocalDB / Express                                    │
│ Database: CraftOutsourcingDB                                    │
│                                                                  │
│ 15 Tables with Relationships:                                   │
│ ├─ Tables:   [Roles], [Users], [Products], [Materials],       │
│ │            [ProductMaterials], [Assignments],               │
│ │            [AssignmentMaterials], [Submissions],            │
│ │            [SampleOrders], [Payments], [Penalties],         │
│ │            [Notifications], [MaterialTransactions],         │
│ │            [MaterialRequests], [Profits]                    │
│ │                                                              │
│ ├─ Constraints:                                                 │
│ │  ├─ PRIMARY KEY (Id)                                        │
│ │  ├─ FOREIGN KEY (UserId, RoleId, ProductId, ...)          │
│ │  ├─ NOT NULL (Name, StockQuantity, ...)                    │
│ │  ├─ DECIMAL(18,2) (UnitPrice, Amount, ...)                │
│ │  └─ CHECK (StockQuantity ≥ 0)                              │
│ │                                                              │
│ ├─ Indexes:                                                     │
│ │  ├─ Covering: (UserId, Status) on Assignments              │
│ │  ├─ Covering: (Status, Created) on Submissions             │
│ │  └─ FK indexes auto-created by EF Core                     │
│ │                                                              │
│ └─ Migrations:                                                  │
│    └─ 20260316152428_InitialCreate                             │
│       ├─ CreateTable Roles, Users, Products, ...              │
│       ├─ CreateForeignKey assignments                          │
│       ├─ CreateIndex for performance                          │
│       └─ SeedData (Default roles)                             │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

### 4.2. Công nghệ Stack & Dependency Injection

| **Layer** | **Technology** | **Version** | **Mục đích** |
|-----------|---------------|-----------|----------|
| **UI Framework** | ASP.NET Core MVC | 10.0 | Routing, Controller, View rendering |
| **Frontend** | Razor + jQuery + Bootstrap | 5.x | HTML templating, AJAX, CSS grid |
| **ORM** | Entity Framework Core | 10.0 | Data mapping, relationships, migrations |
| **Database** | SQL Server | LocalDB | Persistent storage, transactions |
| **Authentication** | Cookie Auth | Built-in | Session, authorization, roles |
| **Security** | BCrypt.Net-Next | 4.1.0 | Password hashing |
| **HTTP** | Kestrel/IIS | — | Web server |

### 4.3. Dependency Injection (Program.cs)

```csharp
// Program.cs - Cấu hình services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
    });

builder.Services.AddAuthorization();

// Controllers sẽ inject ApplicationDbContext
// public AdminController(ApplicationDbContext context) { _context = context; }
```

---

## PHẦN V: CHI TIẾT API ENDPOINTS (50 Endpoints)

### 5.1. Danh sách đầy đủ theo nhóm chức năng

#### **Nhóm 1: Authentication - 7 APIs - 🔵 SV1**

| # | Method | Endpoint | Mô tả | Quyền hạn | SV |
|----|--------|----------|-------|-----------|-----| 
| 1 | GET | `/Auth/Login` | Trang đăng nhập | Public | 🔵 SV1 |
| 2 | POST | `/Auth/Login` | Xác thực tài khoản (BCrypt) | Public | 🔵 SV1 |
| 3 | GET | `/Auth/Register` | Trang đăng ký hộ gia công | Public | 🔵 SV1 |
| 4 | POST | `/Auth/Register` | Tạo tài khoản hộ gia công | Public | 🔵 SV1 |
| 5 | GET | `/Auth/RegisterAdmin` | Trang đăng ký Admin | Public | 🔵 SV1 |
| 6 | POST | `/Auth/RegisterAdmin` | Tạo Admin mới (pending approval) | Public | 🔵 SV1 |
| 7 | POST | `/Auth/Logout` | Đăng xuất (clear cookie) | Authenticated | 🔵 SV1 |

---

#### **Nhóm 2: User Management - 6 APIs - 🔵 SV1**

| # | Method | Endpoint | Mô tả | Quyền hạn | SV |
|----|--------|----------|-------|-----------|-----| 
| 8 | GET | `/Admin/GetUsers` | Lấy danh sách hộ gia công | Admin | 🔵 SV1 |
| 9 | POST | `/Admin/ToggleUserActive/{id}` | Bật/tắt kích hoạt hộ gia công | Admin | 🔵 SV1 |
| 10 | POST | `/Admin/DeleteUser/{id}` | Xóa hộ gia công | Admin | 🔵 SV1 |
| 11 | POST | `/Admin/ApproveAdmin/{id}` | Phê duyệt Admin mới | Admin | 🔵 SV1 |
| 12 | GET | `/Admin/GetAdminAccounts` | Lấy danh sách Admin | Admin | 🔵 SV1 |
| 13 | POST | `/Admin/RejectAdmin/{id}` | Từ chối/xóa Admin mới | Admin | 🔵 SV1 |

---

#### **Nhóm 3: Products Management - 5 APIs - 🔵 SV1**

| # | Method | Endpoint | Mô tả | Quyền hạn | SV |
|----|--------|----------|-------|-----------|-----| 
| 14 | GET | `/Admin/GetProducts` | Lấy danh sách sản phẩm | Admin | 🔵 SV1 |
| 15 | POST | `/Admin/AddProduct` | Tạo sản phẩm + định mức BOM | Admin | 🔵 SV1 |
| 16 | POST | `/Admin/UpdateProduct` | Cập nhật sản phẩm + BOM | Admin | 🔵 SV1 |
| 17 | POST | `/Admin/DeleteProduct/{id}` | Xóa sản phẩm | Admin | 🔵 SV1 |
| 18 | GET | `/Admin/GetCostEstimation` | Ước tính giá thành sản phẩm | Admin | 🔵 SV1 |

---

#### **Nhóm 4: Materials Management - 4 APIs - 🔵 SV1**

| # | Method | Endpoint | Mô tả | Quyền hạn | SV |
|----|--------|----------|-------|-----------|-----| 
| 19 | GET | `/Admin/GetMaterials` | Lấy danh sách nguyên liệu | Admin | 🔵 SV1 |
| 20 | POST | `/Admin/AddMaterial` | Tạo nguyên liệu mới | Admin | 🔵 SV1 |
| 21 | POST | `/Admin/UpdateMaterial/{id}` | Cập nhật nguyên liệu | Admin | 🔵 SV1 |
| 22 | POST | `/Admin/DeleteMaterial/{id}` | Xóa nguyên liệu | Admin | 🔵 SV1 |

---

#### **Nhóm 5: Inventory & Material Requests - 3 APIs - 🔴 SV2**

| # | Method | Endpoint | Mô tả | Quyền hạn | SV |
|----|--------|----------|-------|-----------|-----| 
| 23 | GET | `/Admin/GetInventoryReport` | Tồn kho + cảnh báo tồn thấp | Admin | 🔴 SV2 |
| 24 | POST | `/Admin/ApproveMaterialRequest/{id}` | Phê duyệt cấp NL + trừ kho | Admin | 🔴 SV2 |
| 25 | POST | `/Admin/RejectMaterialRequest/{id}` | Từ chối yêu cầu cấp NL | Admin | 🔴 SV2 |

---

#### **Nhóm 6: Sample Orders - 2 APIs - 🔴 SV2**

| # | Method | Endpoint | Mô tả | Quyền hạn | SV |
|----|--------|----------|-------|-----------|-----| 
| 26 | GET | `/Admin/GetSampleOrders` | Lấy danh sách đơn hàng | Admin | 🔴 SV2 |
| 27 | POST | `/Admin/CreateSampleOrder` | Tạo đơn hàng mẫu (auto tính chi phí) | Admin | 🔴 SV2 |

---

#### **Nhóm 7: Assignments - 3 APIs - 🔴 SV2**

| # | Method | Endpoint | Mô tả | Quyền hạn | SV |
|----|--------|----------|-------|-----------|-----| 
| 28 | GET | `/Admin/GetAssignments` | Lấy danh sách giao công | Admin | 🔴 SV2 |
| 29 | POST | `/Admin/CreateAssignment` | Tạo giao công + auto trừ kho BOM | Admin | 🔴 SV2 |
| 30 | GET | `/Admin/GetAssignmentDetails/{id}` | Chi tiết giao công + NL cấp | Admin | 🔴 SV2 |

---

#### **Nhóm 8: Submissions - 2 APIs - 🔴 SV2**

| # | Method | Endpoint | Mô tả | Quyền hạn | SV |
|----|--------|----------|-------|-----------|-----| 
| 31 | GET | `/Admin/GetSubmissions` | Lấy danh sách nộp hàng chưa duyệt | Admin | 🔴 SV2 |
| 32 | POST | `/Admin/ApproveSubmission` | Duyệt KCS + auto tạo Payment + Penalty | Admin | 🔴 SV2 |

---

#### **Nhóm 9: Payments - 1 API - 🔴 SV2**

| # | Method | Endpoint | Mô tả | Quyền hạn | SV |
|----|--------|----------|-------|-----------|-----| 
| 33 | GET | `/Admin/GetPayments` | Danh sách thanh toán lương | Admin | 🔴 SV2 |

---

#### **Nhóm 10: Penalties - 4 APIs - 🔴 SV2**

| # | Method | Endpoint | Mô tả | Quyền hạn | SV |
|----|--------|----------|-------|-----------|-----| 
| 34 | GET | `/Admin/GetPenalties` | Lấy danh sách phạt Active | Admin | 🔴 SV2 |
| 35 | POST | `/Admin/CreatePenalty` | Tạo phạt thủ công | Admin | 🔴 SV2 |
| 36 | POST | `/Admin/DeductPenalty/{id}` | Thanh toán phạt + convert defect→good | Admin | 🔴 SV2 |
| 37 | POST | `/Admin/RejectSubmission` | Từ chối nộp hàng | Admin | 🔴 SV2 |

---

#### **Nhóm 11: Finished Products, Reports & Admin - 4 APIs - 🔴 SV2**

| # | Method | Endpoint | Mô tả | Quyền hạn | SV |
|----|--------|----------|-------|-----------|-----| 
| 38 | GET | `/Admin/GetFinishedProducts` | Danh sách thành phẩm (FinishedStock) | Admin | � SV1 |
| 39 | GET | `/Admin/GetProfitReport` | Báo cáo lợi nhuận theo đơn | Admin | 🔵 SV1 |
| 40 | GET | `/Admin/Index` | Dashboard Admin (thống kê) | Admin | 🔴 SV2 |
| 41 | POST | `/Admin/CheckOverdue` | Kiểm tra giao công quá hạn + auto phạt | Admin | 🔴 SV2 |
| 42 | POST | `/Admin/ResetDatabase` | Xóa toàn bộ dữ liệu test | Admin | 🔴 SV2 |
| 43 | GET | `/Admin/GetMaterialRequests` | Lấy danh sách yêu cầu cấp NL | Admin | 🔴 SV2 |

---

#### **Nhóm 12: User Dashboard - 4 APIs - 🔴 SV2**

| # | Method | Endpoint | Mô tả | Quyền hạn | SV |
|----|--------|----------|-------|-----------|-----| 
| 44 | GET | `/User/GetMyAssignments` | Lấy danh sách giao việc của tôi | User | 🔴 SV2 |
| 45 | POST | `/User/SubmitWork` | Nộp sản phẩm | User | 🔴 SV2 |
| 46 | GET | `/User/GetMyProgress` | Lấy tiến độ chi tiết + lương + phạt | User | 🔴 SV2 |
| 47 | GET | `/User/GetMyPenalties` | Lấy danh sách phạt của tôi | User | 🔴 SV2 |

---

#### **Nhóm 13: Utilities - 3 APIs - 🔴 SV2**

| # | Method | Endpoint | Mô tả | Quyền hạn | SV |
|----|--------|----------|-------|-----------|-----| 
| 48 | POST | `/Admin/ImportMaterial/{id}` | Nhập kho + tạo transaction | Admin | 🔴 SV2 |
| 49 | GET | `/Admin/GetMaterialTransactions` | Lấy lịch sử giao dịch | Admin | 🔴 SV2 |
| 50 | GET | `/Admin/Inventory` | Trang quản lý tồn kho | Admin | 🔴 SV2 |

---

**📊 TỔNG KẾT: 50 APIs**
- 🔵 **SV1: 24 APIs** (Auth 7 + User Mgmt 6 + Products 5 + Materials 4 + Reports 2)
- 🔴 **SV2: 26 APIs** (Inventory 3 + SampleOrders 2 + Assignments 3 + Submissions 2 + Payments 1 + Penalties 4 + Reports & Admin 4 + UserDashboard 4)

---

## PHẦN VI: CẤU TRÚC REQUEST/RESPONSE MẪU

### 6.1. Ví dụ 1: Thêm Nguyên Liệu (POST `/Admin/AddMaterial`)

#### **Mô tả:**
Endpoint này được sử dụng khi Admin muốn thêm 1 loại nguyên liệu mới vào hệ thống.

#### **Request (HTTP POST):**

```http
POST /Admin/AddMaterial HTTP/1.1
Host: localhost:5000
Content-Type: application/json
Authorization: Cookie authentication (tự động)

{
  "name": "Sợi lụa PE",
  "unit": "kg",
  "unitPrice": 85000,
  "minStock": 10
}
```

**Phần tử chi tiết:**

| Trường | Kiểu | Bắt buộc | Mô tả | Ví dụ |
|--------|------|---------|-------|--------|
| `name` | String | ✅ | Tên nguyên liệu | "Sợi lụa PE" |
| `unit` | String | ✅ | Đơn vị (kg, m, l, cái, v.v) | "kg" |
| `unitPrice` | Decimal | ✅ | Giá 1 đơn vị (VND) | 85000 |
| `minStock` | Float | ✅ | Mức tồn kho tối thiểu | 10 |

#### **Response (HTTP 200 - Success):**

```json
{
  "success": true,
  "message": "Them vat tu thanh cong!"
}
```

#### **Response (HTTP 400 - Validation Error):**

```json
{
  "errors": ["Thiếu thông tin vật tư"]
}
```

**Code xử lý (AdminController.cs):**
```csharp
[HttpPost]
public async Task<IActionResult> AddMaterial([FromBody] MaterialDto model)
{
    if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.Unit))
        return BadRequest("Thiếu thông tin vật tư");

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
```

---

### 6.2. Ví dụ 2: Lấy Danh Sách Nguyên Liệu (GET `/Admin/GetMaterials`)

#### **Mô tả:**
Endpoint này lấy toàn bộ danh sách nguyên liệu hiện có trong kho, kèm theo thông tin tồn kho, giá, và cảnh báo nếu tồn kho dưới mức tối thiểu.

#### **Request (HTTP GET):**

```http
GET /Admin/GetMaterials HTTP/1.1
Host: localhost:5000
Content-Type: application/json
Authorization: Cookie authentication (tự động)
```

#### **Response (HTTP 200 - Success):**

```json
[
  {
    "id": 1,
    "name": "Sợi lụa PE",
    "unit": "kg",
    "stockQuantity": 8,
    "unitPrice": 85000,
    "minStock": 10,
    "isLowStock": true
  },
  {
    "id": 2,
    "name": "Hạt cườm thủy tinh",
    "unit": "cái",
    "stockQuantity": 500,
    "unitPrice": 2500,
    "minStock": 100,
    "isLowStock": false
  },
  {
    "id": 3,
    "name": "Keo dán vải",
    "unit": "l",
    "stockQuantity": 3,
    "unitPrice": 45000,
    "minStock": 5,
    "isLowStock": true
  }
]
```

**Phần tử chi tiết (trên mỗi object):**

| Trường | Kiểu | Mô tả | Ví dụ |
|--------|------|-------|--------|
| `id` | Int | Mã nguyên liệu (khóa chính) | 1 |
| `name` | String | Tên nguyên liệu | "Sợi lụa PE" |
| `unit` | String | Đơn vị tính | "kg" |
| `stockQuantity` | Float | Số lượng tồn kho hiện tại | 8 |
| `unitPrice` | Decimal | Giá 1 đơn vị (VND) | 85000 |
| `minStock` | Float | Mức tồn kho tối thiểu | 10 |
| `isLowStock` | Boolean | Cảnh báo tồn kho thấp (true/false) | true |

#### **Response (HTTP 400 - Chưa xác thực):**

```json
{
  "error": "Unauthorized"
}
```

**Code xử lý (AdminController.cs):**
```csharp
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
```

---

### 6.3. Bảng tóm tắt phương thức HTTP

| HTTP Method | Ý nghĩa | Ví dụ Endpoint |
|------------|---------|-----------------|
| **GET** | Lấy dữ liệu (không thay đổi) | `GET /Admin/GetMaterials` |
| **POST** | Thêm/tạo dữ liệu mới | `POST /Admin/AddMaterial` |
| **PUT** | Cập nhật dữ liệu (toàn bộ) | (không dùng trong hệ thống này) |
| **POST** | Cập nhật dữ liệu (từng trường) | `POST /Admin/UpdateMaterial/{id}` |
| **DELETE** | Xóa dữ liệu | `POST /Admin/DeleteMaterial/{id}` |

---

## PHẦN VII: CHI TIẾT LOGIC CÁC THUẬT TOÁN

### 7.1. Thuật toán: Giao Công (CreateAssignment) - 🔴 SV2

**Mục đích:** Giao việc cho hộ gia công + tự động trừ kho nguyên liệu theo BOM

**Các bước thực hiện:**

**Bước 1:** Kiểm tra hợp lệ
```
- ✅ Kiểm tra User có tồn tại & IsActive = true
- ✅ Kiểm tra Product có tồn tại
- ✅ Kiểm tra Product có BOM (ProductMaterial) không
```

**Bước 2:** Kiểm tra kho nguyên liệu đủ
```
FOREACH ProductMaterial của Product:
  - Tính NL cần: RequiredQty = ProductMaterial.QuantityRequired × QuantityAssigned
  - IF Material.StockQuantity < RequiredQty:
      RETURN ERROR "Nguyên liệu không đủ"
```

**Bước 3:** Tạo Assignment
```
INSERT INTO Assignments:
  - UserId = userId
  - ProductId = productId
  - QuantityAssigned = số lượng cần làm
  - AssignedDate = DateTime.Now
  - DueDate = ngày deadline
  - Status = "Pending"
  - SampleOrderId = (nếu có)
```

**Bước 4:** Trừ kho & tạo AssignmentMaterial
```
FOREACH ProductMaterial của Product:
  - INSERT INTO AssignmentMaterials:
      • AssignmentId = newAssignment.Id
      • MaterialId = ProductMaterial.MaterialId
      • QuantityGiven = ProductMaterial.QuantityRequired × QuantityAssigned
      
  - UPDATE Materials:
      • StockQuantity -= QuantityGiven
```

**Bước 5:** Ghi log nhập/xuất kho
```
FOREACH Material được trừ:
  - INSERT INTO MaterialTransactions:
      • MaterialId = id
      • TransactionType = "Export"
      • Quantity = QuantityGiven
      • TransactionDate = DateTime.Now
      • ReferenceId = Assignment.Id
```

**Bước 6:** Gửi thông báo
```
INSERT INTO Notifications:
  - UserId = userId
  - Title = "Có giao việc mới"
  - Message = "Sản phẩm: " + Product.Name + ", SL: " + QuantityAssigned
  - Type = "assignment"
  - CreatedDate = DateTime.Now
```

**Ví dụ nhỏ:**
```
Giao công cho Hộ #5:
- Sản phẩm: Bộ hạt dâu tây (ID=2)
- Số lượng: 50 cái
- BOM sản phẩm:
  • Sợi lụa PE: 200g/cái → cần 10kg
  • Hạt cườm: 5 viên/cái → cần 250 viên

Kiểm tra kho:
✅ Sợi lụa PE: 15kg ≥ 10kg → OK
✅ Hạt cườm: 340 viên ≥ 250 viên → OK

Thực hiện:
- Tạo Assignment #23 (Status=Pending)
- AssignmentMaterial #45: MaterialId=1, QuantityGiven=10kg
- AssignmentMaterial #46: MaterialId=2, QuantityGiven=250 viên
- Material #1: StockQuantity: 15kg → 5kg
- Material #2: StockQuantity: 340 → 90 viên
- MaterialTransaction #101: Export 10kg Sợi lụa PE
- MaterialTransaction #102: Export 250 viên Hạt cườm
- Notification #89: Gửi cho User #5 "Có giao việc mới"

KẾT QUẢ: ✅ Giao công thành công, Hộ #5 nhận 50 đơn hàng + 10kg Sợi + 250 viên Hạt
```

---

### 7.2. Thuật toán: Duyệt Chất Lượng (ApproveSubmission) - 🔴 SV2

**Mục đích:** Duyệt nộp hàng + tự động tạo thanh toán & phạt

**Các bước thực hiện:**

**Bước 1:** Kiểm tra Submission hợp lệ
```
- ✅ Kiểm tra Submission tồn tại
- ✅ Kiểm tra Status = "Pending"
- ✅ Kiểm tra QuantityGood + QuantityDefect = QuantitySubmitted
```

**Bước 2:** IF Duyệt (Status = Approved)

```
2a. Cập nhật Submission
    UPDATE Submissions:
      - Status = "Approved"
    
2b. Cập nhật tiến độ giao công
    UPDATE Assignments:
      - CompletedQuantity += Submission.QuantityGood
      - IF CompletedQuantity ≥ QuantityAssigned:
          • Status = "Completed"
    
    UPDATE SampleOrders:
      - CompletedQuantity += Submission.QuantityGood

2c. Tạo Payment (thanh toán lương)
    Amount = Submission.QuantityGood × Product.UnitPrice
    
    INSERT INTO Payments:
      - UserId = Assignment.UserId
      - SubmissionId = Submission.Id
      - Amount = tính toán trên
      - PaymentDate = DateTime.Now
      - Status = "Paid"

2d. IF Submission.QuantityDefect > 0: Tạo Penalty (phạt lỗi)
    PenaltyAmount = Submission.QuantityDefect × Product.UnitPrice × 0.5
    
    INSERT INTO Penalties:
      - AssignmentId = Submission.AssignmentId
      - UserId = Assignment.UserId
      - Reason = "QualityFail"
      - Amount = PenaltyAmount
      - DefectiveQuantity = Submission.QuantityDefect
      - Status = "Active"
      - SubmissionId = Submission.Id
      - CreatedDate = DateTime.Now

2e. Cộng thành phẩm tốt vào finished stock
    UPDATE Products:
      - FinishedStock += Submission.QuantityGood

2f. Gửi thông báo cho hộ gia công
    IF Amount > 0:
        INSERT Notifications: "Lương của bạn: " + Amount
    IF Penalty.Amount > 0:
        INSERT Notifications: "Bạn có phạt: " + Penalty.Amount
```

**Bước 3:** ELSE Từ chối (Status = Rejected)

```
3a. Cập nhật Submission
    UPDATE Submissions:
      - Status = "Rejected"

3b. Hoàn lại nguyên liệu (nếu bị từ chối)
    FOREACH AssignmentMaterial:
      - UPDATE Materials:
          • StockQuantity += QuantitySubmitted
      - INSERT MaterialTransaction:
          • TransactionType = "Import" (hoàn lại)
          • Quantity = QuantitySubmitted

3c. Gửi thông báo
    INSERT Notifications: "Nộp hàng bị từ chối"
```

**Ví dụ nhỏ:**
```
Submission #67: Hộ #5 nộp 50 sản phẩm
- QuantityGood = 48 cái
- QuantityDefect = 2 cái
- Unit Price (sản phẩm) = 50,000 VND

Xử lý duyệt (Approved):

1️⃣ Tính lương:
   Amount = 48 × 50,000 = 2,400,000 VND
   → INSERT Payment #156: UserId=5, Amount=2,400,000
   
2️⃣ Tính phạt lỗi:
   PenaltyAmount = 2 × 50,000 × 0.5 = 50,000 VND
   → INSERT Penalty #78: UserId=5, Amount=50,000, DefectiveQty=2
   
3️⃣ Cộng thành phẩm:
   Product.FinishedStock += 48
   
4️⃣ Gửi thông báo:
   - "Lương của bạn: 2,400,000 VND"
   - "Bạn có phạt: 50,000 VND (2 sản phẩm lỗi)"

KẾT QUẢ:
✅ Payment #156: 2,400,000 VND (Paid)
✅ Penalty #78: 50,000 VND (Active) → Cần thanh toán để tha bổng
✅ FinishedStock +48
```

---

### 7.3. Thuật toán: Thanh Toán Phạt (DeductPenalty) - 🔴 SV2

**Mục đích:** Thanh toán phạt + chuyển sản phẩm lỗi thành đạt (tha bổng)

**Các bước thực hiện:**

**Bước 1:** Kiểm tra Penalty
```
- ✅ Kiểm tra Penalty tồn tại
- ✅ Kiểm tra Status = "Active"
- ✅ Kiểm tra User.Balance ≥ Penalty.Amount
```

**Bước 2:** Thanh toán phạt
```
UPDATE Penalties:
  - Status = "Paid"
  - PaidDate = DateTime.Now

UPDATE Users:
  - Balance -= Penalty.Amount
```

**Bước 3:** Tha bổng (chuyển lỗi → đạt)
```
IF Penalty.Reason = "QualityFail":
  
  Tìm Submission liên quan:
  submission = Penalties.SubmissionId
  
  UPDATE Submissions:
    - QuantityGood += Penalty.DefectiveQuantity
    - QuantityDefect -= Penalty.DefectiveQuantity
  
  Cộng thêm Payment (lương cho sản phẩm đã tha):
  AdditionalPayment = Penalty.DefectiveQuantity × Product.UnitPrice
  
  INSERT INTO Payments:
    - UserId = Penalty.UserId
    - Amount = AdditionalPayment
    - Status = "Paid"
```

**Bước 4:** Gửi thông báo
```
INSERT Notifications:
  - "Phạt của bạn đã được thanh toán"
  - "Lương thêm từ tha bổng: " + AdditionalPayment
```

**Ví dụ nhỏ:**
```
Penalty #78: Hộ #5, 50,000 VND (2 sản phẩm lỗi)

Thanh toán phạt:

User #5 Current Balance: 500,000 VND

1️⃣ Kiểm tra:
   Balance: 500,000 ≥ Penalty: 50,000 ✅
   
2️⃣ Trừ tiền:
   User.Balance: 500,000 → 450,000 VND
   Penalty.Status: Active → Paid
   
3️⃣ Tha bổng:
   Submission #67 (liên quan):
   - QuantityGood: 48 → 50
   - QuantityDefect: 2 → 0
   
   Lương thêm = 2 × 50,000 = 100,000 VND
   → INSERT Payment #157: 100,000 VND
   
4️⃣ Thông báo:
   "Phạt 50,000 đã thanh toán, nhận lương thêm 100,000"

KẾT QUẢ:
✅ User.Balance: 450,000 (trừ phạt)
✅ Penalty.Status: Paid
✅ Submission: 50 sản phẩm đạt (0 lỗi)
✅ Payment +100,000 (tha bổng)
```

---

### 7.4. Thuật toán: Tính Lợi Nhuận (CalculateProfit) - 🔴 SV2

**Mục đích:** Tính tổng lợi nhuận của từng đơn hàng

**Công thức:**
```
SalesProfit = (QuantityGood × SellingPrice) - (QuantityGood × CostPerUnit)

CostPerUnit = Product.UnitPrice + Σ(Material.UnitPrice × ProductMaterial.QuantityRequired)

PenaltyRevenue = Σ(Penalty.Amount) - Σ(Payment.Amount từ tha bổng)

TotalProfit = SalesProfit + PenaltyRevenue
```

**Các bước:**

**Bước 1:** Lấy dữ liệu đơn hàng
```
Lấy SampleOrder + tất cả Assignments
Lấy tất cả Submissions (Approved)
Tính QuantityGood = SUM(Submission.QuantityGood)
```

**Bước 2:** Tính giá vốn
```
CostPerUnit = Product.UnitPrice 
             + SUM(ProductMaterial.QuantityRequired × Material.UnitPrice)
```

**Bước 3:** Tính lợi nhuận bán hàng
```
SalesProfit = QuantityGood × (SellingPrice - CostPerUnit)
```

**Bước 4:** Tính doanh thu từ phạt
```
PenaltyRevenue = SUM(Penalty.Amount) 
               - SUM(Payment từ tha bổng)
```

**Bước 5:** Lưu kết quả
```
INSERT INTO Profits:
  - SampleOrderId
  - QuantityGood
  - QuantityDefect
  - SellingPrice
  - CostPrice = CostPerUnit
  - SalesProfit
  - PenaltyRevenue
  - TotalProfit = SalesProfit + PenaltyRevenue
```

**Ví dụ nhỏ:**
```
SampleOrder #10: "Bộ hạt dâu tây"
- Số lượng đặt: 100 cái
- Số lượng đạt duyệt: 95 cái (5 lỗi)
- Giá bán: 800,000 VND/100cái = 8,000 VND/cái
- Giá công: 50,000 VND/cái

Chi phí NL:
- Sợi lụa PE: 200g/cái × 5,000 VND/kg = 1,000 VND/cái
- Hạt cườm: 5 viên/cái × 500 VND = 2,500 VND/cái

Giá vốn/cái = 50,000 + 1,000 + 2,500 = 53,500 VND

Lợi nhuận bán:
SalesProfit = 95 × (8,000 - 53,500) = 95 × (-45,500) = -4,322,500 VND ❌
(LỖ - giá bán quá thấp!)

Doanh thu phạt:
- Penalty tạo: 5 × 53,500 × 0.5 = 133,750 VND
- Payment tha bổng: 0 (khách không chịu thanh toán)
→ PenaltyRevenue = 0 VND

Tổng profit:
TotalProfit = -4,322,500 + 0 = -4,322,500 VND ❌ KINH DOANH LỖ
```

---

### 7.5. Thuật toán: Kiểm Tra Quá Hạn (CheckOverdue) - 🔵 SV1

**Mục đích:** Kiểm tra giao công quá hạn + tự động tạo phạt

**Các bước thực hiện:**

**Bước 1:** Tìm giao công quá hạn
```
SELECT * FROM Assignments
WHERE DueDate < DateTime.Now AND Status = "InProgress"
```

**Bước 2:** Cập nhật trạng thái
```
FOREACH Assignment quá hạn:
  UPDATE Assignments:
    - Status = "Overdue"
```

**Bước 3:** Kiểm tra & tạo phạt quá hạn
```
FOREACH Assignment quá hạn:
  - Kiểm tra đã có Penalty với Reason = "Overdue" chưa
  - IF chưa có:
      INSERT INTO Penalties:
        • AssignmentId
        • UserId = Assignment.UserId
        • Reason = "Overdue"
        • Amount = 0 (Admin tự set sau)
        • Note = "Đơn giao việc quá hạn"
        • Status = "Active"
        • CreatedDate = DateTime.Now
```

**Ví dụ nhỏ:**
```
Assignment #25 (Hộ #5):
- DueDate: 2026-03-10
- Status: "InProgress"
- Hôm nay: 2026-03-17

Kiểm tra:
✅ 2026-03-17 > 2026-03-10 → Quá hạn

Thực hiện:
1️⃣ UPDATE Assignment #25:
   Status: "InProgress" → "Overdue"
   
2️⃣ Kiểm tra Penalty:
   SELECT * WHERE AssignmentId=25 AND Reason="Overdue"
   → Chưa có
   
3️⃣ Tạo Penalty:
   INSERT Penalty #80:
   - AssignmentId: 25
   - UserId: 5
   - Reason: "Overdue"
   - Amount: 0 (Admin sẽ set)
   - Status: "Active"

KẾT QUẢ:
✅ Assignment.Status: Overdue
✅ Penalty #80 tạo: 0 VND (chờ Admin xác định)
```

---

### 7.6. Thuật toán: Tạo Đơn Hàng Mẫu (CreateSampleOrder) - 🔴 SV2

**Mục đích:** Tạo đơn hàng + auto sinh mã đơn + tính giá thành dự kiến

**Các bước thực hiện:**

**Bước 1:** Auto sinh mã đơn nếu trống
```
IF OrderCode (trống hoặc null):
  - Lấy MaxId từ SampleOrders
  - nextNumber = MaxId + 1
  - OrderCode = "SO" + PADLEFT(nextNumber, 4, "0")
  
VÍ DỤ: SO0001, SO0002, ..., SO9999
```

**Bước 2:** Kiểm tra mã không trùng
```
SELECT * FROM SampleOrders WHERE OrderCode = input_code
IF tồn tại:
  RETURN ERROR "Mã đơn đã tồn tại"
```

**Bước 3:** Tính giá thành dự kiến
```
FOREACH ProductMaterial của Product:
  MaterialCost += ProductMaterial.QuantityRequired × Material.UnitPrice

TotalCostPerUnit = Product.UnitPrice + MaterialCost

EstimatedCost = TotalCostPerUnit × TotalQuantity
```

**Bước 4:** Tạo SampleOrder
```
INSERT INTO SampleOrders:
  - OrderCode = (auto sinh)
  - CustomerName
  - Description
  - ProductId
  - TotalQuantity
  - SellingPrice
  - TargetDate
  - EstimatedCost = tính toán
  - Status = "Draft"
  - CreatedDate = DateTime.Now
```

**Ví dụ nhỏ:**
```
Tạo đơn hàng: Bộ hạt dâu tây

Input:
- OrderCode: "" (trống)
- CustomerName: "Cửa hàng Hana Seoul"
- ProductId: 2
- TotalQuantity: 100
- SellingPrice: 800,000 (cho 100 cái)
- TargetDate: 2026-04-15

Xử lý:

1️⃣ Auto sinh mã:
   MaxId = 10 → nextId = 11
   OrderCode = "SO0011"
   
2️⃣ Tính giá:
   Product #2 (Bộ hạt dâu):
   - UnitPrice (tiền công): 50,000 VND/cái
   - Material #1 (Sợi): 200g × 5,000/kg = 1,000 VND/cái
   - Material #2 (Hạt): 5 viên × 500 = 2,500 VND/cái
   
   TotalCostPerUnit = 50,000 + 1,000 + 2,500 = 53,500 VND/cái
   EstimatedCost = 53,500 × 100 = 5,350,000 VND
   
3️⃣ Tạo đơn:
   INSERT SampleOrder #11:
   - OrderCode: SO0011
   - CustomerName: Cửa hàng Hana Seoul
   - TotalQuantity: 100
   - SellingPrice: 800,000
   - EstimatedCost: 5,350,000
   - Status: Draft

KẾT QUẢ:
✅ OrderCode: SO0011 (tự động)
✅ EstimatedCost: 5,350,000 VND
✅ Status: Draft (chờ giao công)
```

---

### 7.7. Thuật toán: Duyệt Yêu Cầu Cấp Nguyên Liệu (ApproveMaterialRequest) - 🔵 SV1

**Mục đích:** Duyệt cấp thêm NL cho hộ gia công + trừ kho

**Các bước thực hiện:**

**Bước 1:** Kiểm tra yêu cầu
```
- ✅ MaterialRequest tồn tại
- ✅ Status = "Pending"
- ✅ Material tồn tại
```

**Bước 2:** Kiểm tra tồn kho
```
IF Material.StockQuantity < QuantityRequested:
  RETURN ERROR "Không đủ nguyên liệu"
```

**Bước 3:** Trừ kho
```
UPDATE Materials:
  - StockQuantity -= QuantityRequested
```

**Bước 4:** Cập nhật yêu cầu
```
UPDATE MaterialRequests:
  - Status = "Approved"
  - ApprovedDate = DateTime.Now
```

**Bước 5:** Ghi log giao dịch
```
INSERT INTO MaterialTransactions:
  - MaterialId
  - TransactionType = "Export"
  - Quantity = QuantityRequested
  - TransactionDate = DateTime.Now
  - ReferenceId = AssignmentId (liên quan)
```

**Ví dụ nhỏ:**
```
MaterialRequest #8:
- AssignmentId: 25 (Hộ #5, Bộ hạt dâu)
- MaterialId: 1 (Sợi lụa)
- QuantityRequested: 5kg
- Status: Pending

Duyệt:

1️⃣ Kiểm tra kho:
   Material #1: 8kg ≥ 5kg ✅
   
2️⃣ Trừ kho:
   Material #1: 8kg → 3kg
   
3️⃣ Cập nhật yêu cầu:
   MaterialRequest #8:
   - Status: Pending → Approved
   - ApprovedDate: 2026-03-17 10:30
   
4️⃣ Ghi log:
   INSERT MaterialTransaction #256:
   - MaterialId: 1
   - TransactionType: Export
   - Quantity: 5kg
   - ReferenceId: 25

KẾT QUẢ:
✅ Material.StockQuantity: 3kg (trừ 5kg)
✅ MaterialRequest.Status: Approved
✅ MaterialTransaction ghi log
```

---

### 7.8. Thuật toán: Nhập Kho Nguyên Liệu (ImportMaterial) - 🔵 SV1

**Mục đích:** Nhập kho + lưu lịch sử giao dịch

**Các bước thực hiện:**

**Bước 1:** Cập nhật tồn kho
```
UPDATE Materials:
  - StockQuantity += ImportQuantity
```

**Bước 2:** Ghi log giao dịch
```
INSERT INTO MaterialTransactions:
  - MaterialId
  - TransactionType = "Import"
  - Quantity = ImportQuantity
  - TransactionDate = DateTime.Now
```

**Ví dụ nhỏ:**
```
Nhập Sợi lụa PE (Material #1):
- Số lượng: 20kg
- Giá: 5,000 VND/kg

Xử lý:

1️⃣ Cập nhật kho:
   Material #1:
   StockQuantity: 3kg → 23kg
   
2️⃣ Ghi log:
   INSERT MaterialTransaction #257:
   - MaterialId: 1
   - TransactionType: Import
   - Quantity: 20kg
   - TransactionDate: 2026-03-17 14:00

KẾT QUẢ:
✅ Material.StockQuantity: 23kg (+20kg)
✅ MaterialTransaction ghi log (Import)
```

---

### 7.9. Thuật toán: Tính Giá Thành & Lợi Nhuận (GetCostEstimation) - 🔵 SV1

**Mục đích:** Hiển thị chi phí & lợi nhuận chi tiết từng sản phẩm & đơn hàng

**Các bước thực hiện:**

**Bước 1:** Tính chi phí sản phẩm
```
FOREACH Product:
  MaterialCost = SUM(ProductMaterial.QuantityRequired × Material.UnitPrice)
  
  TotalCostPerUnit = Product.UnitPrice + MaterialCost
```

**Bước 2:** Tính lợi nhuận dự kiến
```
FOREACH SampleOrder:
  TotalCostPerUnit = (như trên)
  
  EstimatedProfit = (SellingPrice - TotalCostPerUnit) × TotalQuantity
  
  (Lợi nhuận nếu bán hết đủ số lượng)
```

**Bước 3:** Tính lợi nhuận thực tế
```
FOREACH SampleOrder:
  - Lấy QuantityGood từ tất cả Submissions (Status=Approved)
  - ActualProfit = (SellingPrice - TotalCostPerUnit) × QuantityGood
```

**Ví dụ nhỏ:**
```
SampleOrder #10: "Bộ hạt dâu tây"
- TotalQuantity: 100 cái
- SellingPrice: 800,000 (8,000/cái)
- QuantityGood (thực tế): 95 cái
- TotalCostPerUnit: 53,500 VND/cái

Tính toán:

1️⃣ Lợi nhuận dự kiến:
   EstimatedProfit = (8,000 - 53,500) × 100
                   = -45,500 × 100
                   = -4,550,000 VND ❌ (dự kiến LỖ)
   
2️⃣ Lợi nhuận thực tế:
   ActualProfit = (8,000 - 53,500) × 95
                = -45,500 × 95
                = -4,322,500 VND ❌ (thực tế LỖ)

KẾT QUẢ:
⚠️ Cân nhắc giá bán - quá thấp so với giá vốn!
```

---

### 7.10. Thuật toán: Từ Chối Nộp Hàng (RejectSubmission) - 🔴 SV2

**Mục đích:** Từ chối nộp hàng + hoàn lại Assignment status

**Các bước thực hiện:**

**Bước 1:** Kiểm tra Submission
```
- ✅ Submission tồn tại
- ✅ Status = "Pending"
```

**Bước 2:** Cập nhật Submission
```
UPDATE Submissions:
  - Status = "Rejected"
```

**Bước 3:** Reset Assignment status
```
UPDATE Assignments:
  - Status = "InProgress" (cho làm lại)
```

**Ví dụ nhỏ:**
```
Submission #67: Hộ #5, 50 sản phẩm
- Status: Pending

Từ chối:

1️⃣ Cập nhật Submission:
   Status: Pending → Rejected
   
2️⃣ Reset Assignment:
   Assignment #25:
   Status: Pending → InProgress (hộ có thể nộp lại)

KẾT QUẢ:
✅ Submission #67: Rejected
✅ Assignment #25: InProgress (hộ có thể nộp lại)
```

---

### 7.11. Thuật toán: Dashboard Admin (Index) - 🔵 SV1

**Mục đích:** Hiển thị thống kê tổng hợp + cảnh báo

**Các bước thực hiện:**

**Bước 1:** Tính toán thống kê
```
TotalUsers = COUNT(Users WHERE RoleId=2 AND IsActive=true)
TotalMaterials = COUNT(Materials)
TotalAssignments = COUNT(Assignments)
TotalPendingSubmissions = COUNT(Submissions WHERE Status="Pending")
TotalProducts = COUNT(Products)
TotalSampleOrders = COUNT(SampleOrders WHERE Status!="Cancelled")
```

**Bước 2:** Tính lợi nhuận thực tế
```
FOREACH SampleOrder (không cancelled):
  CostPerUnit = Product.UnitPrice + SUM(Material.UnitPrice × QuantityRequired)
  QuantityGood = SUM(Submission.QuantityGood WHERE Status="Approved")
  
  ActualProfit = (SellingPrice - CostPerUnit) × QuantityGood

TotalProfit = SUM(ActualProfit của tất cả đơn)
```

**Bước 3:** Kiểm tra đơn quá hạn
```
OverdueCount = COUNT(Assignments 
               WHERE DueDate < DateTime.Now 
               AND Status = "InProgress")
```

**Bước 4:** Cảnh báo tồn kho thấp
```
LowStockCount = COUNT(Materials 
                WHERE StockQuantity <= MinStock)
```

**Bước 5:** Hiển thị Admin chờ duyệt
```
PendingAdmins = COUNT(Users 
                WHERE RoleId=1 AND IsApproved=false)
```

**Ví dụ nhỏ:**
```
Dashboard - 2026-03-17:

Thống kê:
- Tổng hộ gia công: 12
- Tổng NL: 45
- Tổng giao công: 34
- Nộp hàng chờ duyệt: 8
- Tổng sản phẩm: 8
- Tổng đơn hàng: 15

Lợi nhuận:
- SO #10 (Bộ hạt): -4,322,500 VND ❌
- SO #11 (Khác): +2,500,000 VND ✅
- Tổng: -1,822,500 VND (LỖ)

Cảnh báo:
⚠️ 7 đơn quá hạn (cần xử lý phạt)
⚠️ 3 NL tồn kho thấp (cần nhập)
⚠️ 2 Admin chờ duyệt

KẾT QUẢ:
→ Dashboard hiển thị toàn bộ metrics
```

---

## PHẦN VIII: CHI TIẾT VIEWS (28 Views)

### 8.1. Tổng quan kiến trúc View

Hệ thống sử dụng **3 Layout** phân tầng:

| Layout | Mô tả | Dùng cho |
|--------|--------|----------|
| `_Layout.cshtml` | Layout tối giản (không navbar) | Trang Auth (Login, Register) |
| `_AdminLayout.cshtml` | Sidebar 13 mục điều hướng | Tất cả trang Admin |
| `_UserLayout.cshtml` | Header navbar (Công việc, Công thức, Thu nhập) | Tất cả trang User |

---

### 8.2. Shared Views (5 file - dùng chung)

#### **1. _Layout.cshtml - 🔵 [SV1]**

**Chức năng:** Layout gốc (base layout), wrapper tối giản cho các trang xác thực.

- HTML5, `lang="vi"`, Google Fonts (Inter)
- Load `~/css/site.css`, `~/css/validation.css`
- Load `~/js/validation.js`, `~/js/toast.js`
- `@RenderBody()` + `@RenderSectionAsync("Scripts")`
- Không có navigation bar → phù hợp cho trang Login/Register

---

#### **2. _AdminLayout.cshtml - 🔵 [SV1]**

**Chức năng:** Layout Admin Panel với sidebar điều hướng đầy đủ.

- **Sidebar** (260px, nền tối `#111827`) gồm 13 mục + Logout:
  - Thống Kê (`/Admin/Index`) - chart-pie
  - Đơn Hàng Mẫu (`/Admin/SampleOrders`) - file-invoice
  - Quy Trình BOM (`/Admin/Products`) - cube
  - Vật Tư (`/Admin/Materials`) - box-open
  - Kho (`/Admin/Inventory`) - warehouse
  - Giao Việc (`/Admin/Assignments`) - truck-fast
  - Duyệt Thành Phẩm (`/Admin/Submissions`) - clipboard-check
  - Thành Phẩm (`/Admin/FinishedProducts`) - gem
  - Giá Thành (`/Admin/CostEstimation`) - calculator
  - Thanh Toán (`/Admin/Payments`) - money-bill-wave
  - Phạt (`/Admin/Penalties`) - gavel
  - Hộ Dân (`/Admin/Users`) - users
  - Quản Lý Admin (`/Admin/AdminAccounts`) - user-shield
- Font Awesome 6.4.0 cho icons
- Auto-highlight nav link active theo URL hiện tại
- `@RenderBody()` trong `<main class="content">`

---

#### **3. _UserLayout.cshtml - 🔴 [SV2]**

**Chức năng:** Layout cho hộ gia công với header navigation.

- **Header** (nền primary, flexbox):
  - Brand: "Làng Nghề Gia Công"
  - Nav links: Công Việc (`/User/Index`), Công Thức (`/User/Recipes`), Thu Nhập (`/User/MyProgress`)
  - Lời chào: "Xin chào, [FullName]" (đọc từ Claims)
  - Đăng Xuất (màu đỏ) → `/Auth/Logout`
- Responsive media query @768px
- `@RenderBody()` trong `<main class="content">`

---

#### **4. Error.cshtml - 🔴 [SV2]**

**Chức năng:** Trang hiển thị lỗi hệ thống.

- Model: `ErrorViewModel`
- Hiển thị Request ID (nếu có)
- Hướng dẫn bật Development mode để debug

---

#### **5. _ValidationScriptsPartial.cshtml - 🔴 [SV2]**

**Chức năng:** Partial view chứa jQuery validation scripts.

- Load `jquery.validate.min.js`
- Load `jquery.validate.unobtrusive.min.js`

---

### 8.3. Auth Views (4 file - xác thực) - 🔵 [SV1]

#### **6. Login.cshtml - 🔵 [SV1]**

**Chức năng:** Trang đăng nhập cho hộ gia công (User).

- Layout: `_Layout` (mặc định)
- Form POST → `/Auth/Login` (id: `loginForm`)
- Trường: Username (`data-validate="nameOnly"`), Password
- Hiển thị lỗi (`ViewBag.Error`) và thành công (`TempData["SuccessMsg"]`)
- Link: Đăng ký hộ gia công mới, Đăng ký Admin
- Validation: `validation.js` kiểm tra format username

---

#### **7. AdminLogin.cshtml - 🔵 [SV1]**

**Chức năng:** Trang đăng nhập riêng cho Admin (Cơ Sở).

- Layout: `null` (standalone HTML page)
- Form POST → `/Auth/AdminLogin` (id: `adminLoginForm`)
- Trường: Username (`data-validate="nameOnly"`), Password
- Hiển thị lỗi (`ViewBag.Error`)
- Link quay về: "Trở về Đăng nhập Dành cho Hộ Gia Công"
- Load riêng `/js/toast.js`, `/js/validation.js`

---

#### **8. Register.cshtml - 🔵 [SV1]**

**Chức năng:** Trang đăng ký tài khoản hộ gia công mới.

- Layout: `_Layout` (mặc định)
- Form POST → `/Auth/Register` (id: `registerForm`)
- Trường bắt buộc (*): Username, Password, Họ tên (`nameOnly`), SĐT (`phone`)
- Trường tùy chọn: Địa chỉ (`address`)
- Validation JS: kiểm tra tất cả trường bắt buộc + format
- Link: "Đã có tài khoản? Đăng nhập"

---

#### **9. RegisterAdmin.cshtml - 🔵 [SV1]**

**Chức năng:** Trang đăng ký tài khoản Admin mới (cần duyệt).

- Layout: `_Layout` (mặc định)
- Form POST → `/Auth/RegisterAdmin` (id: `registerAdminForm`)
- Trường bắt buộc (*): Username, Password, Họ tên, SĐT
- **Banner cảnh báo** (vàng): Tài khoản sẽ ở trạng thái "chờ duyệt" → cần Admin hiện tại phê duyệt
- Validation JS: kiểm tra username, fullname, phone
- Link: "Đã có tài khoản? Đăng nhập"

---

### 8.4. User Views (3 file - hộ gia công) - 🔴 [SV2]

#### **10. User/Index.cshtml - 🔴 [SV2]**

**Chức năng:** Dashboard chính của hộ gia công - xem công việc, nộp hàng, xem/thanh toán phạt.

- Layout: `_UserLayout`
- **Header:** "Công Việc Của Bạn" + tổng thu nhập dự kiến (`ViewBag.TotalEarnings`)
- **Banner thông báo mới** (`#newAssignmentNotification`): hiện khi phát hiện giao việc mới
- **Bảng Công Việc** (`#assignmentsTable`):
  - Cột: Mã ĐH, Sản phẩm, SL giao, Ngày giao, Hạn nộp, Trạng thái, Hành động
  - Badge trạng thái: Đang Thực Hiện (xanh), Chờ Admin Duyệt KCS (vàng), Hoàn Tất (xanh lá)
  - Nút "Nộp Hàng" (xanh) cho đơn InProgress
- **Bảng Phạt** (`#penaltiesTable`):
  - Cột: Mã ĐH, Lý do phạt, Số tiền, Trạng thái, Ngày, Hành động
  - Badge: Chưa Thanh Toán (đỏ), Đã Thanh Toán (xanh), Hủy Bỏ (xám)
  - Nút "Thanh Toán" (đỏ) cho phạt Active
- **Modal Nộp Hàng** (`#submitModal`): nhập số lượng, validate ≤ SL giao
- **AJAX:** `GET GetMyAssignments`, `GET GetMyPenalties`, `GET GetUnreadNotifications`, `POST SubmitWork`, `POST PayPenalty`
- **Auto-refresh:** 20 giây
- **Toast notification system:** success/error/warning/info
- **localStorage:** phát hiện giao việc mới bằng so sánh ID

---

#### **11. User/Recipes.cshtml - 🔴 [SV2]**

**Chức năng:** Xem công thức sản xuất (BOM) - nguyên liệu cần cho sản phẩm được giao.

- Layout: `_UserLayout`
- **Container** (`#recipesList`): hiển thị danh sách recipe cards
- **Recipe Card** (`.recipe-card`): mỗi card = 1 sản phẩm
  - Tên sản phẩm + mô tả
  - Danh sách nguyên liệu: tên, số lượng cần, đơn vị
- **Empty state:** "Bạn chưa được giao sản phẩm nào..."
- **AJAX:** `GET GetMyRecipes` (load 1 lần khi mở trang)
- Read-only, không có form hay tương tác

---

#### **12. User/MyProgress.cshtml - 🔴 [SV2]**

**Chức năng:** Xem thu nhập cá nhân + lịch sử đơn hoàn thành.

- Layout: `_UserLayout`
- **3 Stat Cards** (CSS Grid):
  - Tổng Thu Nhập (`#totalEarnings`) - xanh lá
  - Tiền Phạt (`#totalPenalties`) - đỏ
  - Thu Nhập Ròng (`#netIncome`) - xanh dương (tính client-side: earnings - penalties)
- **Bảng Lịch Sử** (`#completedOrdersTable`):
  - Cột: Mã ĐH, Sản phẩm, Số lượng, Ngày giao, Trạng thái, Chất lượng
  - Chỉ hiện đơn status = "Completed"
  - Chất lượng: "Đạt: X/Y"
- **AJAX:** `GET GetMyProgress` (load 1 lần)
- Format tiền VND: `toLocaleString('vi-VN')`

---

### 8.5. Admin Views (14 file - quản trị)

#### **13. Admin/Index.cshtml - 🔵 [SV1]**

**Chức năng:** Dashboard Admin - thống kê tổng quan hệ thống.

- Layout: `_AdminLayout`
- **12 Stat Cards** (server-side ViewBag):
  - Tổng hộ gia công, Tổng vật tư, Tổng giao việc, Nộp hàng chờ duyệt
  - Tổng sản phẩm, Tổng đơn hàng, Tổng phạt, Tổng thanh toán
  - Doanh thu lợi nhuận (SalesRevenue)
- **3 Cảnh báo** (vàng/đỏ):
  - Đơn quá hạn (OverdueAssignments)
  - Nguyên liệu tồn thấp (LowStockCount)
  - Admin chờ duyệt (PendingAdmins)
- **Nút Reset Database** (`POST /Admin/ResetDatabase`) - xác nhận 2 lần

---

#### **14. Admin/SampleOrders.cshtml - 🔵 [SV1]**

**Chức năng:** Quản lý đơn hàng mẫu - CRUD + theo dõi trạng thái.

- Layout: `_AdminLayout`
- **Bảng đơn hàng:** Mã ĐH, Khách hàng, Sản phẩm, SL, Tiến độ (progress bar), Chi phí, Trạng thái, Hành động
- **Trạng thái:** Draft (xám), Confirmed (xanh), InProduction (vàng), Completed (xanh lá), Cancelled (đỏ)
- **Modal tạo đơn:** OrderCode (auto), Khách hàng, Sản phẩm (dropdown), SL, Giá bán, Hạn, Mô tả
- **AJAX:** `GET GetSampleOrders`, `GET GetProducts`, `POST CreateSampleOrder`, `POST UpdateSampleOrderStatus`

---

#### **15. Admin/Products.cshtml - 🔵 [SV1]**

**Chức năng:** Quản lý sản phẩm + định mức BOM (Bill of Materials).

- Layout: `_AdminLayout`
- **Bảng sản phẩm:** Tên, Mô tả, Đơn giá công, Chi phí NL, Tổng giá thành, BOM chi tiết
- **Modal tạo/sửa sản phẩm:** Tên, Mô tả, Đơn giá, Danh sách NL (thêm/xóa dòng động)
- **BOM:** Dropdown chọn NL + nhập số lượng cần/SP
- **AJAX:** `GET GetProducts`, `GET GetMaterials`, `POST AddProduct`, `POST UpdateProduct`

---

#### **16. Admin/Materials.cshtml - 🔵 [SV1]**

**Chức năng:** Quản lý vật tư/nguyên liệu - CRUD + nhập kho.

- Layout: `_AdminLayout`
- **Bảng vật tư:** Tên, Đơn vị, Tồn kho, Giá, Mức tối thiểu, Cảnh báo tồn thấp
- **Modal thêm NL:** Tên, Đơn vị (dropdown 16 loại), Giá, Mức tối thiểu
- **Modal nhập kho:** Chọn NL + nhập số lượng
- **AJAX:** `GET GetMaterials`, `POST AddMaterial`, `POST ImportMaterial`, `POST DeleteMaterial`

---

#### **17. Admin/Inventory.cshtml - 🔴 [SV2]**

**Chức năng:** Dashboard kho - xem tồn kho NL + thành phẩm + lịch sử giao dịch.

- Layout: `_AdminLayout`
- **2 Bảng song song:**
  - Bảng NL: Tên, Đơn vị, Tồn kho, Giá trị, Cảnh báo tồn thấp
  - Bảng Thành phẩm: Tên, Tồn kho, Đơn giá
- **Bảng Lịch sử giao dịch** (50 gần nhất): Ngày, NL, Loại (Import/Export), SL
- **AJAX:** `GET GetInventoryReport`
- Read-only (chỉ xem)

---

#### **18. Admin/Assignments.cshtml - 🔵 [SV1]**

**Chức năng:** Quản lý giao việc - tạo giao việc cho hộ dân.

- Layout: `_AdminLayout`
- **Bảng giao việc:** Hộ dân, Sản phẩm, Mã ĐH, SL giao, SL hoàn thành, Ngày giao, Hạn nộp, Trạng thái, Quá hạn
- **Trạng thái:** Pending (xám), InProgress (xanh), Completed (xanh lá), Overdue (đỏ)
- **Modal tạo giao việc:** Dropdown Hộ dân, Sản phẩm, Đơn hàng mẫu (optional), SL, Ngày hạn
- **Auto-refresh:** 20 giây
- **AJAX:** `GET GetAssignments`, `GET GetSampleOrders`, `GET GetUsers`, `GET GetProducts`, `POST CreateAssignment`

---

#### **19. Admin/Submissions.cshtml - 🔵 [SV1]**

**Chức năng:** Duyệt KCS - xét duyệt chất lượng sản phẩm nộp.

- Layout: `_AdminLayout`
- **Bảng nộp hàng:** Hộ dân, Sản phẩm, SL nộp, Đợt, Ngày nộp, Trạng thái, Hành động
- **Trạng thái:** Pending (vàng), Approved (xanh lá), Rejected (đỏ)
- **Form duyệt inline:** Nhập SL Đạt + SL Lỗi + Ghi chú → kiểm tra Đạt + Lỗi = SL nộp
- **AJAX:** `GET GetSubmissions`, `POST ApproveSubmission`

---

#### **20. Admin/FinishedProducts.cshtml - 🔴 [SV2]**

**Chức năng:** Xem thống kê thành phẩm - tồn kho, sản xuất, lỗi, tỷ lệ đạt.

- Layout: `_AdminLayout`
- **Bảng thành phẩm:** Tên SP, Tồn kho, Tổng SX, Tổng lỗi, Tỷ lệ đạt (%)
- **Tỷ lệ đạt:** Xanh (≥90%), Vàng (≥70%), Đỏ (<70%)
- **AJAX:** `GET GetFinishedProducts`
- Read-only

---

#### **21. Admin/CostEstimation.cshtml - 🔴 [SV2]**

**Chức năng:** Phân tích giá thành + lợi nhuận chi tiết.

- Layout: `_AdminLayout`
- **Bảng 1 - Chi phí sản phẩm:** Tên SP, Tiền công, Chi phí NL (chi tiết từng NL), Tổng giá thành/SP
- **Bảng 2 - Lợi nhuận đơn hàng:** Mã ĐH, SP, SL, Giá bán, Giá vốn, LN dự kiến, LN thực tế
- **AJAX:** `GET GetCostEstimation`
- Read-only

---

#### **22. Admin/Payments.cshtml - 🔴 [SV2]**

**Chức năng:** Xem lịch sử thanh toán lương cho hộ dân.

- Layout: `_AdminLayout`
- **Tổng thanh toán** hiển thị ở header
- **Bảng thanh toán:** Hộ dân, Sản phẩm, SL đạt KCS, Đơn giá, Tổng tiền, Ngày, Trạng thái
- **AJAX:** `GET GetPayments`
- Read-only

---

#### **23. Admin/Penalties.cshtml - 🔴 [SV2]**

**Chức năng:** Quản lý phạt - tạo, hủy, kiểm tra quá hạn.

- Layout: `_AdminLayout`
- **Bảng phạt:** Hộ dân, Mã ĐH, Lý do, Số tiền, Ghi chú, Trạng thái, Ngày, Hành động
- **Trạng thái:** Active (đỏ), Waived (xám), Deducted (xanh lá)
- **Lý do:** QualityFail (Sản phẩm lỗi), Overdue (Quá hạn)
- **Nút "Kiểm tra quá hạn":** `POST CheckOverdue` - auto tạo phạt cho đơn quá hạn
- **Modal tạo phạt:** Chọn Assignment, nhập số tiền, lý do, ghi chú
- **Nút Hủy phạt:** `POST WaivePenalty`
- **AJAX:** `GET GetPenalties`, `POST CreatePenalty`, `POST WaivePenalty`, `POST CheckOverdue`

---

#### **24. Admin/Users.cshtml - 🔴 [SV2]**

**Chức năng:** Quản lý hộ dân - xem danh sách, xóa user.

- Layout: `_AdminLayout`
- **Bảng hộ dân:** Họ tên, Username, SĐT, Địa chỉ, Trạng thái, Ngày tạo, Tổng giao việc, SL hoàn thành, Thu nhập, Phạt, Thu nhập ròng
- **Nút Xóa** (chỉ xóa nếu không có giao việc active)
- **AJAX:** `GET GetUsers`, `POST DeleteUser`

---

#### **25. Admin/AdminAccounts.cshtml - 🔴 [SV2]**

**Chức năng:** Quản lý tài khoản Admin - duyệt/từ chối Admin mới.

- Layout: `_AdminLayout`
- **Bảng Admin:** Họ tên, Username, SĐT, Trạng thái duyệt, Trạng thái hoạt động, Số dư, Ngày tạo
- **Nút Duyệt** (`POST ApproveAdmin`) + **Nút Từ chối** (`POST RejectAdmin`)
- **AJAX:** `GET GetAdminAccounts`, `POST ApproveAdmin`, `POST RejectAdmin`

---

#### **26. Admin/OrderProgress.cshtml - 🔵 [SV1]**

**Chức năng:** Theo dõi tiến độ sản xuất theo đơn hàng + giao việc.

- Layout: `_AdminLayout`
- **Phần 1 - Tiến độ đơn hàng:** Progress bar % hoàn thành, chi tiết assignments của từng đơn
- **Phần 2 - Tiến độ giao việc:** Ngày còn lại, trạng thái quá hạn
- **Nút "Kiểm tra quá hạn":** `POST CheckOverdue`
- **AJAX:** `GET GetOrderProgress`, `POST CheckOverdue`

---

### 8.6. Root Views (2 file - cấu hình)

#### **27. _ViewImports.cshtml - 🔴 [SV2]**

**Chức năng:** Import namespaces + Tag Helpers cho tất cả Views.

- `@using CraftOutsourcing`
- `@using CraftOutsourcing.Models`
- `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`

---

#### **28. _ViewStart.cshtml - 🔴 [SV2]**

**Chức năng:** Set default layout cho tất cả Views.

- `Layout = "_Layout"` (mặc định)

---

### 8.7. Tóm tắt Views theo Phân Công SV

| # | View | Chức năng chính | SV |
|---|------|-----------------|-----|
| 1 | `Shared/_Layout.cshtml` | Layout gốc (Auth pages) | 🔵 SV1 |
| 2 | `Shared/_AdminLayout.cshtml` | Layout Admin (sidebar) | 🔵 SV1 |
| 3 | `Shared/_UserLayout.cshtml` | Layout User (header nav) | 🔴 SV2 |
| 4 | `Shared/Error.cshtml` | Trang lỗi hệ thống | 🔴 SV2 |
| 5 | `Shared/_ValidationScriptsPartial.cshtml` | jQuery Validation | 🔴 SV2 |
| 6 | `Auth/Login.cshtml` | Đăng nhập User | 🔵 SV1 |
| 7 | `Auth/AdminLogin.cshtml` | Đăng nhập Admin | 🔵 SV1 |
| 8 | `Auth/Register.cshtml` | Đăng ký User | 🔵 SV1 |
| 9 | `Auth/RegisterAdmin.cshtml` | Đăng ký Admin | 🔵 SV1 |
| 10 | `User/Index.cshtml` | Dashboard User (công việc + phạt) | 🔴 SV2 |
| 11 | `User/Recipes.cshtml` | Công thức sản xuất (BOM) | 🔴 SV2 |
| 12 | `User/MyProgress.cshtml` | Thu nhập + lịch sử hoàn thành | 🔴 SV2 |
| 13 | `Admin/Index.cshtml` | Dashboard Admin (thống kê) | 🔵 SV1 |
| 14 | `Admin/SampleOrders.cshtml` | Quản lý đơn hàng mẫu | 🔵 SV1 |
| 15 | `Admin/Products.cshtml` | Quản lý sản phẩm + BOM | 🔵 SV1 |
| 16 | `Admin/Materials.cshtml` | Quản lý vật tư | 🔵 SV1 |
| 17 | `Admin/Inventory.cshtml` | Dashboard kho | 🔴 SV2 |
| 18 | `Admin/Assignments.cshtml` | Giao việc | 🔵 SV1 |
| 19 | `Admin/Submissions.cshtml` | Duyệt KCS | 🔵 SV1 |
| 20 | `Admin/FinishedProducts.cshtml` | Thành phẩm | 🔴 SV2 |
| 21 | `Admin/CostEstimation.cshtml` | Giá thành & lợi nhuận | 🔴 SV2 |
| 22 | `Admin/Payments.cshtml` | Thanh toán lương | 🔴 SV2 |
| 23 | `Admin/Penalties.cshtml` | Quản lý phạt | 🔴 SV2 |
| 24 | `Admin/Users.cshtml` | Quản lý hộ dân | 🔴 SV2 |
| 25 | `Admin/AdminAccounts.cshtml` | Quản lý tài khoản Admin | 🔴 SV2 |
| 26 | `Admin/OrderProgress.cshtml` | Tiến độ đơn hàng | 🔵 SV1 |
| 27 | `_ViewImports.cshtml` | Import namespaces | 🔴 SV2 |
| 28 | `_ViewStart.cshtml` | Default layout config | 🔴 SV2 |

**Tổng:** 28 Views
- 🔵 **SV1:** 14 Views (2 Layout + 4 Auth + 8 Admin)
- 🔴 **SV2:** 14 Views (1 Layout + 2 Shared + 3 User + 6 Admin + 2 Root)

---

## PHẦN IX: BẢNG TỔNG PHÂN CÔNG SINH VIÊN 1 & SINH VIÊN 2

### 🔵 Sinh viên 1 (51 items)

| Hạng mục | Danh sách |
|----------|-----------|
| **CSDL (8 bảng)** | Roles, Users, Materials, MaterialTransactions, Products, ProductMaterials, MaterialRequests, Notifications |
| **API (24 APIs)** | Auth 7 (Login, Register, RegisterAdmin, Logout) + User Mgmt 6 (GetUsers, ToggleActive, DeleteUser, GetAdminAccounts, ApproveAdmin, RejectAdmin) + Products 5 (GetProducts, AddProduct, UpdateProduct, DeleteProduct, GetCostEstimation) + Materials 4 (GetMaterials, AddMaterial, UpdateMaterial, DeleteMaterial) + Reports 2 (GetFinishedProducts, GetProfitReport) |
| **Views (14)** | _Layout, _AdminLayout, Login, AdminLogin, Register, RegisterAdmin, Admin/Index, Admin/SampleOrders, Admin/Products, Admin/Materials, Admin/Assignments, Admin/Submissions, Admin/OrderProgress |
| **Thuật toán (5)** | Dashboard Admin, CheckOverdue, ApproveMaterialRequest, ImportMaterial, GetCostEstimation |

### 🔴 Sinh viên 2 (53 items)

| Hạng mục | Danh sách |
|----------|-----------|
| **CSDL (7 bảng)** | SampleOrders, Assignments, AssignmentMaterials, Submissions, Payments, Penalties, Profits |
| **API (26 APIs)** | Inventory 3 (GetInventoryReport, ApproveMaterialRequest, RejectMaterialRequest) + SampleOrders 2 (GetSampleOrders, CreateSampleOrder) + Assignments 3 (GetAssignments, CreateAssignment, GetAssignmentDetails) + Submissions 2 (GetSubmissions, ApproveSubmission) + Payments 1 (GetPayments) + Penalties 4 (GetPenalties, CreatePenalty, DeductPenalty, RejectSubmission) + Admin Utilities 4 (Index, CheckOverdue, ResetDatabase, GetMaterialRequests) + User APIs 4 (GetMyAssignments, SubmitWork, GetMyProgress, GetMyPenalties) + Utilities 3 (ImportMaterial, GetMaterialTransactions, Inventory) |
| **Views (14)** | _UserLayout, Error, _ValidationScriptsPartial, _ViewImports, _ViewStart, User/Index, User/Recipes, User/MyProgress, Admin/Inventory, Admin/FinishedProducts, Admin/CostEstimation, Admin/Payments, Admin/Penalties, Admin/Users, Admin/AdminAccounts |
| **Thuật toán (6)** | CreateAssignment, ApproveSubmission, DeductPenalty, CalculateProfit, CreateSampleOrder, RejectSubmission |


