using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using WebRuou.Models;

namespace WebRuou.Controllers
{
    public class ReviewController : Controller
    {
        private DBRuouEntities db=new DBRuouEntities();
        // GET: Review
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SubmitReview(int ProductID, int Rating, string Comment)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var externalId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Lấy UserID từ claims

            var user = db.Users.FirstOrDefault(u => u.ExternalId == externalId);
            if (user == null)
            {
                TempData["Error"] = "Lỗi xác thực người dùng!";
                return RedirectToAction("Login", "Account");
            }

            var review = new Review
            {
                ProductID = ProductID,
                UserID = user.UserID, // Chỉ lưu ID của User
                Rating = Rating,
                Comment = Comment,
                ReviewDate = DateTime.Now,
                IsHidden = false // Mặc định là hiển thị
            };

            db.Reviews.Add(review);
            db.SaveChanges();

            TempData["Success"] = "Đánh giá của bạn đã được gửi!";
            return RedirectToAction("OrderDetail", "Checkout", new { id = ProductID });
        }

    }
}