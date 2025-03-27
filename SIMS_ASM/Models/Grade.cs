using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_ASM.Models
{
    public class Grade
    {
        [Key]
        public int GradeID { get; set; }

        [ForeignKey("Enrollment")]
        public int EnrollmentID { get; set; }

        public decimal Score { get; set; }

        // Navigation properties
        public virtual Enrollment Enrollment { get; set; }
    }
}
