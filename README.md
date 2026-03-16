## Xây dựng Backend Hệ thống Quản lý Gia công Thủ công Mỹ nghệ (CraftOutsourcing)

---

## PHẦN I: MỞ ĐẦU

### 1. Tên đề tài
**"Xây dựng Backend Hệ thống Quản lý Gia công Thủ công Mỹ nghệ - CraftOutsourcing"**

### 2. Tính cấp thiết của đề tài

Tại Việt Nam, các làng nghề thủ công mỹ nghệ thường gia công sản phẩm theo hình thức phân phối nguyên liệu đến các hộ dân, sau đó thu hồi thành phẩm. Quy trình này gặp nhiều khó khăn:

- **Vấn đề quản lý nguyên liệu**: Nhập/xuất kho thủ công, khó kiểm soát tồn kho và thất thoát nguyên liệu
- **Vấn đề giao việc & tiến độ**: Không có hệ thống tập trung theo dõi ai đang làm gì, bao nhiêu, hạn chót khi nào
- **Vấn đề kiểm soát chất lượng (KCS)**: Quy trình duyệt sản phẩm đạt/lỗi chưa minh bạch, dữ liệu không lưu trữ
- **Vấn đề tài chính**: Tính tiền công, tiền phạt thủ công dễ nhầm lẫn và thiếu minh bạch
- **Vấn đề đơn hàng**: Khó theo dõi tiến độ hoàn thành đơn hàng mẫu từ khách hàng

**Giải pháp**: Xây dựng hệ thống Backend MVC tập trung để tự động hóa toàn bộ quy trình gia công, từ quản lý nguyên liệu, giao/nhận việc, kiểm soát chất lượng đến thanh toán và báo cáo lợi nhuận.

### 3. Mục tiêu đề tài

Mục tiêu chính của bài tập lớn:

1. **Thiết kế cơ sở dữ liệu** cho hệ thống quản lý gia công với 15 bảng dữ liệu
2. **Xây dựng Backend MVC** hỗ trợ các chức năng quản lý từ cơ bản đến nâng cao
3. **Triển khai logic nghiệp vụ** tự động: giao việc - trừ kho - nộp hàng - duyệt KCS - trả công - phạt lỗi
4. **Phát triển hệ thống thông báo** real-time cho hộ dân
5. **Đảm bảo tính bảo mật** xác thực Cookie, phân quyền Admin/User

### 4. Phạm vi công việc

- **Phạm vi dữ liệu**: Hệ thống quản lý nhiều hộ gia công, nhiều sản phẩm, nhiều đơn hàng mẫu
- **Phạm vi chức năng**: 2 nhóm người dùng (Admin quản trị, User hộ gia công)
- **Phạm vi công nghệ**: ASP.NET Core MVC (.NET 10), Entity Framework Core, SQL Server, Cookie Authentication, BCrypt
- **Phạm vi thời gian**: 1 kỳ học

---

## PHẦN II: PHÂN TÍCH YÊU CẦU HỆ THỐNG

### 1. Phân tích đối tượng sử dụng

| **Nhóm người dùng** | **Vai trò** | **Nhu cầu chính** |
|---|---|---|
| **Admin (Quản trị)** | Quản lý toàn bộ hệ thống | Quản lý nguyên liệu, sản phẩm, giao việc, duyệt KCS, thanh toán, phạt, báo cáo lợi nhuận |
| **User (Hộ gia công)** | Người thợ nhận gia công | Xem công việc, nộp sản phẩm, xem công thức BOM, theo dõi tiến độ, thanh toán phạt |

**Kết luận**: Hệ thống tập trung vào **Backend quản lý nghiệp vụ gia công**, hỗ trợ 2 loại người dùng với quyền hạn khác nhau.

### 2. Phân tích yêu cầu chức năng chính

#### **Nhóm chức năng A: Quản lý dữ liệu nền**
- Quản lý thông tin hộ gia công (CRUD + Active/Inactive)
- Quản lý tài khoản Admin (đăng ký cần phê duyệt)
- Quản lý nguyên liệu/vật tư (CRUD + Nhập kho + Cảnh báo tồn kho thấp)
- Quản lý sản phẩm + Định mức nguyên liệu BOM (Bill of Materials)

#### **Nhóm chức năng B: Quy trình gia công**
- Tạo đơn hàng mẫu (SampleOrder) từ khách hàng
- Giao việc cho hộ dân (auto trừ kho nguyên liệu, auto fill số lượng)
- Hộ dân nộp sản phẩm (Submission) theo nhiều đợt
- Admin duyệt KCS (Kiểm tra chất lượng): phân loại Đạt/Lỗi
- Tự động tạo Payment (trả công) và Penalty (phạt lỗi)

