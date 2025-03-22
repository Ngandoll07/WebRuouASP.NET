using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebRuou.Models;

namespace WebRuou.Controllers
{
    public class IndexController : Controller
    {
        private DBRuouEntities db=new DBRuouEntities();
        // GET: Index
        public ActionResult Index()
        {
            var categories = db.Categories.ToList(); 
            ViewBag.Categories = categories;

            var products = db.Products.ToList();
            ViewBag.Products = products.Take(6).ToList();


            return View();
        }
        public ActionResult AboutUs()
        {
            return View();
        }
        public ActionResult News()
        {
            return View();
        }
    }
}