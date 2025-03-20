using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Models;

namespace SIMS_ASM.Data
{
    public class ApplicationDbContex : DbContext
    {
        public ApplicationDbContex(DbContextOptions<ApplicationDbContex> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<RequestSupport> RequestSupports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Đảm bảo Username là duy nhất
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Ràng buộc Role chỉ nhận giá trị Student, Lecturer, Administrator
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasMaxLength(20)
                .HasConversion<string>();
            modelBuilder.Entity<User>()
                .ToTable(t => t.HasCheckConstraint("CK_User_Role", "Role IN ('Student', 'Lecturer', 'Administrator')"));

            // Ràng buộc RequestStatus chỉ nhận giá trị In Progress, Completed
            modelBuilder.Entity<RequestSupport>()
                .Property(r => r.RequestStatus)
                .HasMaxLength(20)
                .HasConversion<string>();
            modelBuilder.Entity<RequestSupport>()
                .ToTable(t => t.HasCheckConstraint("CK_RequestSupport_Status", "RequestStatus IN ('In Progress', 'Completed')"));

            // Cấu hình mối quan hệ cho Course và Grade
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Grade) // Mối quan hệ 1-1 với Grade
                .WithMany() // Grade không cần điều hướng ngược (nếu không có)
                .HasForeignKey(c => c.GradeID)
                .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa cascade nếu cần

            modelBuilder.Entity<Course>()
                .HasOne(c => c.User) // Mối quan hệ với User
                .WithMany(u => u.Courses) // User có nhiều Courses
                .HasForeignKey(c => c.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Grade>()
                .HasOne(g => g.Course) // Mối quan hệ với Course
                .WithMany(c => c.Grades) // Course có nhiều Grades
                .HasForeignKey(g => g.CourseID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Grade>()
                .HasOne(g => g.User) // Mối quan hệ với User
                .WithMany(u => u.Grades) // User có nhiều Grades
                .HasForeignKey(g => g.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RequestSupport>()
                .HasOne(r => r.User) // Mối quan hệ với User
                .WithMany(u => u.RequestSupports) // User có nhiều RequestSupports
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
