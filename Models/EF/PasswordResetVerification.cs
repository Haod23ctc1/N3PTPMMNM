using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.EF
{
    [Table("PasswordResetVerification")]
    public class PasswordResetVerification
    {
        public int PasswordResetVerificationId { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        [StringLength(64)]
        public string DocumentNumberHash { get; set; }

        [Required]
        [StringLength(20)]
        public string DocumentType { get; set; }

        [Required(AllowEmptyStrings = true)]
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

        [StringLength(64)]
        public string ResetOtpHash { get; set; }

        public DateTime? ResetOtpExpiresAt { get; set; }
    }
}
