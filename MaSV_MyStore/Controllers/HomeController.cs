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

        public ActionResult Index()
        {
            try
            {
                // Lấy danh sách sản phẩm từ cơ sở dữ liệu
                var products = db.Products.ToList();

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

        // 🌟 BỔ SUNG HÀM DETAILS NÀY ĐỂ XEM CHI TIẾT SẢN PHẨM
        public ActionResult Details(int id)
        {
            try
            {
                // Tìm sản phẩm theo ID trong cơ sở dữ liệu
                var product = db.Products.FirstOrDefault(p => p.ProductID == id);
                // Lưu ý: Nếu cột khóa chính trong CSDL của bạn tên là MaSP hoặc ID, 
                // hãy đổi p.ProductID thành p.MaSP hoặc p.ID tương ứng.

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

        public ActionResult About() => View();
        public ActionResult Contact() => View();
    }
}