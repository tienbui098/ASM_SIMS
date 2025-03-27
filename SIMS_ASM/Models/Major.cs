using System.ComponentModel.DataAnnotations;

namespace SIMS_ASM.Models
{
    public class Major
    {
        [Key]
        public int MajorID { get; set; }

        [StringLength(100)]
        public string MajorName { get; set; }

        public DateTime CourseStartDate { get; set; }
        public DateTime CourseEndDate { get; set; }

        // Navigation properties
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
        public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}