#### **Nhóm chức năng C: Tài chính & Báo cáo**
- Quản lý thanh toán tiền công cho hộ gia công
- Quản lý tiền phạt (tạo, miễn, trừ, chuyển lỗi → đạt khi thanh toán phạt)
- Dự tính giá thành sản phẩm (Cost Estimation)
- Báo cáo lợi nhuận theo đơn hàng mẫu
- Kiểm tra đơn quá hạn tự động tạo phạt

#### **Nhóm chức năng D: Hệ thống hỗ trợ**
- Hệ thống thông báo (Notification) cho hộ dân
- Yêu cầu cấp thêm nguyên liệu (Material Request)
- Quản lý kho thành phẩm (Finished Products)
- Reset Database (tiện ích Admin)

---

## PHẦN III: THIẾT KẾ CƠ SỞ DỮ LIỆU

### 1. Sơ đồ Lôgic ER (Entity-Relationship)

```
┌──────────────┐         ┌──────────────────┐
│    Role      │────────→│      User        │
│              │   1:N   │                  │
│ PK: Id       │         │ PK: Id           │
└──────────────┘         │ FK: RoleId       │
                         └──────────────────┘
                                │
          ┌─────────────────────┼─────────────────────┐
          │                     │                     │
          ↓                     ↓                     ↓
┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│   Assignment     │  │    Payment       │  │    Penalty       │
│                  │  │                  │  │                  │
│ PK: Id           │  │ PK: Id           │  │ PK: Id           │
│ FK: UserId       │  │ FK: UserId       │  │ FK: UserId       │
│ FK: ProductId    │  │ FK: SubmissionId  │  │ FK: AssignmentId │
│ FK: SampleOrderId│  └──────────────────┘  │ FK: SubmissionId │
└──────────────────┘                        └──────────────────┘
          │
          ├──→ ┌──────────────────────┐
          │    │  AssignmentMaterial   │
          │    │ FK: AssignmentId      │
          │    │ FK: MaterialId        │
          │    └──────────────────────┘
          │
          └──→ ┌──────────────────┐        ┌──────────────────┐
               │   Submission     │───────→│    Payment       │
               │                  │  1:1   │                  │
               │ PK: Id           │        │ FK: SubmissionId  │
               │ FK: AssignmentId │        └──────────────────┘
               └──────────────────┘

┌──────────────┐         ┌──────────────────┐
│   Product    │────────→│  ProductMaterial  │
│              │   1:N   │                  │
│ PK: Id       │         │ FK: ProductId    │
└──────────────┘         │ FK: MaterialId   │
       │                 └──────────────────┘
       │
       ├──→ ┌──────────────────┐
       │    │   SampleOrder    │
       │    │ FK: ProductId    │
       │    └──────────────────┘
       │
       └──→ ┌──────────────────┐
            │   Assignment     │
            │ FK: ProductId    │
            └──────────────────┘

┌──────────────────┐         ┌────────────────────────┐
│    Material      │────────→│  MaterialTransaction   │
│                  │   1:N   │                        │
│ PK: Id           │         │ FK: MaterialId          │
└──────────────────┘         └────────────────────────┘

┌──────────────────┐
│  MaterialRequest │
│ FK: AssignmentId │
│ FK: MaterialId   │
└──────────────────┘

┌──────────────────┐         ┌──────────────────┐
│  Notification    │         │     Profit       │
│ FK: UserId       │         │ FK: SampleOrderId│
└──────────────────┘         └──────────────────┘
```

### 2. Chi tiết các bảng dữ liệu

#### **Bảng 1: Role (Vai trò)**
```sql
CREATE TABLE Roles (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(50) NOT NULL
);
```

**Mô tả**:
- Lưu trữ vai trò người dùng (Admin, User)
- `Id`: Mã vai trò (khóa chính, tự tăng)
- Seed data: Admin (Id=1), User (Id=2)

---

#### **Bảng 2: User (Người dùng / Hộ gia công)**
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

**Mô tả**:
- Lưu trữ thông tin người dùng (Admin + Hộ gia công)
- `PasswordHash`: Mật khẩu mã hóa BCrypt
- `IsApproved`: Admin mới cần được phê duyệt bởi Admin hiện tại
- `IsActive`: Trạng thái hoạt động (có thể vô hiệu hóa)
- `Balance`: Số dư tài khoản (dùng cho trừ phạt)

---

#### **Bảng 3: Material (Nguyên liệu / Vật tư)**
```sql
CREATE TABLE Materials (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL,
    Unit NVARCHAR(20) NOT NULL,       -- kg, m, cái, cuộn...
    StockQuantity FLOAT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,  -- Giá 1 đơn vị nguyên liệu
    MinStock FLOAT NOT NULL            -- Mức tồn kho tối thiểu để cảnh báo
);
```

