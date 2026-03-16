using Microsoft.EntityFrameworkCore;
using CraftOutsourcing.Models;

namespace CraftOutsourcing.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Material> Materials { get; set; } = null!;
        public DbSet<MaterialTransaction> MaterialTransactions { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductMaterial> ProductMaterials { get; set; } = null!;
        public DbSet<Assignment> Assignments { get; set; } = null!;
        public DbSet<AssignmentMaterial> AssignmentMaterials { get; set; } = null!;
        public DbSet<Submission> Submissions { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<SampleOrder> SampleOrders { get; set; } = null!;
        public DbSet<Penalty> Penalties { get; set; } = null!;
        public DbSet<MaterialRequest> MaterialRequests { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Profit> Profits { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Moi quan he One-to-One: Submission va Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Submission)
                .WithOne(s => s.Payment)
                .HasForeignKey<Payment>(p => p.SubmissionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Assignments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Product)
                .WithMany(p => p.Assignments)
                .HasForeignKey(a => a.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // SampleOrder -> Product
            modelBuilder.Entity<SampleOrder>()
                .HasOne(so => so.Product)
                .WithMany(p => p.SampleOrders)
                .HasForeignKey(so => so.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Assignment -> SampleOrder (optional)
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.SampleOrder)
                .WithMany(so => so.Assignments)
                .HasForeignKey(a => a.SampleOrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // Penalty
            modelBuilder.Entity<Penalty>()
                .HasOne(p => p.Assignment)
                .WithMany(a => a.Penalties)
                .HasForeignKey(p => p.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Penalty>()
                .HasOne(p => p.User)
                .WithMany(u => u.Penalties)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Penalty -> Submission (optional)
            modelBuilder.Entity<Penalty>()
                .HasOne(p => p.Submission)
                .WithMany()
                .HasForeignKey(p => p.SubmissionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Định mức nguyên liệu cho sản phẩm
            modelBuilder.Entity<ProductMaterial>()
                .HasOne(pm => pm.Product)
                .WithMany(p => p.ProductMaterials)
                .HasForeignKey(pm => pm.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductMaterial>()
                .HasOne(pm => pm.Material)
                .WithMany(m => m.ProductMaterials)
                .HasForeignKey(pm => pm.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấp nguyên liệu khi giao việc
            modelBuilder.Entity<AssignmentMaterial>()
                .HasOne(am => am.Assignment)
                .WithMany(a => a.AssignmentMaterials)
                .HasForeignKey(am => am.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssignmentMaterial>()
                .HasOne(am => am.Material)
                .WithMany(m => m.AssignmentMaterials)
                .HasForeignKey(am => am.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed dữ liệu Role và Admin khởi tạo
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" }
            );

            // Seed User Admin: Password la 'admin123'
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "$2a$11$G7yT92Z1.J9x7i3Qy/tYm.aZ92mI/K6X9e3N.C2l19x2H.K1x2F1O", // admin123
                    FullName = "Quan tri vien",
                    Phone = "0123456789",
                    RoleId = 1,
                    IsApproved = true,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1)
                }
            );
        }
    }
}
