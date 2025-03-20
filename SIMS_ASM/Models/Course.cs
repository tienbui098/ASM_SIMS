using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_ASM.Models
{
    public class Course
    {
        [Key]
        public int CourseID { get; set; }

        [ForeignKey("Grade")]
        public int? GradeID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string CourseName { get; set; }

        public DateTime? CourseStartDate { get; set; }
        public DateTime? CourseEndDate { get; set; }

        // Quan hệ
        public Grade Grade { get; set; }
        public User User { get; set; }
        // Xóa ICollection<Grade> Grades nếu không cần thiết
        public ICollection<Grade> Grades { get; set; }
    }
 
}

