using PagedList;
using System;
using System.Linq;
using System.Web.Mvc;
using WebRuou.Models;

namespace WebRuou.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private DBRuouEntities db = new DBRuouEntities();

        // Hiển thị danh sách đơn hàng
        public ActionResult Index(int? page)
        {
            int pageSize = 10;
            int pageNum = (page ?? 1);

            var orders = db.Orders.OrderByDescending(o => o.OrderDate).ToPagedList(pageNum, pageSize);
            return View(orders);
        }

        // Chi tiết đơn hàng
        public ActionResult Details(int id)
        {
            var order = db.Orders.Find(id);
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
