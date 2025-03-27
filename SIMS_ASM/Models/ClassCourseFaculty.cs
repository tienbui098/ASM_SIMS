using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SIMS_ASM.Models
{
    public class ClassCourseFaculty
    {
        [Key]
        public int ClassCourseFacultyID { get; set; }

        [ForeignKey("Class")]
        public int ClassID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; } // FacultyID

        [ForeignKey("Course")]
        public int CourseID { get; set; }

        // Navigation properties
        public virtual Class Class { get; set; }
        public virtual User User { get; set; } // Faculty
        public virtual Course Course { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}