**Mô tả**:
- Lưu trữ thông tin nguyên liệu/vật tư
- `StockQuantity`: Số lượng tồn kho hiện tại
- `MinStock`: Mức tồn kho tối thiểu, dưới mức này sẽ cảnh báo
- Hỗ trợ 16 đơn vị tính: kg, g, l, ml, m, cm, mm, cái, cuộn, bộ, hộp, túi, chiếc, sợi, đoạn, lớp

---

#### **Bảng 4: MaterialTransaction (Lịch sử giao dịch nguyên liệu)**
```sql
CREATE TABLE MaterialTransactions (
    Id INT PRIMARY KEY IDENTITY,
    MaterialId INT NOT NULL FOREIGN KEY REFERENCES Materials(Id),
    TransactionType NVARCHAR(20) NOT NULL,  -- Import (nhập), Export (xuất)
    Quantity FLOAT NOT NULL,
    TransactionDate DATETIME2 NOT NULL,
    ReferenceId INT                          -- ID Assignment nếu xuất kho cho giao việc
);
```

**Mô tả**:
- Ghi nhận lịch sử nhập/xuất kho nguyên liệu
- `ReferenceId`: Liên kết với Assignment ID khi xuất kho cho giao việc

---

#### **Bảng 5: Product (Sản phẩm)**
```sql
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    UnitPrice DECIMAL(18,2) NOT NULL,  -- Đơn giá trả công cho thợ / 1 sản phẩm
    FinishedStock INT NOT NULL DEFAULT 0  -- Tồn kho thành phẩm
);
```

**Mô tả**:
- Lưu trữ thông tin sản phẩm thủ công mỹ nghệ
- `UnitPrice`: Tiền công trả cho hộ dân trên 1 sản phẩm hoàn thành
- `FinishedStock`: Tồn kho thành phẩm (tự động cộng khi duyệt KCS)

---

#### **Bảng 6: ProductMaterial (Định mức nguyên liệu - BOM)**
```sql
CREATE TABLE ProductMaterials (
    Id INT PRIMARY KEY IDENTITY,
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(Id) ON DELETE CASCADE,
    MaterialId INT NOT NULL FOREIGN KEY REFERENCES Materials(Id) ON DELETE NO ACTION,
    QuantityRequired FLOAT NOT NULL  -- Số lượng nguyên liệu cần cho 1 sản phẩm
);
```

**Mô tả**:
- Bảng trung gian lưu định mức nguyên liệu cho mỗi sản phẩm (Bill of Materials)
- `QuantityRequired`: Số lượng nguyên liệu cần cho **1 sản phẩm**
- Dùng để: tự động trừ kho khi giao việc, tính giá thành sản phẩm

---

#### **Bảng 7: SampleOrder (Đơn hàng mẫu)**
```sql
CREATE TABLE SampleOrders (
    Id INT PRIMARY KEY IDENTITY,
    OrderCode NVARCHAR(150) NOT NULL,     -- Mã đơn: SO0001, SO0002...
    CustomerName NVARCHAR(200) NOT NULL,  -- Khách hàng đặt
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(Id) ON DELETE NO ACTION,
    TotalQuantity INT NOT NULL,           -- Tổng số lượng cần làm
    CompletedQuantity INT NOT NULL DEFAULT 0,
    Description NVARCHAR(500),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Draft',  -- Draft, Confirmed, InProduction, Completed, Cancelled
    CreatedDate DATETIME2 NOT NULL,
    TargetDate DATETIME2,
    EstimatedCost DECIMAL(18,2) NOT NULL,  -- Ước tính giá thành
    ActualCost DECIMAL(18,2) NOT NULL DEFAULT 0,
    SellingPrice DECIMAL(18,2) NOT NULL    -- Giá bán cho khách hàng / SP
);
```

**Mô tả**:
- Lưu trữ đơn hàng mẫu từ khách hàng
- `OrderCode`: Tự động sinh (SO0001, SO0002...) hoặc nhập tay
- `EstimatedCost`: Tự động tính = (Chi phí NL + Tiền công) × Số lượng
- `Status`: Theo dõi vòng đời đơn hàng từ Draft → InProduction → Completed

---

#### **Bảng 8: Assignment (Giao việc)**
```sql
CREATE TABLE Assignments (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE NO ACTION,
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(Id) ON DELETE NO ACTION,
    QuantityAssigned INT NOT NULL,
    CompletedQuantity INT NOT NULL DEFAULT 0,
    AssignedDate DATETIME2 NOT NULL,
    DueDate DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',  -- Pending, InProgress, PendingVerification, Completed, Overdue, Cancelled
    SampleOrderId INT FOREIGN KEY REFERENCES SampleOrders(Id) ON DELETE SET NULL
);
```

**Mô tả**:
- Ghi nhận việc giao công cho hộ dân
- Khi tạo Assignment: tự động trừ kho nguyên liệu theo BOM
- `CompletedQuantity`: Cập nhật khi Admin duyệt KCS
- Liên kết với SampleOrder (optional) để theo dõi tiến độ đơn hàng

