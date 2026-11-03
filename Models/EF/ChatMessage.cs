namespace Models.EF
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    // Tin nhan noi bo giua khach hang va "He thong"/ho tro (issue #114).
    // Moi luot OTP (dang ky / lay lai mat khau) cung duoc gui nhu 1 tin nhan system
    // vao thread cua nguoi dung, de doc ma ngay tren site neu mail khong toi.
    [Table("ChatMessage")]
    public partial class ChatMessage
    {
        public int ChatMessageId { get; set; }

        // UserId cua nguoi dung (null khi thread dang ky chua xac thuc -> do Email/Phone danh danh)
        public int? UserId { get; set; }

        // Email the cua thread ( dung de tra cuu tin nhan cho khach chua dang nhap )
        [StringLength(100)]
        public string Email { get; set; }

        // So dien thoai chuan hoa (bo so 0 dau) - thua so tra cuu chat voi User.Phone int
        [StringLength(15)]
        public string Phone { get; set; }

        // "He thong" = tin nhan server, so khac = ten nguoi gui
        [StringLength(50)]
        public string Sender { get; set; }

        [StringLength(MAX_NVARCHAR)]
        public string Content { get; set; }

        // true = tin nhan tu He thong (chua OTP...), false = khach gui cho ho tro
        public bool IsSystem { get; set; }

        public bool? Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public const int MAX_NVARCHAR = 4000;
    }
}
