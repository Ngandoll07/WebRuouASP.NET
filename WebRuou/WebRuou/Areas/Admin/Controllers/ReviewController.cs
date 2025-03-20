using System;
using System.Linq;
using System.Web.Mvc;
using WebRuou.Models;
using PagedList;

namespace WebRuou.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReviewController : Controller
    {
        DBRuouEntities db = new DBRuouEntities();

        public ActionResult Index(int? page)
        {
            int pageSize = 10; // Số đánh giá trên mỗi trang
            int pageNumber = (page ?? 1);

            var reviews = db.Reviews
                            .OrderByDescending(r => r.ReviewDate)
                            .ToPagedList(pageNumber, pageSize);

            return View(reviews);
        }
        // Đổi trạng thái hiển thị / ẩn của đánh giá
        public ActionResult ToggleVisibility(int id)
        {
            var review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }

            // Thay đổi trạng thái
            review.IsHidden = !review.IsHidden;
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
