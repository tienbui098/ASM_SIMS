
using System.ComponentModel.DataAnnotations;


namespace SIMS_ASM.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [StringLength(50)]
        public string Username { get; set; }

        [StringLength(50)]
        public string Password { get; set; }

        [StringLength(20)]
        public string Role { get; set; }

        [StringLength(100)]
        public string FullName { get; set; } 

        [StringLength(100)]
        public string Email { get; set; }

        public DateOnly Date_of_birth { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(15)]
        public string Phone_number { get; set; }

        [StringLength(10)]
        public string Gender { get; set; }

        // Navigation properties
        public virtual ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
        public virtual ICollection<ClassCourseFaculty> ClassCourseFaculties { get; set; } = new List<ClassCourseFaculty>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
