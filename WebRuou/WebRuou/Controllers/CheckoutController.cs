using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
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

            if (PaymentMethod == "PayPal")
            {
                Session["CheckoutInfo"] = new Dictionary<string, object>
                {
                    { "Email", Email },
                    { "FullName", FullName },
                    { "Phone", Phone },
                    { "Address", Address },
                    { "Note", Note }
                };
                return RedirectToAction("PayWithPayPal");
            }

            // Tạo đơn hàng mới
            var order = new Models.Order
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

            db.Payments.Add(new Models.Payment
            {
                OrderID = order.OrderID,
                PaymentMethod = PaymentMethod,
                Status = "Chưa thanh toán",
                PaymentDate = null
            });
            db.SaveChanges();

            Session["Cart"] = null;
            TempData["Success"] = "Đơn hàng đã được đặt!";
            return RedirectToAction("Success", new { orderID = order.OrderID });
        }
        public ActionResult PayWithPayPal()
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null || cart.Count == 0)
                return RedirectToAction("Index", "Cart");

            var apiContext = GetAPIContext();
            var itemList = new ItemList()
            {
                items = cart.Select(i => new Item
                {
                    name = i.ProductName,
                    currency = "USD",
                    price = i.ProductPrice.ToString(),
                    quantity = i.Quantity.ToString(),
                    sku = i.ProductID.ToString()
                }).ToList()
            };

            var totalAmount = cart.Sum(i => i.ProductPrice * i.Quantity);
            var payment = new PayPal.Api.Payment()
            {
                intent = "sale",
                payer = new Payer() { payment_method = "paypal" },
                transactions = new List<Transaction> { new Transaction
                {
                    amount = new Amount() { currency = "USD", total = totalAmount.ToString() },
                    item_list = itemList,
                    description = "Thanh toán WineHorse"
                }},
                redirect_urls = new RedirectUrls
                {
                    return_url = Url.Action("PaymentSuccess", "Checkout", null, Request.Url.Scheme),
                    cancel_url = Url.Action("PaymentCancel", "Checkout", null, Request.Url.Scheme)
                }
            };

            var createdPayment = payment.Create(apiContext);
            Session["PaymentID"] = createdPayment.id;
            return Redirect(createdPayment.links.FirstOrDefault(l => l.rel == "approval_url")?.href);
        }

        public ActionResult PaymentSuccess(string paymentId, string PayerID)
        {
            var apiContext = GetAPIContext();
            var payment = new PayPal.Api.Payment() { id = paymentId };
            var executedPayment = payment.Execute(apiContext, new PaymentExecution() { payer_id = PayerID });

            if (executedPayment.state.ToLower() != "approved")
            {
                TempData["Error"] = "Thanh toán thất bại!";
                return RedirectToAction("Index", "Cart");
            }

            var cart = Session["Cart"] as List<CartItem>;
            var order = new Models.Order
            {
                UserID = (int?)Session["UserID"],
                TotalAmount = cart.Sum(i => i.ProductPrice * i.Quantity),
                Status = "Đã thanh toán",
                OrderDate = DateTime.Now
            };
            db.Orders.Add(order);
            db.SaveChanges();

            db.Payments.Add(new Models.Payment
            {
                OrderID = order.OrderID,
                PaymentMethod = "PayPal",
                Status = "Đã thanh toán",
                PaymentDate = DateTime.Now
            });
            db.SaveChanges();

            Session["Cart"] = null;
            TempData["Success"] = "Thanh toán thành công!";
            return RedirectToAction("Success", new { orderID = order.OrderID });
        }

        public ActionResult PaymentCancel()
        {
            TempData["Error"] = "Bạn đã hủy thanh toán.";
            return RedirectToAction("Index", "Cart");
        }
        private APIContext GetAPIContext()
        {
            var config = new Dictionary<string, string>
            {
                { "mode", ConfigurationManager.AppSettings["PayPalMode"] }
            };
            var accessToken = new OAuthTokenCredential(
                ConfigurationManager.AppSettings["PayPalClientId"],
                ConfigurationManager.AppSettings["PayPalClientSecret"],
                config).GetAccessToken();
            return new APIContext(accessToken) { Config = config };
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