---

#### **Bảng 9: AssignmentMaterial (Nguyên liệu cấp cho giao việc)**
```sql
CREATE TABLE AssignmentMaterials (
    Id INT PRIMARY KEY IDENTITY,
    AssignmentId INT NOT NULL FOREIGN KEY REFERENCES Assignments(Id) ON DELETE CASCADE,
    MaterialId INT NOT NULL FOREIGN KEY REFERENCES Materials(Id) ON DELETE NO ACTION,
    QuantityGiven FLOAT NOT NULL  -- Số lượng nguyên liệu thực tế cấp cho thợ
);
```

**Mô tả**:
- Lưu chi tiết nguyên liệu đã cấp cho mỗi lần giao việc
- Tự động tạo khi Admin giao việc

---

#### **Bảng 10: Submission (Nộp thành phẩm)**
```sql
CREATE TABLE Submissions (
    Id INT PRIMARY KEY IDENTITY,
    AssignmentId INT NOT NULL FOREIGN KEY REFERENCES Assignments(Id) ON DELETE CASCADE,
    SubmittedDate DATETIME2 NOT NULL,
    SubmissionNumber INT NOT NULL,     -- Lần thứ mấy nộp (1, 2, 3...)
    QuantitySubmitted INT NOT NULL,    -- Số lượng nộp lần này
    QuantityGood INT NOT NULL DEFAULT 0,     -- Số lượng đạt KCS
    QuantityDefect INT NOT NULL DEFAULT 0,   -- Số lượng lỗi
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',  -- Pending, Approved, Rejected
    ReviewNote NVARCHAR(200)           -- Ghi chú khi duyệt
);
```

**Mô tả**:
- Ghi nhận mỗi lần hộ dân nộp sản phẩm
- Hỗ trợ nộp nhiều đợt (SubmissionNumber = 1, 2, 3...)
- Admin duyệt KCS: phân loại `QuantityGood` và `QuantityDefect`
- Quan hệ One-to-One với Payment

---

#### **Bảng 11: Payment (Thanh toán tiền công)**
```sql
CREATE TABLE Payments (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE NO ACTION,
    SubmissionId INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Submissions(Id) ON DELETE NO ACTION,
    Amount DECIMAL(18,2) NOT NULL,     -- Tổng tiền công = QuantityGood × UnitPrice
    PaymentDate DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Paid'  -- Unpaid, Paid
);
```

**Mô tả**:
- Ghi nhận thanh toán tiền công cho hộ gia công
- Tự động tạo khi Admin duyệt KCS
- **Công thức**: Amount = Số lượng đạt × Đơn giá sản phẩm

---

#### **Bảng 12: Penalty (Tiền phạt)**
```sql
CREATE TABLE Penalties (
    Id INT PRIMARY KEY IDENTITY,
    AssignmentId INT NOT NULL FOREIGN KEY REFERENCES Assignments(Id) ON DELETE NO ACTION,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE NO ACTION,
    Reason NVARCHAR(20) NOT NULL,        -- Overdue, QualityFail
    Amount DECIMAL(18,2) NOT NULL,
    Note NVARCHAR(500),
    CreatedDate DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active',  -- Active, Waived, Deducted, Paid, Resolved
    PaidDate DATETIME2,
    DefectiveQuantity INT NOT NULL DEFAULT 0,
    SubmissionId INT FOREIGN KEY REFERENCES Submissions(Id) ON DELETE SET NULL
);
```

**Mô tả**:
- Quản lý tiền phạt cho hộ gia công
- `Reason`: Overdue (quá hạn) hoặc QualityFail (lỗi chất lượng)
- **Phạt lỗi**: Tự động tạo khi duyệt KCS có sản phẩm lỗi, Amount = Số lỗi × Chi phí NL/SP
- **Phạt quá hạn**: Tự động tạo khi kiểm tra đơn quá hạn
- Khi thanh toán phạt: tự động chuyển sản phẩm lỗi → đạt (tha bổng)

---

#### **Bảng 13: MaterialRequest (Yêu cầu cấp thêm nguyên liệu)**
```sql
CREATE TABLE MaterialRequests (
    Id INT PRIMARY KEY IDENTITY,
    AssignmentId INT NOT NULL FOREIGN KEY REFERENCES Assignments(Id) ON DELETE CASCADE,
    MaterialId INT NOT NULL FOREIGN KEY REFERENCES Materials(Id) ON DELETE CASCADE,
    QuantityRequested INT NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',  -- Pending, Approved, Completed
    CreatedDate DATETIME2 NOT NULL,
    ApprovedDate DATETIME2,
    Reason NVARCHAR(200)
);
```

