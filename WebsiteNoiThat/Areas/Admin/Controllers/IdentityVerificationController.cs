using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using Models.EF;
using WebsiteNoiThat.Common;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    public class IdentityVerificationController : HomeController
    {
        private readonly DBNoiThat db = new DBNoiThat();

        [HasCredential(RoleId = "VIEW_ADMIN")]
        public override ActionResult Index()
        {
            var session = Session[Commoncontent.user_sesion_admin] as UserLogin;
            ViewBag.username = session.Username;

            var pending = db.IdentityVerifications
                .Where(x => x.Status == "Pending")
                .Include(x => x.User)
                .OrderBy(x => x.SubmittedAt)
                .ToList();
            return View(pending);
        }

        [HasCredential(RoleId = "VIEW_ADMIN")]
        public ActionResult ResetRequests()
        {
            var pending = db.PasswordResetVerifications
                .Where(x => x.Status == "Pending")
                .Include(x => x.User)
                .OrderBy(x => x.SubmittedAt)
                .ToList();
            return View(pending);
        }

        [HasCredential(RoleId = "VIEW_ADMIN")]
        public ActionResult ResetDetails(int id)
        {
            var request = db.PasswordResetVerifications
                .Include(x => x.User)
                .SingleOrDefault(x => x.PasswordResetVerificationId == id);
            if (request == null)
            {
                return HttpNotFound();
            }

            var identity = db.IdentityVerifications.SingleOrDefault(x => x.UserId == request.UserId);
            if (identity == null || identity.Status != "Approved")
            {
                return new HttpStatusCodeResult(409, "Không tìm thấy hồ sơ đăng ký đã xác minh.");
            }
            ViewBag.IdentityVerification = identity;
            return View(request);
        }

        [HasCredential(RoleId = "VIEW_ADMIN")]
        public ActionResult ResetDocument(int id, string side)
        {
            var request = db.PasswordResetVerifications.Find(id);
            if (request == null)
            {
                return HttpNotFound();
            }
            string fileName = side == "front" ? request.FrontFileName :
                side == "back" ? request.BackFileName : null;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return HttpNotFound();
            }

            try
            {
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetNoStore();
                Response.Headers["X-Content-Type-Options"] = "nosniff";
                return File(IdentityDocumentStorage.Read(fileName),
                    IdentityDocumentStorage.GetContentType(fileName));
            }
            catch (FileNotFoundException)
            {
                return HttpNotFound();
            }
            catch (InvalidDataException)
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredential(RoleId = "EDIT_ADMIN")]
        public ActionResult ApproveReset(int id, string documentNumber)
        {
            var request = db.PasswordResetVerifications.Find(id);
            if (request == null)
            {
                return HttpNotFound();
            }
            if (request.Status != "Pending")
            {
                TempData["Error"] = "Yêu cầu này đã được xử lý.";
                return RedirectToAction("ResetDetails", new { id });
            }

            var identity = db.IdentityVerifications.SingleOrDefault(x => x.UserId == request.UserId);
            if (identity == null || identity.Status != "Approved" ||
                identity.DocumentType != request.DocumentType)
            {
                TempData["Error"] = "Không tìm thấy hồ sơ giấy tờ đã xác minh để đối chiếu.";
                return RedirectToAction("ResetDetails", new { id });
            }
            if (string.IsNullOrWhiteSpace(identity.FrontFileName))
            {
                TempData["Error"] = "Hồ sơ đăng ký không còn ảnh gốc để so sánh. Không thể duyệt yêu cầu này.";
                return RedirectToAction("ResetDetails", new { id });
            }
            if (string.IsNullOrWhiteSpace(request.FrontFileName) ||
                (request.DocumentType == "CCCD" && string.IsNullOrWhiteSpace(request.BackFileName)))
            {
                TempData["Error"] = "Yêu cầu thiếu ảnh giấy tờ cần thiết. Không thể duyệt.";
                return RedirectToAction("ResetDetails", new { id });
            }
            try
            {
                IdentityDocumentStorage.Read(identity.FrontFileName);
                IdentityDocumentStorage.Read(request.FrontFileName);
                if (request.DocumentType == "CCCD")
                {
                    IdentityDocumentStorage.Read(identity.BackFileName);
                    IdentityDocumentStorage.Read(request.BackFileName);
                }
            }
            catch (FileNotFoundException ex)
            {
                System.Diagnostics.Trace.TraceError("Identity document image missing during reset review: " + ex);
                TempData["Error"] = "Không tìm thấy ảnh gốc hoặc ảnh tải lên. Không thể duyệt yêu cầu.";
                return RedirectToAction("ResetDetails", new { id });
            }
            catch (InvalidDataException ex)
            {
                System.Diagnostics.Trace.TraceError("Identity document filename invalid during reset review: " + ex);
                TempData["Error"] = "Tên tệp ảnh không hợp lệ. Không thể duyệt yêu cầu.";
                return RedirectToAction("ResetDetails", new { id });
            }
            if (!IdentityDocumentStorage.VerifyDocumentNumber(documentNumber, request.DocumentNumberHash) ||
                !IdentityDocumentStorage.VerifyDocumentNumber(documentNumber, identity.DocumentNumberHash))
            {
                TempData["Error"] = "Số đọc được trên ảnh không khớp với số giấy tờ đã đăng ký. Không thể duyệt.";
                return RedirectToAction("ResetDetails", new { id });
            }

            var user = db.Users.Find(request.UserId);
            if (user == null)
            {
                return HttpNotFound();
            }

            string otp = MailHelper.GenerateOTP();
            string otpHash;
            try
            {
                otpHash = IdentityDocumentStorage.ComputeDocumentNumberHash("OTP" + otp);
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Trace.TraceError("Reset OTP HMAC configuration error: " + ex);
                TempData["Error"] = "Chưa thể tạo OTP do cấu hình bảo mật chưa hợp lệ.";
                return RedirectToAction("ResetDetails", new { id });
            }

            string frontFileName = request.FrontFileName;
            string backFileName = request.BackFileName;
            var admin = Session[Commoncontent.user_sesion_admin] as UserLogin;
            try
            {
                IdentityDocumentStorage.SaveResetOtp(request.PasswordResetVerificationId, otp);
            }
            catch (IOException ex)
            {
                System.Diagnostics.Trace.TraceError("Could not save protected reset OTP: " + ex);
                TempData["Error"] = "Không thể lưu OTP bảo vệ để hiển thị cho khách. Hãy kiểm tra quyền ghi App_Data/ResetOtps.";
                return RedirectToAction("ResetDetails", new { id });
            }
            catch (CryptographicException ex)
            {
                System.Diagnostics.Trace.TraceError("Could not protect reset OTP: " + ex);
                TempData["Error"] = "Không thể bảo vệ OTP để hiển thị an toàn cho khách hàng.";
                return RedirectToAction("ResetDetails", new { id });
            }

            request.Status = "Approved";
            request.ReviewNote = null;
            request.ReviewedAt = DateTime.Now;
            request.ReviewedByUserId = admin.UserId;
            request.FrontFileName = string.Empty;
            request.BackFileName = null;
            request.ResetOtpHash = otpHash;
            request.ResetOtpExpiresAt = DateTime.Now.AddMinutes(5);
            try
            {
                db.SaveChanges();
            }
            catch
            {
                IdentityDocumentStorage.DeleteResetOtp(request.PasswordResetVerificationId);
                throw;
            }

            string body = string.Format(
                "<div><h3>Khôi phục mật khẩu</h3><p>Mã OTP đặt lại mật khẩu là <b>{0}</b>.</p><p>Mã có hiệu lực trong 5 phút. Không chia sẻ mã này.</p></div>",
                otp);
            bool usedMailFallback;
            bool sent = MailHelper.SendMail(user.Email,
                "Mã OTP khôi phục mật khẩu - Website Nội Thất", body, out usedMailFallback);

            DeleteReviewedDocuments(frontFileName, backFileName);
            TempData["Success"] = sent && !usedMailFallback
                ? "Đã đối chiếu và duyệt giấy tờ thành công. OTP đã gửi qua email và hiển thị tại trang quên mật khẩu trong 5 phút. Ảnh tải lên đã được xóa."
                : "Đã đối chiếu và duyệt giấy tờ thành công. OTP đã được bảo vệ và sẽ hiển thị tại trang quên mật khẩu trong 5 phút. Ảnh tải lên đã được xóa.";
            return RedirectToAction("ResetRequests");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredential(RoleId = "EDIT_ADMIN")]
        public ActionResult RejectReset(int id, string reviewNote)
        {
            var request = db.PasswordResetVerifications.Find(id);
            if (request == null)
            {
                return HttpNotFound();
            }
            if (request.Status != "Pending")
            {
                TempData["Error"] = "Yêu cầu này đã được xử lý.";
                return RedirectToAction("ResetDetails", new { id });
            }
            if (string.IsNullOrWhiteSpace(reviewNote))
            {
                TempData["Error"] = "Vui lòng ghi lý do từ chối.";
                return RedirectToAction("ResetDetails", new { id });
            }

            string frontFileName = request.FrontFileName;
            string backFileName = request.BackFileName;
            var admin = Session[Commoncontent.user_sesion_admin] as UserLogin;
            request.Status = "Rejected";
            request.ReviewNote = reviewNote.Trim().Length > 500
                ? reviewNote.Trim().Substring(0, 500)
                : reviewNote.Trim();
            request.ReviewedAt = DateTime.Now;
            request.ReviewedByUserId = admin.UserId;
            request.FrontFileName = string.Empty;
            request.BackFileName = null;
            request.ResetOtpHash = null;
            request.ResetOtpExpiresAt = null;
            db.SaveChanges();
            IdentityDocumentStorage.DeleteResetOtp(request.PasswordResetVerificationId);
            DeleteReviewedDocuments(frontFileName, backFileName);
            TempData["Success"] = "Đã từ chối yêu cầu và xóa ảnh giấy tờ.";
            return RedirectToAction("ResetRequests");
        }

        [HasCredential(RoleId = "VIEW_ADMIN")]
        public ActionResult Details(int id)
        {
            var verification = db.IdentityVerifications.Find(id);
            if (verification == null)
            {
                return HttpNotFound();
            }

            ViewBag.User = db.Users.Find(verification.UserId);
            return View(verification);
        }

        [HasCredential(RoleId = "VIEW_ADMIN")]
        public ActionResult Document(int id, string side)
        {
            var verification = db.IdentityVerifications.Find(id);
            if (verification == null)
            {
                return HttpNotFound();
            }

            string fileName;
            if (side == "front")
            {
                fileName = verification.FrontFileName;
            }
            else if (side == "back")
            {
                fileName = verification.BackFileName;
            }
            else
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return HttpNotFound();
            }

            try
            {
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetNoStore();
                Response.Headers["X-Content-Type-Options"] = "nosniff";
                return File(IdentityDocumentStorage.Read(fileName),
                    IdentityDocumentStorage.GetContentType(fileName));
            }
            catch (FileNotFoundException)
            {
                return HttpNotFound();
            }
            catch (InvalidDataException)
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredential(RoleId = "EDIT_ADMIN")]
        public ActionResult Approve(int id, string documentNumber)
        {
            return Review(id, true, null, documentNumber);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredential(RoleId = "EDIT_ADMIN")]
        public ActionResult Reject(int id, string reviewNote)
        {
            if (string.IsNullOrWhiteSpace(reviewNote))
            {
                TempData["Error"] = "Khi từ chối, hãy ghi rõ lý do để khách có thể gửi lại hồ sơ.";
                return RedirectToAction("Details", new { id });
            }
            if (reviewNote.Length > 500)
            {
                reviewNote = reviewNote.Substring(0, 500);
            }

            return Review(id, false, reviewNote.Trim(), null);
        }

        private ActionResult Review(int id, bool approved, string reviewNote, string documentNumber)
        {
            var verification = db.IdentityVerifications.Find(id);
            if (verification == null)
            {
                return HttpNotFound();
            }
            if (verification.Status != "Pending")
            {
                TempData["Error"] = "Hồ sơ này đã được xử lý.";
                return RedirectToAction("Details", new { id });
            }
            if (approved && !IdentityDocumentStorage.VerifyDocumentNumber(
                documentNumber, verification.DocumentNumberHash))
            {
                TempData["Error"] = "Số đọc được trên ảnh không khớp với số khai báo. Không thể duyệt hồ sơ.";
                return RedirectToAction("Details", new { id });
            }

            string frontFileName = verification.FrontFileName;
            string backFileName = verification.BackFileName;
            var admin = Session[Commoncontent.user_sesion_admin] as UserLogin;
            verification.Status = approved ? "Approved" : "Rejected";
            verification.ReviewNote = reviewNote;
            verification.ReviewedAt = DateTime.Now;
            verification.ReviewedByUserId = admin.UserId;
            if (!approved)
            {
                verification.FrontFileName = null;
                verification.BackFileName = null;
            }
            db.SaveChanges();

            if (!approved)
            {
                DeleteReviewedDocuments(frontFileName, backFileName);
            }
            TempData["Success"] = approved
                ? "Đã duyệt thành công giấy tờ tùy thân. Ảnh gốc được lưu riêng tư để xác minh khi khôi phục tài khoản."
                : "Đã từ chối hồ sơ. Ảnh giấy tờ đã được xóa; khách có thể gửi lại.";
            return RedirectToAction("Index");
        }

        private void DeleteReviewedDocuments(string frontFileName, string backFileName)
        {
            try
            {
                IdentityDocumentStorage.Delete(frontFileName);
                IdentityDocumentStorage.Delete(backFileName);
            }
            catch (IOException ex)
            {
                System.Diagnostics.Trace.TraceError("Could not delete reviewed identity documents: " + ex);
                TempData["Warning"] = "Quyết định đã lưu nhưng không xóa được một ảnh giấy tờ. Cần kiểm tra App_Data/IdentityDocuments.";
            }
        }
    }
}
