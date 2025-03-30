using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_ASM.Models
{
    public class Course
    {
        [Key]
        public int CourseID { get; set; }

        [ForeignKey("Major")]
        public int MajorID { get; set; }

        [StringLength(100)]
        public string CourseName { get; set; }

        //public DateTime CourseStartDate { get; set; }
        //public DateTime CourseEndDate { get; set; }

        // Navigation properties
        public virtual Major Major { get; set; }
        public virtual ICollection<ClassCourseFaculty> ClassCourseFaculties { get; set; }
    }

}