**Mô tả**:
- Hộ dân yêu cầu cấp thêm nguyên liệu khi thiếu
- Admin duyệt → tự động trừ kho

---

#### **Bảng 14: Notification (Thông báo)**
```sql
CREATE TABLE Notifications (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    Title NVARCHAR(MAX) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Type NVARCHAR(MAX) NOT NULL DEFAULT 'info',  -- info, success, warning, error
    RelatedLink NVARCHAR(MAX),
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL
);
```

**Mô tả**:
- Hệ thống thông báo cho hộ gia công
- Tự động gửi khi: giao việc mới, duyệt KCS, phạt, hoàn thành đơn

---

#### **Bảng 15: Profit (Lợi nhuận)**
```sql
CREATE TABLE Profits (
    Id INT PRIMARY KEY IDENTITY,
    SampleOrderId INT NOT NULL FOREIGN KEY REFERENCES SampleOrders(Id) ON DELETE CASCADE,
    QuantityGood INT NOT NULL,
    QuantityDefect INT NOT NULL,
    SellingPrice DECIMAL(18,2) NOT NULL,      -- Giá bán / SP
    CostPrice DECIMAL(18,2) NOT NULL,         -- Giá vốn / SP
    SalesProfit DECIMAL(18,2) NOT NULL,       -- = (Good × SellingPrice) - (Good × CostPrice)
    PenaltyRevenue DECIMAL(18,2) NOT NULL,    -- Tiền phạt thu được
    TotalProfit DECIMAL(18,2) NOT NULL,       -- = SalesProfit + PenaltyRevenue
    RecordDate DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active'  -- Active, Cancelled
);
```

**Mô tả**:
- Ghi nhận lợi nhuận theo đơn hàng mẫu
- **Công thức lợi nhuận**:
  - SalesProfit = (Số lượng đạt × Giá bán) - (Số lượng đạt × Giá vốn)
  - TotalProfit = SalesProfit + PenaltyRevenue

---

### 3. Tóm tắt thiết kế dữ liệu

| **Bảng** | **Nhóm** | **Chức năng** |
|---|---|---|
| **Role** | Identity | Quản lý vai trò (Admin, User) |
| **User** | Identity | Quản lý người dùng & hộ gia công |
| **Material** | Inventory | Quản lý nguyên liệu/vật tư |
| **MaterialTransaction** | Inventory | Lịch sử nhập/xuất kho |
| **Product** | Production | Quản lý sản phẩm |
| **ProductMaterial** | Production | Định mức nguyên liệu (BOM) |
| **SampleOrder** | Production | Đơn hàng mẫu từ khách hàng |
| **Assignment** | Production | Giao việc cho hộ dân |
| **AssignmentMaterial** | Production | NL cấp cho mỗi lần giao việc |
| **Submission** | Production | Nộp thành phẩm |
| **MaterialRequest** | Production | Yêu cầu cấp thêm NL |
| **Payment** | Finance | Thanh toán tiền công |
| **Penalty** | Finance | Quản lý tiền phạt |
| **Profit** | Finance | Lợi nhuận theo đơn hàng |
| **Notification** | Support | Hệ thống thông báo |

---

## PHẦN IV: THIẾT KẾ BACKEND

### 1. Kiến trúc Backend (MVC Monolithic)

**Mô tả**: Backend được tổ chức theo mô hình **ASP.NET Core MVC** (Model-View-Controller), sử dụng Razor Views + AJAX cho frontend:

```
┌─────────────────────────────────────┐
│      Browser (Razor Views + AJAX)   │
└─────────────────────────────────────┘
                 │
                 ↓
┌─────────────────────────────────────┐
│   Cookie Authentication Middleware  │ (BCrypt Password Hashing)
└─────────────────────────────────────┘
                 │
        ┌────────┼────────┐
        ↓        ↓        ↓
┌──────────────────────────────────────┐
│          MVC Controllers             │
├──────────────────────────────────────┤
│ ├─ AuthController                   │  (Login, Register, Logout)
│ ├─ AdminController [Authorize]      │  (Toàn bộ quản trị)
│ └─ UserController  [Authorize]      │  (Chức năng hộ gia công)
└──────────────────────────────────────┘
                 │
                 ↓
┌──────────────────────────────────────┐
│     Entity Framework Core (ORM)      │
│     ApplicationDbContext             │
│     (15 DbSet + Fluent API Config)   │
└──────────────────────────────────────┘
                 │
                 ↓
┌─────────────────────────────────────┐
│     SQL Server (Database)           │
│     CraftOutsourcingDB              │
└─────────────────────────────────────┘
```

### 2. Công nghệ sử dụng

