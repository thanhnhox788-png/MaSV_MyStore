using System.Linq;
using System.Web.Mvc;
using MaSV_MyStore.Models;
using MaSV_MyStore.Models.ViewModel;
using PagedList;

namespace MaSV_MyStore.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities1 db = new MyStoreEntities1();

        // GET: Home/Index
        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();
            var products = db.Products.AsQueryable();

            // Tìm kiếm sản phẩm theo từ khóa
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                                              p.ProductDescription.Contains(searchTerm) ||
                                              p.Category.CategoryName.Contains(searchTerm));
            }

            int pageSize = 6; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1);

            // Lấy danh sách ID top 10 sản phẩm bán chạy từ bảng OrderDetails
            var topProductIDs = db.OrderDetails
                .GroupBy(od => od.ProductID)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(10)
                .ToList();

            // Gán danh sách sản phẩm nổi bật vào ViewModel
            model.FeaturedProducts = products
                .Where(p => topProductIDs.Contains(p.ProductID))
                .ToList();

            // Lấy danh sách sản phẩm mới và phân trang
            model.NewProducts = products
                .OrderByDescending(p => p.ProductID)
                .ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}