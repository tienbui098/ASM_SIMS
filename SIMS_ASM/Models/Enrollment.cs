using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SIMS_ASM.Models
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; } // StudentID

        [ForeignKey("ClassCourseFaculty")]
        public int ClassCourseFacultyID { get; set; }

        public DateTime EnrollmentDate { get; set; }

        // Navigation properties
        public virtual User User { get; set; } // Student
        public virtual ClassCourseFaculty ClassCourseFaculty { get; set; }
        public virtual ICollection<Grade> Grades { get; set; }
    }
}