| **Thành phần** | **Công nghệ** | **Phiên bản** |
|---|---|---|
| Framework | ASP.NET Core MVC | .NET 10.0 |
| ORM | Entity Framework Core | 10.0.4 |
| Database | SQL Server Express | LocalDB |
| Authentication | Cookie Authentication | Built-in |
| Password Hashing | BCrypt.Net-Next | 4.1.0 |
| Frontend | Razor Views + jQuery AJAX | — |
| Razor Runtime | RuntimeCompilation | 10.0.4 |

### 3. Danh sách API Endpoints

#### **A. Authentication (AuthController)**

| **Method** | **Endpoint** | **Mô tả** | **Quyền** |
|---|---|---|---|
| GET | `/Auth/Login` | Trang đăng nhập | Public |
| POST | `/Auth/Login` | Xử lý đăng nhập | Public |
| GET | `/Auth/Register` | Trang đăng ký hộ gia công | Public |
| POST | `/Auth/Register` | Xử lý đăng ký User | Public |
| GET | `/Auth/RegisterAdmin` | Trang đăng ký Admin | Public |
| POST | `/Auth/RegisterAdmin` | Xử lý đăng ký Admin (cần phê duyệt) | Public |
| GET | `/Auth/Logout` | Đăng xuất | Authenticated |

---

#### **B. Admin - Quản lý hộ gia công (AdminController)**

| **Method** | **Endpoint** | **Mô tả** |
|---|---|---|
| GET | `/Admin/GetUsers` | Lấy danh sách hộ gia công (kèm thống kê thu nhập, phạt) |
| POST | `/Admin/ToggleUserActive` | Bật/tắt trạng thái hoạt động |
| POST | `/Admin/DeleteUser` | Xóa hộ gia công (kiểm tra ràng buộc) |

---

#### **C. Admin - Quản lý tài khoản Admin**

| **Method** | **Endpoint** | **Mô tả** |
|---|---|---|
| GET | `/Admin/GetAdminAccounts` | Lấy danh sách Admin |
| POST | `/Admin/ApproveAdmin` | Phê duyệt Admin mới |
| POST | `/Admin/RejectAdmin` | Từ chối Admin mới |

---

#### **D. Admin - Quản lý nguyên liệu**

| **Method** | **Endpoint** | **Mô tả** |
|---|---|---|
| GET | `/Admin/GetMaterials` | Lấy danh sách nguyên liệu (kèm cảnh báo tồn kho) |
| POST | `/Admin/AddMaterial` | Thêm nguyên liệu mới |
| POST | `/Admin/UpdateMaterial` | Cập nhật nguyên liệu |
| POST | `/Admin/DeleteMaterial` | Xóa nguyên liệu (kiểm tra đang dùng) |
| POST | `/Admin/ImportMaterial` | Nhập kho nguyên liệu (tạo transaction) |

---

#### **E. Admin - Quản lý sản phẩm & BOM**

| **Method** | **Endpoint** | **Mô tả** |
|---|---|---|
| GET | `/Admin/GetProducts` | Lấy DS sản phẩm (kèm BOM, giá thành) |
| POST | `/Admin/AddProduct` | Thêm sản phẩm + định mức NL |
| POST | `/Admin/UpdateProduct` | Cập nhật sản phẩm + BOM |
| POST | `/Admin/DeleteProduct` | Xóa sản phẩm (kiểm tra ràng buộc) |

---

#### **F. Admin - Đơn hàng mẫu**

| **Method** | **Endpoint** | **Mô tả** |
|---|---|---|
| GET | `/Admin/GetSampleOrders` | Lấy DS đơn hàng (kèm tiến độ) |
| POST | `/Admin/CreateSampleOrder` | Tạo đơn hàng mẫu (auto tính giá thành) |
| POST | `/Admin/UpdateSampleOrderStatus` | Cập nhật trạng thái đơn hàng |

---

#### **G. Admin - Giao việc**

| **Method** | **Endpoint** | **Mô tả** |
|---|---|---|
| GET | `/Admin/GetAssignments` | Lấy DS giao việc |
| GET | `/Admin/GetAssignmentDetails` | Chi tiết giao việc (submissions, penalties) |
| POST | `/Admin/CreateAssignment` | **Giao việc** (auto trừ kho, gửi thông báo) |

---

#### **H. Admin - Duyệt KCS**

| **Method** | **Endpoint** | **Mô tả** |
|---|---|---|
| GET | `/Admin/GetSubmissions` | Lấy DS nộp hàng |
| POST | `/Admin/ApproveSubmission` | **Duyệt KCS** (auto trả công, auto phạt lỗi, cộng kho thành phẩm) |
| POST | `/Admin/RejectSubmission` | Từ chối (cho làm lại) |

---

#### **I. Admin - Tài chính**

