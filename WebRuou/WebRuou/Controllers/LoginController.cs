using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebRuou.Models;

namespace WebRuou.Controllers
{
    public class LoginController : Controller
    {
        public DBRuouEntities db=new DBRuouEntities();
        // GET: Login

        // Trang đăng nhập
        public ActionResult Index()
        {
            return View();
        }

        

        // Xử lý đăng nhập bằng Google
        public ActionResult LoginGoogle()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("ExternalLoginCallback", "Login", null, Request.Url.Scheme) };
            HttpContext.GetOwinContext().Authentication.Challenge(properties, "Google");
            return new HttpUnauthorizedResult();
        }       

        // Xử lý đăng nhập bằng Facebook
        public ActionResult LoginFacebook()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("ExternalLoginCallback", "Login") };
            HttpContext.GetOwinContext().Authentication.Challenge(properties, "Facebook");
            return new HttpUnauthorizedResult();
        }

        // Xử lý callback sau khi đăng nhập thành công
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await HttpContext.GetOwinContext().Authentication.GetExternalLoginInfoAsync();

            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            var nameIdentifier = loginInfo.ExternalIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(nameIdentifier))
            {
                ModelState.AddModelError("", "Lỗi: Thiếu NameIdentifier. Vui lòng đăng xuất và thử lại!");
                return View("Login");
            }

            // Kiểm tra trong database xem người dùng đã tồn tại chưa
            var user = db.Users.SingleOrDefault(u => u.ExternalId == nameIdentifier);

            if (user == null)
            {
                user = new User
                {
                    ExternalId = nameIdentifier,
                    Email = loginInfo.Email,
                    FullName = loginInfo.DefaultUserName,
                    IsActive = true,
                    Role = "User", // Mặc định User
                    CreatedAt = DateTime.Now
                };

                var existingUser = db.Users.FirstOrDefault(u => u.Email == user.Email);
                if (existingUser == null)
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                }
                else
                {
                    // Nếu user đã tồn tại, cập nhật ExternalId nếu cần
                    existingUser.ExternalId = user.ExternalId;
                    db.SaveChanges();
                }

                db.SaveChanges();
            }

            // Tạo Claims Identity
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.ExternalId),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role) // Phân quyền từ DB
            };

            var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            HttpContext.GetOwinContext().Authentication.SignIn(authProperties, identity);

            // Lưu vào session
            Session["UserID"] = user.UserID;
            Session["UserName"] = user.FullName;
            Session["UserEmail"] = user.Email;
            Session["UserRole"] = user.Role;

            return user.Role == "Admin" ? RedirectToAction("Index", "Home", new { area = "Admin" }) : RedirectToAction("Index", "Index");
        }

        public ActionResult EditProfile()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var externalIdClaim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(externalIdClaim))
            {
                TempData["ErrorMessage"] = "Không lấy được thông tin người dùng. Vui lòng đăng nhập lại.";
                return RedirectToAction("Index", "Home");
            }

            var user = db.Users.SingleOrDefault(u => u.ExternalId == externalIdClaim);
            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new UserEditViewModel
            {
                ExternalId = user.ExternalId,  // Đổi UserID thành ExternalId
                FullName = user.FullName,
                Email = user.Email,
                Password=user.PasswordHash,
                Phone = user.Phone,
                Address = user.Address
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(UserEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.SingleOrDefault(u => u.ExternalId == model.ExternalId);
                if (user == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật thông tin
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.PasswordHash = model.Password;
                user.Phone = model.Phone;
                user.Address = model.Address;

                db.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("EditProfile");
            }

            return View(model);
        }


        /* public ActionResult ExternalLoginCallback(string returnUrl)
         {
             var loginInfo = HttpContext.GetOwinContext().Authentication.GetExternalLoginInfo();
             if (loginInfo == null || string.IsNullOrEmpty(loginInfo.Email))
             {
                 return RedirectToAction("Login", "Account");
             }

             using (var db = new DBRuouEntities())
             {
                 System.Diagnostics.Debug.WriteLine($"Email từ Google/Facebook: {loginInfo.Email}");

                 var adminUser = db.AdminUsers.SingleOrDefault(a => a.Username == loginInfo.Email);

                 if (adminUser != null)
                 {
                     var identity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
                     identity.AddClaim(new Claim(ClaimTypes.Name, adminUser.Username));
                     identity.AddClaim(new Claim(ClaimTypes.Email, adminUser.Username));
                     identity.AddClaim(new Claim(ClaimTypes.Role, adminUser.Role ?? "User"));

                     HttpContext.GetOwinContext().Authentication.SignIn(new AuthenticationProperties(), identity);
                     return RedirectToAction("Index", "Home", new { area = "Admin" });
                 }

                 var user = db.Users.SingleOrDefault(u => u.Email == loginInfo.Email);

                 if (user == null)
                 {
                     var newUser = new User
                     {
                         FullName = loginInfo.DefaultUserName ?? loginInfo.Email,
                         Email = loginInfo.Email,
                         CreatedAt = DateTime.Now,
                         IsActive = true
                     };
                     db.Users.Add(newUser);

                     var newAdminUser = new AdminUser
                     {
                         Username = loginInfo.Email,
                         PasswordHash = "",
                         Role = "User"
                     };
                     db.AdminUsers.Add(newAdminUser);

                     try
                     {
                         db.SaveChanges();
                     }
                     catch (DbEntityValidationException ex)
                     {
                         foreach (var validationErrors in ex.EntityValidationErrors)
                         {
                             foreach (var validationError in validationErrors.ValidationErrors)
                             {
                                 System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                             }
                         }
                         throw;
                     }
                 }

                 var userIdentity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
                 userIdentity.AddClaim(new Claim(ClaimTypes.Name, user?.FullName ?? loginInfo.Email));
                 userIdentity.AddClaim(new Claim(ClaimTypes.Email, loginInfo.Email));
                 userIdentity.AddClaim(new Claim(ClaimTypes.Role, "User"));

                 HttpContext.GetOwinContext().Authentication.SignIn(new AuthenticationProperties(), userIdentity);

                 return RedirectToAction("Index", "Index");
             }
         }
 */
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Login", new { ReturnUrl = returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            HttpContext.GetOwinContext().Authentication.Challenge(properties, provider);
            return new HttpUnauthorizedResult();
        }
        public ActionResult LoginGoogleCallback()
        {
            // Lấy thông tin email từ Google sau khi đăng nhập thành công
            string userEmail = GetUserEmailFromGoogle(); // Hàm lấy email từ Google

            // Tìm UserID trong database dựa vào email
            var user = db.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user != null)
            {
                int userId = user.UserID; // Lấy UserID từ DB

                // Lưu UserID vào Session
                Session["UserID"] = userId;

                // Kiểm tra giỏ hàng trong Session và lưu vào DB
                if (Session["Cart"] != null)
                {
                    var cartItems = (List<Cart>)Session["Cart"];
                    foreach (var item in cartItems)
                    {
                        var cartItem = new Cart
                        {
                            UserID = userId,
                            ProductID = item.ProductID,
                            Quantity = item.Quantity
                        };
                        db.Carts.Add(cartItem);
                    }
                    db.SaveChanges();
                    Session["Cart"] = null; // Xóa session để tránh thêm lại
                }
            }
            else
            {
                // Nếu không tìm thấy user, có thể điều hướng về trang đăng ký hoặc báo lỗi
                return RedirectToAction("Register", "Account");
            }

            return RedirectToAction("Index", "Home");
        }
        private string GetUserEmailFromGoogle()
        {
            var auth = HttpContext.GetOwinContext().Authentication;
            var loginInfo = auth.GetExternalLoginInfo();

            if (loginInfo != null)
            {
                return loginInfo.Email;
            }
            return null;
        }

        public ActionResult CheckLogin(string email, string password)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password);
            if (user != null)
            {
                Session["UserID"] = user.UserID;

                if (Session["Cart"] != null)
                {
                    var cartItems = (List<Cart>)Session["Cart"];
                    foreach (var item in cartItems)
                    {
                        db.Carts.Add(new Cart
                        {
                            UserID = user.UserID,
                            ProductID = item.ProductID,
                            Quantity = item.Quantity
                        });
                    }
                    db.SaveChanges();
                    Session["Cart"] = null;
                }
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Sai email hoặc mật khẩu!";
            return View();
        }

        // Đăng xuất
        public ActionResult Logout()
        {
            // Xóa session giỏ hàng
            Session.Remove("Cart"); // "Cart" là key bạn dùng để lưu giỏ hàng

            // Đăng xuất người dùng
            HttpContext.GetOwinContext().Authentication.SignOut();

            return RedirectToAction("Index", "Index");
        }
    }
}