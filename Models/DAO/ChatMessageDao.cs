using System;
using System.Collections.Generic;
using System.Linq;
using Models.EF;

namespace Models.DAO
{
    // DAO tinh nang chat noi bo (issue #114)
    public class ChatMessageDao : IDisposable
    {
        DBNoiThat db = new DBNoiThat();

        public void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
                db = null;
                GC.SuppressFinalize(this);
            }
        }

        // Chu hoa so dien thoai: bo cac ky tu khong phai so, roi bo so 0 dau
        // (User.Phone trong DB la INT nen 0901234567 luu thanh 901234567)
        public static string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            return digits.TrimStart('0');
        }

        public long Insert(ChatMessage entity)
        {
            entity.CreatedDate = DateTime.Now;
            db.ChatMessages.Add(entity);
            db.SaveChanges();
            return entity.ChatMessageId;
        }

        // Thread cua 1 nguoi dung: tin nhan gan UserId duy truc, cong tin nhan
        // "khach chua xac thuc" cung Email & Phone (do khi chua kich hoat tai khoan)
        public List<ChatMessage> ListThreadForUser(int userId, string email)
        {
            var mail = (email ?? string.Empty).Trim().ToLower();
            return db.ChatMessages
                     .Where(m => m.UserId == userId
                              || (m.UserId == null && m.Email.ToLower() == mail))
                     .OrderBy(m => m.CreatedDate)
                     .ThenBy(m => m.ChatMessageId)
                     .ToList();
        }

        // Tra cuu chat cho khach chua dang nhap: phai khop ca Email + Phone
        // (2 thong tin chi chu the tai khoan moi biet) -> khong lo rò OTP
        public List<ChatMessage> ListGuestThread(string email, string phone)
        {
            var mail = (email ?? string.Empty).Trim().ToLower();
            var dtb = NormalizePhone(phone);
            return db.ChatMessages
                     .Where(m => m.Email.ToLower() == mail && m.Phone == dtb)
                     .OrderBy(m => m.CreatedDate)
                     .ThenBy(m => m.ChatMessageId)
                     .ToList();
        }

        // Sau khi xac thuc OTP dang ky thanh cong, gan thread khach (UserId null)
        // vao tai khoan vua kich hoat
        public void LinkGuestThreadToUser(string email, string phone, int userId)
        {
            var mail = (email ?? string.Empty).Trim().ToLower();
            var dtb = NormalizePhone(phone);
            var guestMsgs = db.ChatMessages
                              .Where(m => m.UserId == null && m.Email.ToLower() == mail && m.Phone == dtb)
                              .ToList();
            foreach (var m in guestMsgs)
            {
                m.UserId = userId;
            }
            db.SaveChanges();
        }
    }
}
