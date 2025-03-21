using System.Linq;
using System.Web.Mvc;
using WebRuou.Models;
using PagedList;

namespace WebRuou.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]

    public class UserController : Controller
    {
        DBRuouEntities db = new DBRuouEntities();

        // Hiển thị danh sách người dùng (có phân trang)
        public ActionResult Index(int? page, string searchString, string sortOrder)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var users = db.Users.AsQueryable();

            // Lọc theo tên, email hoặc địa chỉ
            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.FullName.Contains(searchString)
                                      || u.Email.Contains(searchString)
                                      || u.Address.Contains(searchString));
            }

            // Sắp xếp danh sách theo tiêu chí
            ViewBag.NameSortParam = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.EmailSortParam = sortOrder == "email_asc" ? "email_desc" : "email_asc";
            ViewBag.AddressSortParam = sortOrder == "address_asc" ? "address_desc" : "address_asc";
            ViewBag.StatusSortParam = sortOrder == "status_asc" ? "status_desc" : "status_asc";

            switch (sortOrder)
            {
                case "name_asc":
                    users = users.OrderBy(u => u.FullName);
                    break;
                case "name_desc":
                    users = users.OrderByDescending(u => u.FullName);
                    break;
                case "email_asc":
                    users = users.OrderBy(u => u.Email);
                    break;
                case "email_desc":
                    users = users.OrderByDescending(u => u.Email);
                    break;
                case "address_asc":
                    users = users.OrderBy(u => u.Address);
                    break;
                case "address_desc":
                    users = users.OrderByDescending(u => u.Address);
                    break;
                case "status_asc":
                    users = users.OrderBy(u => u.IsActive);
                    break;
                case "status_desc":
                    users = users.OrderByDescending(u => u.IsActive);
                    break;
                default:
                    users = users.OrderByDescending(u => u.CreatedAt);
                    break;
            }

            ViewBag.SearchString = searchString;
            ViewBag.SortOrder = sortOrder;

            return View(users.ToPagedList(pageNumber, pageSize));
        }
        // Kích hoạt hoặc vô hiệu hóa tài khoản
        public ActionResult ToggleStatus(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Thay đổi trạng thái
            user.IsActive = !user.IsActive;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // Xem danh sách sản phẩm yêu thích của người dùng
        public ActionResult Wishlist(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var wishlistItems = db.Wishlists.Where(w => w.UserID == id).ToList();
            return View(wishlistItems);
        }
    }
}