| **Method** | **Endpoint** | **Mô tả** |
|---|---|---|
| GET | `/Admin/GetPayments` | Lấy DS thanh toán |
| GET | `/Admin/GetPenalties` | Lấy DS tiền phạt |
| POST | `/Admin/CreatePenalty` | Tạo phạt thủ công |
| POST | `/Admin/WaivePenalty` | Miễn phạt |
| POST | `/Admin/DeductPenalty` | **Trừ/Thanh toán phạt** (auto chuyển lỗi→đạt) |
| GET | `/Admin/GetCostEstimation` | Dự tính giá thành & lợi nhuận |

---

#### **J. Admin - Kho & Tiện ích**

| **Method** | **Endpoint** | **Mô tả** |
|---|---|---|
| GET | `/Admin/GetInventoryReport` | Báo cáo tồn kho (NL + Thành phẩm + Lịch sử) |
| GET | `/Admin/GetFinishedProducts` | DS thành phẩm |
| GET | `/Admin/GetMaterialRequests` | DS yêu cầu NL |
| POST | `/Admin/ApproveMaterialRequest` | Duyệt yêu cầu NL (auto trừ kho) |
| POST | `/Admin/RejectMaterialRequest` | Từ chối yêu cầu NL |
| POST | `/Admin/CheckOverdue` | Kiểm tra quá hạn (auto tạo phạt) |
| POST | `/Admin/ResetDatabase` | Reset toàn bộ dữ liệu |

---

#### **K. User - Hộ gia công (UserController)**

| **Method** | **Endpoint** | **Mô tả** |
|---|---|---|
| GET | `/User/GetMyAssignments` | Xem công việc được giao |
| POST | `/User/SubmitWork` | **Nộp sản phẩm** (nhiều đợt) |
| GET | `/User/GetMyRecipes` | Xem công thức BOM sản phẩm |
| GET | `/User/GetMyProgress` | Xem tiến độ cá nhân |
| GET | `/User/GetUnreadNotifications` | Lấy thông báo chưa đọc |
| GET | `/User/GetMyPenalties` | Xem DS phạt cá nhân |
| POST | `/User/PayPenalty` | **Thanh toán phạt** (auto tha bổng lỗi) |

---

## PHẦN V: CHI TIẾT LOGIC NGHIỆP VỤ

### 1. Quy trình giao việc & tự động trừ kho

**Mô tả vấn đề**: Khi Admin giao việc cho hộ dân, cần tự động kiểm tra và trừ nguyên liệu kho.

**Thuật toán**:

```
function CreateAssignment(userId, productId, quantity, dueDate, sampleOrderId):

    // Bước 1: Auto-fill số lượng từ đơn hàng nếu chưa nhập
    if quantity <= 0 AND sampleOrderId exists:
        assignedQty = SUM(Assignment.QuantityAssigned WHERE SampleOrderId)
        quantity = SampleOrder.TotalQuantity - assignedQty

    // Bước 2: Lấy BOM (định mức nguyên liệu)
    requiredMaterials = ProductMaterials WHERE ProductId

    // Bước 3: Kiểm tra tồn kho
    for each material in requiredMaterials:
        totalNeeded = material.QuantityRequired × quantity
        if material.StockQuantity < totalNeeded:
            return Error("Không đủ nguyên liệu")

    // Bước 4: Tạo Assignment
    assignment = new Assignment(userId, productId, quantity, dueDate, "InProgress")

    // Bước 5: Trừ kho + Lưu lịch sử
    for each material in requiredMaterials:
        totalNeeded = material.QuantityRequired × quantity
        material.StockQuantity -= totalNeeded

        // Lưu chi tiết NL đã cấp
        create AssignmentMaterial(assignmentId, materialId, totalNeeded)

        // Lưu lịch sử giao dịch
        create MaterialTransaction("Export", totalNeeded, assignmentId)

    // Bước 6: Cập nhật SampleOrder status → InProduction
    // Bước 7: Gửi Notification cho hộ dân

    return Success
```

---

### 2. Quy trình duyệt KCS & tự động trả công/phạt

**Mô tả**: Khi Admin duyệt Submission, hệ thống tự động xử lý nhiều tác vụ.

**Thuật toán**:

```
function ApproveSubmission(submissionId, quantityGood, quantityDefect, reviewNote):

    // Bước 1: Validate
    if good + defect != submission.QuantitySubmitted:
        return Error("Tổng không khớp")

    // Bước 2: Cập nhật Submission
    submission.QuantityGood = good
    submission.QuantityDefect = defect
    submission.Status = "Approved"

    // Bước 3: Cập nhật Assignment
    assignment.CompletedQuantity += good
    if assignment.CompletedQuantity >= assignment.QuantityAssigned:
        assignment.Status = "Completed"

    // Bước 4: TỰ ĐỘNG TRẢ CÔNG
    amount = good × product.UnitPrice
    create Payment(userId, submissionId, amount, "Paid")

    // Bước 5: TỰ ĐỘNG TẠO PHẠT (nếu có lỗi)
    if defect > 0:
        materialCostPerUnit = SUM(BOM.QuantityRequired × Material.UnitPrice)
        defectCost = defect × materialCostPerUnit
        create Penalty(assignmentId, userId, "QualityFail", defectCost)

    // Bước 6: Cộng vào kho thành phẩm
    product.FinishedStock += good

    // Bước 7: Cập nhật SampleOrder nếu hoàn thành
    // Bước 8: Gửi Notification

    return Success
```

