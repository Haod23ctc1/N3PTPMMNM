using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Models.EF;
using WebsiteNoiThat.Common;

namespace WebsiteNoiThat.Controllers
{
    public class IdentityVerificationController : Controller
    {
        private readonly DBNoiThat db = new DBNoiThat();

        [HttpGet]
        public ActionResult Submit()
        {
            int? userId = GetSubmissionUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "RegisterAndLogin");
            }

            IdentityVerification verification;
            try
            {
                verification = db.IdentityVerifications.SingleOrDefault(x => x.UserId == userId.Value);
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                System.Diagnostics.Trace.TraceError("IdentityVerification table is missing: " + ex);
                ModelState.AddModelError("", "Cơ sở dữ liệu chưa có bảng xác minh danh tính. " +
                    "Hãy chạy SQL/2026-11-03_tao_bang_identityverification.sql trên DBNoiThat.");
                return View();
            }
            ViewBag.IdentityStatus = verification == null ? null : verification.Status;
            ViewBag.ReviewNote = verification == null ? null : verification.ReviewNote;
            if (verification != null && verification.Status == "Approved" &&
                string.IsNullOrWhiteSpace(verification.DocumentNumberHash))
            {
                ViewBag.IdentityStatus = "Rejected";
                ViewBag.ReviewNote = "Hồ sơ cũ chưa có mã giấy tờ để đối chiếu. Vui lòng gửi lại giấy tờ.";
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(string documentType, HttpPostedFileBase frontDocument,
            HttpPostedFileBase backDocument, bool documentConsent = false)
        {
            int? userId = GetSubmissionUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "RegisterAndLogin");
            }

            if (documentType != "CCCD" && documentType != "Passport")
            {
                ModelState.AddModelError("", "Vui lòng chọn CCCD hoặc hộ chiếu.");
            }
            if (!documentConsent)
            {
                ModelState.AddModelError("", "Bạn cần đồng ý với việc sử dụng ảnh giấy tờ để xác minh danh tính.");
            }
            if (documentType == "CCCD" && (backDocument == null || backDocument.ContentLength <= 0))
            {
                ModelState.AddModelError("", "CCCD cần ảnh cả mặt trước và mặt sau.");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = db.IdentityVerifications.SingleOrDefault(x => x.UserId == userId.Value);
                    if (existing != null && (existing.Status == "Pending" ||
                        (existing.Status == "Approved" && !string.IsNullOrWhiteSpace(existing.DocumentNumberHash))))
                    {
                        ModelState.AddModelError("", "Hồ sơ đang chờ duyệt hoặc đã được xác minh.");
                    }
                    else
                    {
                        string frontFileName = null;
                        string backFileName = null;
                        try
                        {
                            frontFileName = IdentityDocumentStorage.Save(frontDocument);
                            if (documentType == "CCCD")
                            {
                                backFileName = IdentityDocumentStorage.Save(backDocument);
                            }

                            if (existing == null)
                            {
                                existing = new IdentityVerification
                                {
                                    UserId = userId.Value
                                };
                                db.IdentityVerifications.Add(existing);
                            }

                            existing.DocumentType = documentType;
                            existing.FrontFileName = frontFileName;
                            existing.BackFileName = backFileName;
                            existing.Status = "Pending";
                            existing.ReviewNote = null;
                            existing.SubmittedAt = DateTime.Now;
                            existing.ReviewedAt = null;
                            existing.ReviewedByUserId = null;
                            db.SaveChanges();

                            Session.Remove("Register_Data");
                            Session.Remove("Register_OTP");
                            Session.Remove("Register_ExpireTime");
                            TempData["IdentitySubmitted"] = true;
                            return RedirectToAction("Submit");
                        }
                        catch
                        {
                            IdentityDocumentStorage.Delete(frontFileName);
                            IdentityDocumentStorage.Delete(backFileName);
                            throw;
                        }
                    }
                }
                catch (InvalidDataException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (SqlException ex) when (ex.Number == 208)
                {
                    System.Diagnostics.Trace.TraceError("IdentityVerification table is missing: " + ex);
                    ModelState.AddModelError("", "Cơ sở dữ liệu chưa có bảng xác minh danh tính. " +
                        "Hãy chạy SQL/2026-11-03_tao_bang_identityverification.sql trên DBNoiThat.");
                    return View();
                }
            }

            IdentityVerification current;
            try
            {
                current = db.IdentityVerifications.SingleOrDefault(x => x.UserId == userId.Value);
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                System.Diagnostics.Trace.TraceError("IdentityVerification table is missing: " + ex);
                ModelState.AddModelError("", "Cơ sở dữ liệu chưa có bảng xác minh danh tính. " +
                    "Hãy chạy SQL/2026-11-03_tao_bang_identityverification.sql trên DBNoiThat.");
                return View();
            }
            ViewBag.IdentityStatus = current == null ? null : current.Status;
            ViewBag.ReviewNote = current == null ? null : current.ReviewNote;
            if (current != null && current.Status == "Approved" &&
                string.IsNullOrWhiteSpace(current.DocumentNumberHash))
            {
                ViewBag.IdentityStatus = "Rejected";
                ViewBag.ReviewNote = "Hồ sơ cũ chưa có mã giấy tờ để đối chiếu. Vui lòng gửi lại giấy tờ.";
            }
            return View();
        }

        private int? GetSubmissionUserId()
        {
            var session = Session[Commoncontent.user_sesion] as UserLogin;
            if (session != null)
            {
                return session.UserId;
            }

            var registrationUserId = Session["IdentityVerificationUserId"];
            return registrationUserId == null ? (int?)null : Convert.ToInt32(registrationUserId);
        }
    }
}
