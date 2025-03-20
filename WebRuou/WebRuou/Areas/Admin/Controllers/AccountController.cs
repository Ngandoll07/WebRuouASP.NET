using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebRuou.Models;

namespace WebRuou.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private DBRuouEntities db = new DBRuouEntities();
        // GET: Admin/Account
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Logout()
        {
            // Xóa session
            Session.Clear();
            Session.Abandon();

            // Đăng xuất khỏi OWIN Authentication
            HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            // Điều hướng về trang login
            return RedirectToAction("Index", "Home");
        }


    }
}