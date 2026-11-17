using System;
using System.Linq;
using System.Web.Mvc;
using MaSV_MyStore.Models;
using MaSV_MyStore.Models.ViewModel;

namespace MaSV_MyStore.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities1 db = new MyStoreEntities1();

        // 🌟 ĐÃ TÍCH HỢP TÌM KIẾM DỰA TRÊN CODE GỐC CỦA BẠN
        public ActionResult Index(string searchString)
        {
            try
            {
                // Lấy danh sách sản phẩm dưới dạng AsQueryable để hỗ trợ lọc tìm kiếm
                var productsQuery = db.Products.AsQueryable();

                // Nếu người dùng nhập từ khóa tìm kiếm
                if (!string.IsNullOrEmpty(searchString))
                {
                    productsQuery = productsQuery.Where(p => p.ProductName.Contains(searchString));
                }

                var products = productsQuery.ToList();

                // Nếu bạn dùng ViewModel (HomeProductVM)
                var viewModel = new HomeProductVM();

                // Gán vào thuộc tính chứa sản phẩm (hỗ trợ tự động các tên phổ biến)
                var prop = viewModel.GetType().GetProperty("Products")
                        ?? viewModel.GetType().GetProperty("ListProduct")
                        ?? viewModel.GetType().GetProperty("DanhSachSanPham");

                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(viewModel, products);
                    return View(viewModel);
                }

                // Nếu không dùng ViewModel mà truyền trực tiếp List<Product>
                return View(products);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi tải sản phẩm: " + ex.Message);
                return View(new HomeProductVM());
            }
        }

        // 🌟 HÀM DETAILS ĐÃ ĐƯỢC TỐI ƯU CHỐNG LỆCH TÊN KHÓA CHÍNH (ID)
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            try
            {
                // Entity Framework hỗ trợ tìm trực tiếp theo khóa chính của bảng Product
                var product = db.Products.Find(id);

                if (product == null)
                {
                    return HttpNotFound("Không tìm thấy sản phẩm này.");
                }

                return View(product);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi xem chi tiết sản phẩm: " + ex.Message);
                return RedirectToAction("Index");
            }
        }

        // 🌟 BỔ SUNG ACTION XỬ LÝ NÚT "MUA NGAY" TRÁNH LỖI 404 /Order/Checkout
        [HttpGet]
        public ActionResult BuyNow(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            try
            {
                var product = db.Products.Find(id);
                if (product == null)
                {
                    return HttpNotFound("Không tìm thấy sản phẩm.");
                }

                // Chuyển hướng sang trang chi tiết hoặc trang giỏ hàng/thanh toán tương ứng
                return RedirectToAction("Details", new { id = id });
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult About() => View();
        public ActionResult Contact() => View();
    }
}