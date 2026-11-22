using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Models.EF;

namespace WebsiteNoiThat.Common
{
    // Du lieu that cho chuong thong bao + hop thu tren thanh quan tri.
    // - Issue #154: hop thu/chuong phai hien thi tin nhan khach gui that
    //   (chat ho tro + yeu cau lien he), khong con du lieu mau cua template.
    // - Issue #157: moi tin nhan co nut "Tra loi" mo dung trang xu ly
    //   (/Admin/SupportChat/Details cho chat, /Admin/Contact cho lien he).
    // Layout admin goi truc tiep AdminInboxHelper.Load() nen khong can them
    // action/route rieng cho moi trang.
    public static class AdminInboxHelper
    {
        private const int MaxMessages = 5;
        private const int MaxNotifications = 5;

        // 1 tin nhan hien thi trong dropdown
        public class InboxItem
        {
            public string Kind { get; set; }        // "chat" | "contact"
            public string Name { get; set; }        // ten khach / email
            public string Snippet { get; set; }     // noi dung khach gui (da rut gon)
            public string TimeText { get; set; }    // thoi gian than thien / ma yeu cau
            public string ReplyUrl { get; set; }    // trang tra loi tuong ung
            public bool Pending { get; set; }       // dang cho admin tra loi
            public string Icon { get; set; }        // FontAwesome class
            public string Color { get; set; }       // bg-* cua SB Admin
            public DateTime SortTime { get; set; }  // dung de sap xep
        }

        public class InboxData
        {
            public List<InboxItem> Messages { get; set; }
            public List<InboxItem> Notifications { get; set; }
            public int PendingCount { get; set; }
            public int PendingChatCount { get; set; }
            public int PendingContactCount { get; set; }

            public InboxData()
            {
                Messages = new List<InboxItem>();
                Notifications = new List<InboxItem>();
            }
        }

