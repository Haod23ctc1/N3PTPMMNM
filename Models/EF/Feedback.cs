namespace Models.EF
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    // Danh gia san pham (issue #76): moi nguoi dung co the gui nhieu feedback (Rate 1..5 sao)
    [Table("Feedbacks")]
    public partial class Feedback
    {
        public int FeedbackId { get; set; }

        public int ProductId { get; set; }

        public int UserId { get; set; }

        [Range(1, 5)]
        public int Rate { get; set; }

        [StringLength(500)]
        public string Content { get; set; }

        public DateTime Time { get; set; }
    }
}
