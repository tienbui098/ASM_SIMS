using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_ASM.Models
{
    public class Class
    {
        [Key]
        public int ClassID { get; set; }

        [ForeignKey("Major")]
        public int MajorID { get; set; }

        [StringLength(100)]
        public string ClassName { get; set; }

        // Navigation properties
        public virtual Major Major { get; set; }
        public virtual ICollection<StudentClass> StudentClasses { get; set; }
        public virtual ICollection<ClassCourseFaculty> ClassCourseFaculties { get; set; }

    }
}
