using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Models;

namespace SIMS_ASM.Data
{
    public class ApplicationDbContex : DbContext
    {
        // Constructor: Nhận options để cấu hình DbContext (kế thừa từ lớp cha DbContext)
        public ApplicationDbContex(DbContextOptions<ApplicationDbContex> options) : base(options)
        {
        }

        // DbSets: Đại diện cho các bảng trong cơ sở dữ liệu
        public DbSet<Major> Majors { get; set; } // Bảng ngành học
        public DbSet<Course> Courses { get; set; } // Bảng khóa học
        public DbSet<Class> Classes { get; set; } // Bảng lớp học
        public DbSet<User> Users { get; set; } // Bảng người dùng
        public DbSet<ClassCourseFaculty> ClassCourseFaculties { get; set; } // Bảng liên kết lớp, khóa học, giảng viên
        public DbSet<StudentClass> StudentClasses { get; set; } // Bảng liên kết sinh viên và lớp
        public DbSet<Enrollment> Enrollments { get; set; } // Bảng ghi danh
        public DbSet<Grade> Grades { get; set; } // Bảng điểm số

        // Phương thức cấu hình mô hình dữ liệu và quan hệ giữa các bảng
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Major: Cấu hình bảng ngành học
            modelBuilder.Entity<Major>(entity =>
            {
                entity.HasKey(m => m.MajorID); // Đặt MajorID làm khóa chính
                entity.Property(m => m.MajorName)
                      .HasMaxLength(100) // Giới hạn độ dài tên ngành học là 100 ký tự
                      .IsRequired(); // Bắt buộc nhập (NOT NULL)

                // Quan hệ 1-n: Một ngành học có nhiều khóa học
                entity.HasMany(m => m.Courses)
                      .WithOne(c => c.Major)
                      .HasForeignKey(c => c.MajorID)
                      .OnDelete(DeleteBehavior.Cascade); // Xóa ngành học thì xóa luôn các khóa học liên quan

                // Quan hệ 1-n: Một ngành học có nhiều lớp học
                entity.HasMany(m => m.Classes)
                      .WithOne(c => c.Major)
                      .HasForeignKey(c => c.MajorID)
                      .OnDelete(DeleteBehavior.Cascade); // Xóa ngành học thì xóa luôn các lớp học liên quan
            });

