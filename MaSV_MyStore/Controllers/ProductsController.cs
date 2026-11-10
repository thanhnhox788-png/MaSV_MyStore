using System;
using System.Linq;
using System.Web.Mvc;
using MaSV_MyStore.Models; // Thay MaSV_MyStore bằng namespace thực tế của bạn

namespace MaSV_MyStore.Controllers
{
    public class ProductController : Controller
    {
        private MyStoreEntities1 db = new MyStoreEntities1();

        // 1. GET: Product/Index - Hiển thị danh sách sản phẩm (có hỗ trợ tìm kiếm)
        public ActionResult Index(string searchString)
        {
            var products = db.Products.AsQueryable();

            // Nếu người dùng nhập từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                // Lọc linh hoạt theo tên sản phẩm (hỗ trợ cả ProductName và TenSanPham)
                products = products.Where(p =>
                    (p.GetType().GetProperty("ProductName") != null && p.GetType().GetProperty("ProductName").GetValue(p).ToString().Contains(searchString)) ||
                    (p.GetType().GetProperty("TenSanPham") != null && p.GetType().GetProperty("TenSanPham").GetValue(p).ToString().Contains(searchString))
                );
            }

            return View(products.ToList());
        }

        // 2. GET: Product/Details/5 - Xem chi tiết sản phẩm
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            // Tìm sản phẩm theo ID (hỗ trợ cả ProductID và MaSanPham nếu cần)
            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound("Không tìm thấy sản phẩm này.");
            }

            return View(product);
        }

        // 3. (Tùy chọn) GET: Product/ListByCategory/5 - Lọc sản phẩm theo danh mục
        public ActionResult ListByCategory(int? categoryId)
        {
            if (categoryId == null)
            {
                return RedirectToAction("Index");
            }

            var products = db.Products.ToList().Where(p => {
                var catProp = p.GetType().GetProperty("CategoryID") ?? p.GetType().GetProperty("MaDanhMuc");
                if (catProp != null)
                {
                    int val = Convert.ToInt32(catProp.GetValue(p));
                    return val == categoryId.Value;
                }
                return false;
            }).ToList();

            return View("Index", products);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}