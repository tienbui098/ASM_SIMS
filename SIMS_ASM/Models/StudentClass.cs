using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SIMS_ASM.Models
{
    public class StudentClass
    {
        [Key]
        public int StudentClassID { get; set; }

        [ForeignKey("Class")]
        public int ClassID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; } // StudentID

        // Navigation properties
        public virtual Class Class { get; set; }
        public virtual User User { get; set; } // Student
    }
}
