using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.EF;
namespace Models.DAO
{

    public class ProductDao : IDisposable
    {
        private DBNoiThat db = new DBNoiThat();

        public List<Product> ListSanPham()
        {
            return db.Products.ToList();
        }

        // Lay san pham moi theo danh muc, khong tinh san pham dang giam gia
        private List<Product> ProductByCategory(int cateId, int take)
        {
            return db.Products
                .Where(n => n.Discount == 0 || n.EndDate < DateTime.Now || n.StartDate > DateTime.Now)
                .Where(n => n.CateId == cateId)
                .Take(take)
                .ToList();
        }

        public List<Product> CateKorea()
        {
            return ProductByCategory(9, 4);
        }
        public List<Product> CateHavana()
        {
            return ProductByCategory(8, 4);
        }
        public List<ProductView> ProductHot()
        {
            var model = (from a in db.Products
                         join b in db.OrderDetails on a.ProductId equals b.ProductId
                         group b by new { a.Description,a.ProductId,a.Photo,a.Price,a.Discount,a.EndDate,a.StartDate }
                         into g
                         select new ProductView
                         {
                            Description= g.Key.Description,
                            ProductId=g.Key.ProductId,
                            Price=g.Key.Price,
                            Discount=g.Key.Discount,
                            StartDate=g.Key.StartDate,
                            EndDate=g.Key.EndDate,
                            Photo=g.Key.Photo,

                            Quantity = g.Sum(s => s.Quantity),

                         }).OrderByDescending(n => n.Quantity).Take(6).ToList();
           return model;
        }

        public List<Product> SaleProduct() {
            var model = db.Products.Where(n => n.Discount > 0 ).OrderByDescending(n => n.Discount).Take(8).ToList();
            return model;
        }

        public List<Product> NewProduct()
        {
            var now = DateTime.Now;
            // issue #153: sap xep san pham moi theo ID san pham (ID lon = them moi xep truoc)
            return db.Products.Where(n => n.Discount == 0 || n.EndDate < now || n.StartDate > now).OrderByDescending(n => n.ProductId).Take(8).ToList();
        }
        public Product DetailsProduct(int Id)
        {
           return db.Products.SingleOrDefault(n => n.ProductId == Id);
        }

        //in ra loai san pham

        public List<Product> ListByCategoryId(int cateId, ref int total, int pageindex = 1, int pagesize = 12, int sort = 0, bool onlyAvailable = false)
        {
            // issue #99: hỗ trợ sắp xếp + lọc "chỉ còn hàng"; giữ hành vi cũ khi không truyền tham số
            var query = db.Products.AsQueryable();
            if (onlyAvailable)
            {
                query = query.Where(x => (x.Quantity ?? 0) > 0);
            }
            total = query.Where(x => x.CateId == cateId).Count();

            switch (sort)
            {
                case 1:  // giá tăng dần
                    query = query.Where(x => x.CateId == cateId).OrderBy(x => x.Price);
                    break;
                case 2:  // giá giảm dần
                    query = query.Where(x => x.CateId == cateId).OrderByDescending(x => x.Price);
                    break;
                case 3:  // tên A - Z
                    query = query.Where(x => x.CateId == cateId).OrderBy(x => x.Name);
                    break;
                case 4:  // mới nhất
                    query = query.Where(x => x.CateId == cateId).OrderByDescending(x => x.ProductId);
                    break;
                default: // giữ nguyên hành vi cũ (giá giảm dần)
                    query = query.Where(x => x.CateId == cateId).OrderByDescending(x => x.Price);
                    break;
            }

            var model = query.Skip((pageindex - 1) * pagesize).Take(pagesize).ToList();
            return model;
        }
        // issue #109: san pham lien quan - cung danh muc, bo chinh no, uu tien con hang
        public List<Product> ListRelated(int? cateId, int excludeProductId, int take = 6)
        {
            var query = db.Products.Where(n => n.ProductId != excludeProductId);
            if (cateId.HasValue)
            {
                query = query.Where(n => n.CateId == cateId.Value);
            }
            var list = query.ToList();
            list.Sort((a, b) =>
            {
                var conA = (a.Quantity ?? 0) > 0 ? 1 : 0;
                var conB = (b.Quantity ?? 0) > 0 ? 1 : 0;
                if (conA != conB) return conB - conA; // con hang truoc
                return b.ProductId - a.ProductId;     // sp moi hon truoc
            });
            return list.Take(take).ToList();
        }

        public List<string> ListName(string keyword)
        {

            return db.Products.Where(n => n.Name.Contains(keyword)).Select(n => n.Name).Distinct().ToList();
        }
        public List<Product> Search(string keyword, ref int total, int pageindex = 1, int pagesize = 12)
        {
            total = db.Products.Where(x => x.Name.Contains(keyword)).Count();
            var model = db.Products.Where(n => n.Name.Contains(keyword)).OrderByDescending(x => x.Price).Skip((pageindex - 1) * pagesize).Take(pagesize).ToList();
            return model;
        }
        public Product ViewDetail(int id)
        {
            return db.Products.Find(id);
        }
        public bool Update(Product entity, bool autoSave = true)
        {
            try
            {

                db.Products.Attach(entity);
                db.Entry(entity).State = System.Data.Entity.EntityState.Modified;
                if (autoSave)
                    return Convert.ToBoolean(db.SaveChanges());
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
                db = null;
            }
            GC.SuppressFinalize(this);
        }
    }

}
