using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_ASM.Models
{
    public class Grade
    {
        [Key]
        public int GradeID { get; set; }

        [ForeignKey("Course")]
        public int CourseID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal GradeValue { get; set; } // Điểm số

        // Quan hệ
        public Course Course { get; set; }
        public User User { get; set; }
    }
}