            // 2. Course: Cấu hình bảng khóa học
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(c => c.CourseID); // Đặt CourseID làm khóa chính
                entity.Property(c => c.CourseName)
                      .HasMaxLength(100) // Giới hạn độ dài tên khóa học là 100 ký tự
                      .IsRequired(); // Bắt buộc nhập
                entity.Property(c => c.MajorID)
                      .IsRequired(); // Bắt buộc nhập khóa ngoại MajorID
            });

            // 3. Class: Cấu hình bảng lớp học
            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasKey(c => c.ClassID); // Đặt ClassID làm khóa chính
                entity.Property(c => c.ClassName)
                      .HasMaxLength(100) // Giới hạn độ dài tên lớp học là 100 ký tự
                      .IsRequired(); // Bắt buộc nhập
                entity.Property(c => c.MajorID)
                      .IsRequired(); // Bắt buộc nhập khóa ngoại MajorID
            });

            // 4. User: Cấu hình bảng người dùng
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserID); // Đặt UserID làm khóa chính

                // Đảm bảo Username là duy nhất
                entity.HasIndex(u => u.Username)
                      .IsUnique(); // Tạo chỉ mục duy nhất cho Username

                entity.Property(u => u.Username)
                      .HasMaxLength(50) // Giới hạn độ dài Username là 50 ký tự
                      .IsRequired(); // Bắt buộc nhập

                entity.Property(u => u.Password)
                      .HasMaxLength(50) // Giới hạn độ dài Password là 50 ký tự
                      .IsRequired(); // Bắt buộc nhập

                // Cấu hình Role với ràng buộc giá trị
                entity.Property(u => u.Role)
                      .HasMaxLength(20) // Giới hạn độ dài Role là 20 ký tự
                      .IsRequired() // Bắt buộc nhập
                      .HasConversion<string>(); // Chuyển đổi kiểu dữ liệu sang string
                entity.HasCheckConstraint("CK_User_Role",
                          "[Role] IN ('Student', 'Lecturer', 'Admin')"); // Ràng buộc Role chỉ nhận 3 giá trị

                entity.Property(u => u.FullName)
                      .HasMaxLength(100) // Giới hạn độ dài họ tên là 100 ký tự
                      .IsRequired(); // Bắt buộc nhập
                entity.Property(u => u.Email)
                      .HasMaxLength(100) // Giới hạn độ dài email là 100 ký tự
                      .IsRequired(); // Bắt buộc nhập
                entity.Property(u => u.Date_of_birth)
                      .IsRequired(); // Bắt buộc nhập ngày sinh
                entity.Property(u => u.Address)
                      .HasMaxLength(255) // Giới hạn độ dài địa chỉ là 255 ký tự
                      .IsRequired(); // Bắt buộc nhập
                entity.Property(u => u.Phone_number)
                      .HasMaxLength(15) // Giới hạn độ dài số điện thoại là 15 ký tự
                      .IsRequired(); // Bắt buộc nhập
                entity.Property(u => u.Gender)
                      .HasMaxLength(10) // Giới hạn độ dài giới tính là 10 ký tự
                      .IsRequired(); // Bắt buộc nhập
            });

            // 5. ClassCourseFaculty: Cấu hình bảng liên kết lớp, khóa học, giảng viên
            modelBuilder.Entity<ClassCourseFaculty>(entity =>
            {
                entity.HasKey(ccf => ccf.ClassCourseFacultyID); // Đặt ClassCourseFacultyID làm khóa chính
                entity.Property(ccf => ccf.ClassID)
                      .IsRequired(); // Bắt buộc nhập khóa ngoại ClassID
                entity.Property(ccf => ccf.UserID)
                      .IsRequired(); // Bắt buộc nhập khóa ngoại UserID
                entity.Property(ccf => ccf.CourseID)
                      .IsRequired(); // Bắt buộc nhập khóa ngoại CourseID

                // Quan hệ 1-n với Class (không xóa Class nếu có liên kết)
                entity.HasOne(ccf => ccf.Class)
                      .WithMany()
                      .HasForeignKey(ccf => ccf.ClassID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ 1-n với User (giảng viên)
                entity.HasOne(ccf => ccf.User)
                      .WithMany()
                      .HasForeignKey(ccf => ccf.UserID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ 1-n với Course
                entity.HasOne(ccf => ccf.Course)
                      .WithMany()
                      .HasForeignKey(ccf => ccf.CourseID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 6. StudentClass: Cấu hình bảng liên kết sinh viên và lớp
            modelBuilder.Entity<StudentClass>(entity =>
            {
                entity.HasKey(sc => sc.StudentClassID); // Đặt StudentClassID làm khóa chính
                entity.Property(sc => sc.ClassID)
                      .IsRequired(); // Bắt buộc nhập khóa ngoại ClassID
                entity.Property(sc => sc.UserID)
                      .IsRequired(); // Bắt buộc nhập khóa ngoại UserID

                // Quan hệ 1-n với Class
                entity.HasOne(sc => sc.Class)
                      .WithMany()
                      .HasForeignKey(sc => sc.ClassID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ 1-n với User (sinh viên)
                entity.HasOne(sc => sc.User)
                      .WithMany()
                      .HasForeignKey(sc => sc.UserID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 7. Enrollment: Cấu hình bảng ghi danh
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.EnrollmentID); // Đặt EnrollmentID làm khóa chính
                entity.Property(e => e.UserID)
                      .IsRequired(); // Bắt buộc nhập khóa ngoại UserID
                entity.Property(e => e.ClassCourseFacultyID)
                      .IsRequired(); // Bắt buộc nhập khóa ngoại ClassCourseFacultyID
                entity.Property(e => e.EnrollmentDate)
                      .IsRequired(); // Bắt buộc nhập ngày ghi danh

                // Quan hệ 1-n với User (sinh viên)
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ 1-n với ClassCourseFaculty
                entity.HasOne(e => e.ClassCourseFaculty)
                      .WithMany()
                      .HasForeignKey(e => e.ClassCourseFacultyID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 8. Grade: Cấu hình bảng điểm số
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.HasKey(g => g.GradeID); // Đặt GradeID làm khóa chính
                entity.Property(g => g.EnrollmentID)
                      .IsRequired(); // Bắt buộc nhập khóa ngoại EnrollmentID
                entity.Property(g => g.Score)
                      .HasColumnType("decimal(5,2)") // Định dạng điểm số là DECIMAL(5,2)
                      .IsRequired(); // Bắt buộc nhập

                // Quan hệ 1-n với Enrollment
                entity.HasOne(g => g.Enrollment)
                      .WithMany()
                      .HasForeignKey(g => g.EnrollmentID)
                      .OnDelete(DeleteBehavior.Cascade); // Xóa Enrollment thì xóa luôn Grade liên quan
            });
        }
    }
}