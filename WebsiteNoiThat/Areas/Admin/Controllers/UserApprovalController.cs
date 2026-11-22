using System; // Cần cho một số kiểu dữ liệu cơ bản
using System.Linq;
using System.Web; // CẦN THIẾT cho HttpPostedFileBase
using System.Web.Mvc;
using Models.EF;
using WebsiteNoiThat.Common; // CẦN THIẾT cho HasCredentialAttribute
using WebsiteNoiThat.Models; // Nếu cần cho các model khác

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    public class UserApprovalController : Controller
    {
        private DBNoiThat db = new DBNoiThat();

        public ActionResult Index()
        {
            // Chỉ hiển thị hồ sơ chờ duyệt
            var model = db.IdentityVerifications.Where(x => x.Status == "Pending").ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Approve(int id)
        {
            var verify = db.IdentityVerifications.Find(id);
            if (verify != null)
            {
                verify.Status = "Approved"; // Đã duyệt
                var user = db.Users.Find(verify.UserId);
                if (user != null) user.Status = true; // Cho phép đăng nhập
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        [HasCredential(RoleId = "EDIT_ADMIN")]
        public ActionResult UpdateDocument(int id, HttpPostedFileBase upload)
        {
            var verify = db.IdentityVerifications.Find(id);
            if (verify != null && upload != null)
            {
                // 1. Xóa ảnh cũ nếu có
                IdentityDocumentStorage.Delete(verify.FrontFileName);
                
                // 2. Lưu ảnh mới (Gán vào FrontFileName)
                verify.FrontFileName = IdentityDocumentStorage.Save(upload);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HasCredential(RoleId = "VIEW_ADMIN")]
        public ActionResult ViewDocument(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return HttpNotFound();
            
            try
            {
                byte[] fileBytes = IdentityDocumentStorage.Read(fileName);
                string contentType = IdentityDocumentStorage.GetContentType(fileName);
                return File(fileBytes, contentType);
            }
            catch (Exception)
            {
                return HttpNotFound("Không tìm thấy tệp tin.");
            }
        }
    }
}