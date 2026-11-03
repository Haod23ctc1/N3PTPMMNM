using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.EF
{
    [Table("IdentityVerification")]
    public class IdentityVerification
    {
        public int IdentityVerificationId { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [StringLength(20)]
        public string DocumentType { get; set; }

        [Required]
        [StringLength(64)]
        public string DocumentNumberHash { get; set; }

        [StringLength(45)]
        public string FrontFileName { get; set; }

        [StringLength(45)]
        public string BackFileName { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        [StringLength(500)]
        public string ReviewNote { get; set; }

        public DateTime SubmittedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public int? ReviewedByUserId { get; set; }
    }
}
