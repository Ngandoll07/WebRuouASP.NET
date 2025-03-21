using PagedList;
using System;
using System.Linq;
using System.Web.Mvc;
using WebRuou.Models;
using System.Data.Entity;
using System.Collections.Generic;


namespace WebRuou.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private DBRuouEntities db = new DBRuouEntities();

        // Hiển thị danh sách đơn hàng
        public ActionResult Index(int? page, string searchString, string status, DateTime? startDate, DateTime? endDate)
        {
            int pageSize = 10;
            int pageNum = (page ?? 1);

            var orders = db.Orders.AsQueryable();

            // Lọc theo tên khách hàng
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.User.FullName.Contains(searchString));
            }

            // Lọc theo trạng thái đơn hàng
            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            // Lọc theo ngày đặt hàng
            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= startDate);
            }
            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= endDate);
            }

            ViewBag.SearchString = searchString;

            ViewBag.Status = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Chọn trạng thái", Selected = string.IsNullOrEmpty(status) },
                new SelectListItem { Value = "Chờ xác nhận", Text = "Chờ xác nhận", Selected = status == "Chờ xác nhận" },
                new SelectListItem { Value = "Đang giao", Text = "Đang giao", Selected = status == "Đang giao" },
                new SelectListItem { Value = "Hoàn thành", Text = "Hoàn thành", Selected = status == "Hoàn thành" },
                new SelectListItem { Value = "Đã hủy", Text = "Đã hủy", Selected = status == "Đã hủy" }
            };


            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(orders.OrderByDescending(o => o.OrderDate).ToPagedList(pageNum, pageSize));
        }


        // Chi tiết đơn hàng
        public ActionResult Details(int id)
        {
            var order = db.Orders
                .Include(o => o.User) // Load thông tin khách hàng (User)
                .Include(o => o.OrderDetails.Select(d => d.Product)) // Load sản phẩm
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // Chỉnh sửa đơn hàng
        public ActionResult Edit(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Order order)
        {
            if (ModelState.IsValid)
            {
                var existingOrder = db.Orders.Find(order.OrderID);
                if (existingOrder == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật trạng thái đơn hàng
                existingOrder.Status = order.Status;
                existingOrder.TotalAmount = order.TotalAmount;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(order);
        }
    }
}
