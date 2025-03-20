using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using WebRuou.Models;

namespace WebRuou.Controllers
{
    public class UserController : Controller
    {
        private DBRuouEntities db=new DBRuouEntities();
        // GET: User
        public ActionResult Index()
        {
            return View();
        }
        
    }
}