        public static InboxData Load()
        {
            var data = new InboxData();
            try
            {
                using (var db = new DBNoiThat())
                {
                    var userMap = db.Users.ToDictionary(u => u.UserId);

                    // ================= Chat ho tro =================
                    var messages = db.ChatMessages
                                     .OrderBy(m => m.CreatedDate)
                                     .ThenBy(m => m.ChatMessageId)
                                     .ToList();

                    var chatItems = new List<InboxItem>();
                    foreach (var group in messages.GroupBy(ThreadKey))
                    {
                        var thread = group.ToList();
                        var lastCustomer = thread.LastOrDefault(m => !m.IsSystem);

                        // Thread chi co tin he thong (OTP...) khong phai tin khach gui -> bo qua
                        if (lastCustomer == null) continue;

                        var lastReply = thread.LastOrDefault(IsSupportReply);
                        var user = ResolveUser(thread, userMap);
                        var email = user != null && !string.IsNullOrWhiteSpace(user.Email)
                            ? user.Email
                            : (lastCustomer.Email ?? thread.Select(m => m.Email).FirstOrDefault(e => !string.IsNullOrWhiteSpace(e)));
                        var phone = user != null && user.Phone != null
                            ? user.Phone.ToString()
                            : (lastCustomer.Phone ?? thread.Select(m => m.Phone).FirstOrDefault(p => !string.IsNullOrWhiteSpace(p)));

                        var name = user != null
                            ? (string.IsNullOrWhiteSpace(user.Name) ? user.Username : user.Name)
                            : (string.IsNullOrWhiteSpace(lastCustomer.Sender) ? email : lastCustomer.Sender);

                        var replyUrl = string.IsNullOrWhiteSpace(email)
                            ? "/Admin/SupportChat"
                            : "/Admin/SupportChat/Details?email=" + HttpUtility.UrlEncode(email) +
                              "&phone=" + HttpUtility.UrlEncode(phone ?? string.Empty);

                        chatItems.Add(new InboxItem
                        {
                            Kind = "chat",
                            Name = string.IsNullOrWhiteSpace(name) ? "(khách)" : name,
                            Snippet = Shorten(lastCustomer.Content, 90),
                            TimeText = FriendlyTime(lastCustomer.CreatedDate),
                            ReplyUrl = replyUrl,
                            Pending = lastReply == null || lastCustomer.CreatedDate > lastReply.CreatedDate,
                            Icon = "fa-comments",
                            Color = "bg-primary",
                            SortTime = lastCustomer.CreatedDate
                        });
                    }
                    chatItems = chatItems.OrderByDescending(i => i.SortTime).ToList();

                    // ================= Yeu cau lien he =================
                    var contactItems = db.Contacts
                                         .OrderByDescending(c => c.ContactId)
                                         .ToList()
                                         .Select(c => new InboxItem
                                         {
                                             Kind = "contact",
                                             Name = string.IsNullOrWhiteSpace(c.EmailCC) ? "Khách hàng" : c.EmailCC,
                                             Snippet = Shorten(StripReplyHistory(c.Content), 90),
                                             TimeText = "Yêu cầu #" + c.ContactId,
                                             ReplyUrl = "/Admin/Contact#reply-" + c.ContactId,
                                             Pending = c.Status != true,
                                             Icon = "fa-envelope",
                                             Color = c.Status != true ? "bg-warning" : "bg-success",
                                             SortTime = DateTime.MinValue
                                         })
                                         .ToList();

                    // ================= Tong hop =================
                    data.PendingChatCount = chatItems.Count(i => i.Pending);
                    data.PendingContactCount = contactItems.Count(i => i.Pending);
                    data.PendingCount = data.PendingChatCount + data.PendingContactCount;

                    // Hop thu: uu tien 3 hoi thoai chat moi nhat + 2 yeu cau lien he moi nhat,
                    // sau do bu them neu mot trong hai loai khong du.
                    data.Messages.AddRange(chatItems.Take(3));
                    data.Messages.AddRange(contactItems.Take(2));
                    if (data.Messages.Count < MaxMessages)
                    {
                        data.Messages.AddRange(chatItems.Skip(3).Take(MaxMessages - data.Messages.Count));
                    }
                    if (data.Messages.Count < MaxMessages)
                    {
                        data.Messages.AddRange(contactItems.Skip(2).Take(MaxMessages - data.Messages.Count));
                    }

                    // Chuong thong bao: chi cac muc dang cho tra loi, dam bao co ca 2 loai.
                    data.Notifications.AddRange(chatItems.Where(i => i.Pending).Take(3));
                    data.Notifications.AddRange(contactItems.Where(i => i.Pending).Take(2));
                    if (data.Notifications.Count < MaxNotifications)
                    {
                        data.Notifications.AddRange(chatItems.Where(i => i.Pending).Skip(3)
                            .Take(MaxNotifications - data.Notifications.Count));
                    }
                    if (data.Notifications.Count < MaxNotifications)
                    {
                        data.Notifications.AddRange(contactItems.Where(i => i.Pending).Skip(2)
                            .Take(MaxNotifications - data.Notifications.Count));
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                // Chua chay script tao bang ChatMessage -> admin van hien thi binh thuong
                System.Diagnostics.Trace.TraceError("AdminInboxHelper: ChatMessage table is missing: " + ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("AdminInboxHelper.Load failed: " + ex);
            }
            return data;
        }

        private static string ThreadKey(ChatMessage m)
        {
            return m.UserId.HasValue
                ? "U" + m.UserId.Value
                : "G" + (m.Email ?? string.Empty).Trim().ToLowerInvariant() + "|" + (m.Phone ?? string.Empty);
        }

        private static bool IsSupportReply(ChatMessage m)
        {
            return m.IsSystem &&
                   (m.Sender ?? string.Empty).StartsWith("Hỗ trợ viên", StringComparison.OrdinalIgnoreCase);
        }

        private static User ResolveUser(List<ChatMessage> thread, Dictionary<int, User> userMap)
        {
            var withUser = thread.FirstOrDefault(m => m.UserId.HasValue);
            if (withUser == null) return null;
            User user;
            return userMap.TryGetValue(withUser.UserId.Value, out user) ? user : null;
        }

        // Bo phan lich su phan hoi da luu trong Contact.Content
        private static string StripReplyHistory(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return string.Empty;
            var marker = content.IndexOf("[Đã phản hồi", StringComparison.Ordinal);
            return marker > 0 ? content.Substring(0, marker) : content;
        }

        private static string Shorten(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var flat = string.Join(" ", text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)).Trim();
            if (flat.Length <= maxLength) return flat;
            return flat.Substring(0, maxLength).TrimEnd() + "...";
        }

        private static string FriendlyTime(DateTime time)
        {
            var span = DateTime.Now - time;
            if (span.TotalMinutes < 1) return "Vừa xong";
            if (span.TotalMinutes < 60) return (int)span.TotalMinutes + " phút trước";
            if (span.TotalHours < 24) return (int)span.TotalHours + " giờ trước";
            if (span.TotalDays < 7) return (int)span.TotalDays + " ngày trước";
            return time.ToString("dd/MM/yyyy HH:mm");
        }
    }
}
