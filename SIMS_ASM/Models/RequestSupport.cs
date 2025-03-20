using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_ASM.Models
{
    public class RequestSupport
    {
        [Key]
        public int RequestSupportID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [Required]
        public string RequestDetails { get; set; }

        [Required]
        [StringLength(20)]
        public string RequestStatus { get; set; } // In Progress, Completed

        public DateTime? RequestDate { get; set; }

        // Quan hệ
        public User User { get; set; }
    }
}
