using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.EF;

namespace Models.DAO
{
    // DAO cua tinh nang danh gia san pham (issue #76)
    public class FeedbackDao : IDisposable
    {
        DBNoiThat db = new DBNoiThat();

        public void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
                db = null;
            }
            GC.SuppressFinalize(this);
        }

        // Danh sach danh gia moi nhat len truoc: kem theo ten hien thi cua nguoi danh gia
        public List<FeedbackView> ListByProduct(int productId)
        {
            var query = from f in db.Feedbacks
                        join u in db.Users on f.UserId equals u.UserId
                        where f.ProductId == productId
                        orderby f.Time descending, f.FeedbackId descending
                        select new FeedbackView
                        {
                            FeedbackId = f.FeedbackId,
                            Name = u.Name,
                            Username = u.Username,
                            Rate = f.Rate,
                            Content = f.Content,
                            Time = f.Time
                        };
            return query.ToList();
        }

        public double? AverageRate(int productId)
        {
            var query = db.Feedbacks.Where(f => f.ProductId == productId);
            if (!query.Any())
                return null;
            return query.Average(f => (double)f.Rate);
        }

        public int CountByProduct(int productId)
        {
            return db.Feedbacks.Count(f => f.ProductId == productId);
        }

        public long Insert(Feedback entity)
        {
            db.Feedbacks.Add(entity);
            db.SaveChanges();
            return entity.FeedbackId;
        }
    }

    // Doi tuong view cho danh gia: mot feedback + ten nguoi dung de hien thi
    public class FeedbackView
    {
        public int FeedbackId { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public int Rate { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Name))
                    return Name;
                if (!string.IsNullOrWhiteSpace(Username))
                    return Username;
                return "Khách ẩn danh";
            }
        }
    }
}
