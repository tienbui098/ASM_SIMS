using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Models;

namespace SIMS_ASM.Data
{
    public class ApplicationDbContex : DbContext
    {
        public ApplicationDbContex(DbContextOptions<ApplicationDbContex> options) : base(options)
        {
        }

        // DbSets cho từng bảng
        public DbSet<Major> Majors { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ClassCourseFaculty> ClassCourseFaculties { get; set; }
        public DbSet<StudentClass> StudentClasses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Grade> Grades { get; set; }

        // Cấu hình quan hệ và khóa ngoại (nếu cần)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Major
            modelBuilder.Entity<Major>(entity =>
            {
                entity.HasKey(m => m.MajorID); // Khóa chính
                entity.Property(m => m.MajorName)
                      .HasMaxLength(100) // VARCHAR(100)
                      .IsRequired(); // NOT NULL
                //entity.Property(m => m.CourseStartDate)
                //      .IsRequired(); // NOT NULL
                //entity.Property(m => m.CourseEndDate)
                //      .IsRequired(); // NOT NULL

                // Quan hệ 1-n với Course
                entity.HasMany(m => m.Courses)
                      .WithOne(c => c.Major)
                      .HasForeignKey(c => c.MajorID)
                      .OnDelete(DeleteBehavior.Cascade); // Xóa Major thì xóa luôn Courses liên quan

                // Quan hệ 1-n với Class
                entity.HasMany(m => m.Classes)
                      .WithOne(c => c.Major)
                      .HasForeignKey(c => c.MajorID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 2. Course
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(c => c.CourseID); // Khóa chính
                entity.Property(c => c.CourseName)
                      .HasMaxLength(100) // VARCHAR(100)
                      .IsRequired(); // NOT NULL
                //entity.Property(c => c.CourseStartDate)
                //      .IsRequired(); // NOT NULL
                //entity.Property(c => c.CourseEndDate)
                //      .IsRequired(); // NOT NULL
                entity.Property(c => c.MajorID)
                      .IsRequired(); // NOT NULL cho khóa ngoại
            });

            // 3. Class
            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasKey(c => c.ClassID); // Khóa chính
                entity.Property(c => c.ClassName)
                      .HasMaxLength(100) // VARCHAR(100)
                      .IsRequired(); // NOT NULL
                entity.Property(c => c.MajorID)
                      .IsRequired(); // NOT NULL cho khóa ngoại
            });

            // 4. User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserID); // Khóa chính

                // Đảm bảo Username là duy nhất
                entity.HasIndex(u => u.Username)
                      .IsUnique(); // Tạo chỉ mục duy nhất cho Username

                entity.Property(u => u.Username)
                      .HasMaxLength(50) // VARCHAR(50)
                      .IsRequired(); // NOT NULL

                entity.Property(u => u.Password)
                      .HasMaxLength(50) // VARCHAR(50)
                      .IsRequired(); // NOT NULL

                // Ràng buộc Role chỉ nhận Student, Lecturer, Admin
                entity.Property(u => u.Role)
                      .HasMaxLength(20) // VARCHAR(20)
                      .IsRequired() // NOT NULL
                      .HasConversion<string>(); // Chuyển đổi kiểu dữ liệu
                entity.HasCheckConstraint("CK_User_Role",
                          "[Role] IN ('Student', 'Lecturer', 'Admin')"); // Ràng buộc CHECK


                entity.Property(u => u.FullName)
                      .HasMaxLength(100) // VARCHAR(100)
                      .IsRequired(); // NOT NULL
                entity.Property(u => u.Email)
                      .HasMaxLength(100) // VARCHAR(100)
                      .IsRequired(); // NOT NULL
                entity.Property(u => u.Date_of_birth)
                      .IsRequired(); // NOT NULL
                entity.Property(u => u.Address)
                      .HasMaxLength(255) // VARCHAR(255)
                      .IsRequired(); // NOT NULL
                entity.Property(u => u.Phone_number)
                      .HasMaxLength(15) // VARCHAR(15)
                      .IsRequired(); // NOT NULL
                entity.Property(u => u.Gender)
                      .HasMaxLength(10) // VARCHAR(10)
                      .IsRequired(); // NOT NULL
            });

            // 5. ClassCourseFaculty
            modelBuilder.Entity<ClassCourseFaculty>(entity =>
            {
                entity.HasKey(ccf => ccf.ClassCourseFacultyID); // Khóa chính
                entity.Property(ccf => ccf.ClassID)
                      .IsRequired(); // NOT NULL
                entity.Property(ccf => ccf.UserID)
                      .IsRequired(); // NOT NULL
                entity.Property(ccf => ccf.CourseID)
                      .IsRequired(); // NOT NULL

                // Quan hệ với Class
                entity.HasOne(ccf => ccf.Class)
                      .WithMany()
                      .HasForeignKey(ccf => ccf.ClassID)
                      .OnDelete(DeleteBehavior.Restrict); // Không cho xóa Class nếu có liên kết

                // Quan hệ với User (Faculty)
                entity.HasOne(ccf => ccf.User)
                      .WithMany()
                      .HasForeignKey(ccf => ccf.UserID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ với Course
                entity.HasOne(ccf => ccf.Course)
                      .WithMany()
                      .HasForeignKey(ccf => ccf.CourseID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 6. StudentClass
            modelBuilder.Entity<StudentClass>(entity =>
            {
                entity.HasKey(sc => sc.StudentClassID); // Khóa chính
                entity.Property(sc => sc.ClassID)
                      .IsRequired(); // NOT NULL
                entity.Property(sc => sc.UserID)
                      .IsRequired(); // NOT NULL

                // Quan hệ với Class
                entity.HasOne(sc => sc.Class)
                      .WithMany()
                      .HasForeignKey(sc => sc.ClassID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ với User (Student)
                entity.HasOne(sc => sc.User)
                      .WithMany()
                      .HasForeignKey(sc => sc.UserID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 7. Enrollment
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.EnrollmentID); // Khóa chính
                entity.Property(e => e.UserID)
                      .IsRequired(); // NOT NULL
                entity.Property(e => e.ClassCourseFacultyID)
                      .IsRequired(); // NOT NULL
                entity.Property(e => e.EnrollmentDate)
                      .IsRequired(); // NOT NULL

                // Quan hệ với User (Student)
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ với ClassCourseFaculty
                entity.HasOne(e => e.ClassCourseFaculty)
                      .WithMany()
                      .HasForeignKey(e => e.ClassCourseFacultyID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 8. Grade
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.HasKey(g => g.GradeID); // Khóa chính
                entity.Property(g => g.EnrollmentID)
                      .IsRequired(); // NOT NULL
                entity.Property(g => g.Score)
                      .HasColumnType("decimal(5,2)") // DECIMAL(5,2)
                      .IsRequired(); // NOT NULL

                // Quan hệ với Enrollment
                entity.HasOne(g => g.Enrollment)
                      .WithMany()
                      .HasForeignKey(g => g.EnrollmentID)
                      .OnDelete(DeleteBehavior.Cascade); // Xóa Enrollment thì xóa Grade
            });
        }
    }
}