---

### 3. Logic thanh toán phạt & tha bổng lỗi

**Mô tả**: Khi hộ dân thanh toán phạt lỗi, sản phẩm lỗi được tự động chuyển thành đạt.

```
function PayPenalty(penaltyId):

    penalty.Status = "Deducted"
    penalty.PaidDate = Now

    // Tính tổng lỗi từ tất cả Submission được duyệt
    totalDefectQuantity = SUM(Submissions.QuantityDefect WHERE Status == "Approved")

    if totalDefectQuantity > 0:
        // Tạo Submission "tha bổng" mới
        create Submission(
            QuantitySubmitted = totalDefectQuantity,
            QuantityGood = totalDefectQuantity,  // Tất cả lỗi → đạt
            Status = "Approved",
            ReviewNote = "Tự động tha bổng sau khi đóng phạt"
        )

        // Cập nhật CompletedQuantity
        assignment.CompletedQuantity += totalDefectQuantity

        // Trả thêm tiền công cho phần lỗi được tha
        bonusAmount = totalDefectQuantity × product.UnitPrice
        create Payment(userId, bonusSubmissionId, bonusAmount, "Paid")

    // Kiểm tra nếu hết penalty → assignment Completed
    // Cập nhật SampleOrder nếu cần

    return Success
```

---

### 4. Logic tính giá thành & lợi nhuận

**Công thức tính giá thành 1 sản phẩm**:
```
Giá thành / SP = Tiền công / SP + Chi phí nguyên liệu / SP
               = Product.UnitPrice + SUM(BOM.QuantityRequired × Material.UnitPrice)
```

**Công thức tính lợi nhuận đơn hàng**:
```
Ước tính lợi nhuận = (Giá bán - Giá thành/SP) × Tổng số lượng
Lợi nhuận thực tế  = (Giá bán - Giá thành/SP) × Số lượng hoàn thành tốt
```

---

## PHẦN VI: CẤU HÌNH & TRIỂN KHAI

### 1. Cấu hình Database

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-1JQFEUE\\SQLEXPRESS;Database=CraftOutsourcingDB;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False"
  }
}
```

### 2. Cấu hình Authentication

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });
```

### 3. Seed Data

Hệ thống tự động tạo dữ liệu ban đầu:
- **Role**: Admin (Id=1), User (Id=2)
- **User Admin**: username = "admin", password = "admin123" (BCrypt hash)

### 4. Các Package NuGet sử dụng

| **Package** | **Version** | **Mục đích** |
|---|---|---|
| BCrypt.Net-Next | 4.1.0 | Mã hóa mật khẩu |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.4 | ORM + SQL Server Provider |
| Microsoft.EntityFrameworkCore.Design | 10.0.4 | EF Migration Tools |
| Microsoft.EntityFrameworkCore.Tools | 10.0.4 | CLI Tools cho Migration |
| Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation | 10.0.4 | Hot-reload Razor Views |

---

## PHẦN VII: TÓM TẮT KẾT QUẢ

### 1. Thống kê quy mô

| **Yếu tố** | **Chi tiết** |
|---|---|
| Tổng số bảng dữ liệu | 15 bảng |
| Tổng số API endpoint | ~45 endpoints |
| Tổng số Controller | 3 (Auth, Admin, User) |
| Tổng số Model | 15 entity + 1 static class (UnitType) + 3 DTO |
| Ngôn ngữ lập trình | C# (.NET 10.0) |
| Cơ sở dữ liệu | SQL Server Express |
| Pattern | MVC + Repository (DbContext) |

### 2. Các tính năng nổi bật

1. **Tự động trừ kho** khi giao việc (dựa trên BOM)
2. **Tự động trả tiền công** khi duyệt KCS
3. **Tự động tạo phạt** khi có sản phẩm lỗi (dựa trên chi phí NL)
4. **Tự động tha bổng lỗi** khi hộ dân thanh toán phạt
5. **Tự động kiểm tra quá hạn** và tạo phạt Overdue
6. **Hệ thống thông báo** real-time cho hộ gia công
7. **Dự tính giá thành** và **lợi nhuận** theo đơn hàng
8. **Auto-generate mã đơn hàng** (SO0001, SO0002...)
9. **Cảnh báo tồn kho thấp** khi nguyên liệu dưới mức MinStock
10. **Phê duyệt Admin mới** bởi Admin hiện tại
