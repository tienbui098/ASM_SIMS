
using System.ComponentModel.DataAnnotations;


namespace SIMS_ASM.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; } // Mật khẩu mã hóa

        [Required]
        [StringLength(20)]
        public string Role { get; set; } // Student, Lecturer, Administrator

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(10)]
        public string Gender { get; set; }

        // Quan hệ 1-nhiều với Course, Grade, và RequestSupport
        public ICollection<Course> Courses { get; set; } = new List<Course>();
        public ICollection<Grade> Grades { get; set; } = new List<Grade>();
        public ICollection<RequestSupport> RequestSupports { get; set; } = new List<RequestSupport>();
    }
}
