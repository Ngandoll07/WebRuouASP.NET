using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using WebRuou.Models;

namespace WebRuou.Controllers
{
    public class CheckoutController : Controller
    {
        private DBRuouEntities db = new DBRuouEntities();

        // GET: Checkout
        public ActionResult Index()
        {
            var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();

            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "Cart");
            }

            // Tính tổng tiền và gán vào ViewBag
            ViewBag.TotalPrice = cart.Sum(i => i.ProductPrice * i.Quantity);

            // Tự động điền email nếu người dùng đã đăng nhập
            ViewBag.Email = User.Identity.IsAuthenticated ? User.Identity.Name : "";

            return View(cart);
        }


        [HttpPost]
        public ActionResult ConfirmOrder(string Email, string FullName, string Phone, string Address, string Note, string PaymentMethod)
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] = "Không thể đặt hàng vì giỏ hàng trống!";
                return RedirectToAction("Index", "Cart");
            }

            // Tạo đơn hàng mới
            Order order = new Order
            {
                UserID = (int?)Session["UserID"],
                TotalAmount = cart.Sum(i => i.ProductPrice * i.Quantity),
                Status = "Đang xử lý",
                OrderDate = DateTime.Now
            };
            db.Orders.Add(order);
            db.SaveChanges();

            // Thêm chi tiết đơn hàng
            foreach (var item in cart)
            {
                db.OrderDetails.Add(new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    Price = item.ProductPrice
                });
            }
            db.SaveChanges();

            // Thêm thông tin giao hàng (chỉ còn Address)
            db.Shippings.Add(new Shipping
            {
                OrderID = order.OrderID,
                Address = Address, // Chỉ giữ lại Address
                Status = "Chưa giao",
                EstimatedDeliveryDate = DateTime.Now.AddDays(3)
            });
            db.SaveChanges();

            // Thêm thông tin thanh toán
            db.Payments.Add(new Payment
            {
                OrderID = order.OrderID,
                PaymentMethod = PaymentMethod,
                Status = PaymentMethod == "Momo" ? "Chờ thanh toán" : "Chưa thanh toán",
                PaymentDate = PaymentMethod == "Momo" ? (DateTime?)DateTime.Now : null
            });
            db.SaveChanges();

            // Xóa giỏ hàng sau khi đặt hàng thành công
            Session["Cart"] = null;
            Session["CartCount"] = 0;

            TempData["Success"] = "Đơn hàng của bạn đã được đặt thành công!";
            return RedirectToAction("Success", new { orderID = order.OrderID });
        }

        public ActionResult MyOrders()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var externalId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(externalId))
            {
                TempData["Error"] = "Bạn cần đăng nhập để xem đơn hàng!";
                return RedirectToAction("Login", "Account");
            }

            var user = db.Users.FirstOrDefault(u => u.ExternalId == externalId);
            if (user == null)
            {
                TempData["Error"] = "Lỗi xác thực người dùng!";
                return RedirectToAction("Login", "Account");
            }

            // Debug kiểm tra
            Console.WriteLine("UserID khi truy vấn đơn hàng: " + user.UserID);

            var orders = db.Orders
                .Where(o => o.UserID == user.UserID)  // ✅ So sánh đúng UserID
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }
        public ActionResult OrderDetail(int id)
        {
            var order = db.Orders
        .Include("OrderDetails.Product") // Load danh sách sản phẩm trong đơn hàng
        .FirstOrDefault(o => o.OrderID == id);

            return View(order);
        }
        public ActionResult Success(int orderID)
        {
            ViewBag.OrderID = orderID;
            return View();
        }
    }
}
