using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebRuou.Models;

namespace WebRuou.Areas.Admin.Controllers
{

    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private DBRuouEntities db=new DBRuouEntities();
        // GET: Admin/Home
        public ActionResult Index()
        {
            // Tính tổng doanh thu từ các đơn hàng hoàn thành
            decimal totalRevenue = db.Orders
                .Where(o => o.Status == "Hoàn thành")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            // Tính tổng số sản phẩm đã bán
            int totalProductsSold = db.OrderDetails.Sum(od => (int?)od.Quantity) ?? 0;

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalProductsSold = totalProductsSold;

            return View();
        }

        // API: Trả về doanh thu theo tháng
        public JsonResult GetRevenueData()
        {
            var revenueData = db.Orders
                .Where(o => o.Status == "Hoàn thành")
                .GroupBy(o => new {
                    Year = (o.OrderDate ?? DateTime.MinValue).Year,
                    Month = (o.OrderDate ?? DateTime.MinValue).Month
                })
                .OrderBy(g => g.Key.Year)  // Sắp xếp theo năm
                .ThenBy(g => g.Key.Month)  // Sau đó sắp xếp theo tháng
                .Select(g => new
                {
                    Month = $"{g.Key.Month}/{g.Key.Year}",  // Định dạng tháng/năm
                    TotalRevenue = g.Sum(o => o.TotalAmount)
                })
                .ToList();

            return Json(revenueData, JsonRequestBehavior.AllowGet);
        }

        // API: Trả về số lượng sản phẩm đã bán theo loại
        public JsonResult GetProductSalesData()
        {
            var productSales = db.OrderDetails
                .GroupBy(od => od.Product.Name)
                .Select(g => new
                {
                    ProductName = g.Key,
                    TotalSold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(g => g.TotalSold)
                .ToList();

            return Json(productSales, JsonRequestBehavior.AllowGet);
        }
    }
}