using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using WebRuou.Models;
using System.Diagnostics;

namespace WebRuou.Controllers
{
    public class ProductController : Controller
    {
        private DBRuouEntities db = new DBRuouEntities();

        public ActionResult Index(int? categoryId)
        {
            // Lấy danh sách danh mục
            var categories = db.Categories.Include(c => c.Products).ToList();
            ViewBag.Categories = categories;

            // Lọc sản phẩm theo danh mục (nếu có)
            var products = db.Products.Include(p => p.Category);

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryID == categoryId.Value);
            }

            ViewBag.Products = products.ToList();
            ViewBag.SelectedCategory = categoryId; // Lưu categoryId để highlight danh mục đang chọn

            return View();
        }

        public ActionResult PartialSameProduct(int categoryId, int currentProductId)
        {
            var relatedProducts = db.Products
            .Where(p => p.CategoryID == categoryId && p.ProductID != currentProductId) // Loại bỏ sản phẩm hiện tại
            .ToList();

            if (!relatedProducts.Any())
            {
                Debug.WriteLine("Không có sản phẩm cùng loại.");
            }
            else
            {
                Debug.WriteLine($"Tìm thấy {relatedProducts.Count} sản phẩm cùng loại.");
            }

            return PartialView(relatedProducts);
        }

        public ActionResult ProductDetail(int id)
        {
            // Retrieve the product by ID
            var product = db.Products.Find(id);

            if (product == null)
            {
                return HttpNotFound(); // Return 404 if product not found
            }

            return View(product); // Pass the product to the view
        }
    }
}


