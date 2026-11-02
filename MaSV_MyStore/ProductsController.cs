using System.IO;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using MaSV_MyStore.Models; // Đổi MaSV thành mã sinh viên của bạn

namespace MaSV_MyStore.Controllers // Hoặc namespace Areas/Admin/Controllers tương ứng
{
    public class ProductsController : Controller
    {
        private MyStoreEntities1 db = new MyStoreEntities1();

        // GET: Products/Create
        public ActionResult Create()
        {
            // Đổ dữ liệu vào DropDownList danh mục
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice,ProductImage,UploadImg")] Product product)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem người dùng có chọn file ảnh hay không
                if (product.UploadImg != null && product.UploadImg.ContentLength > 0)
                {
                    // Lấy tên file gốc
                    string fileName = Path.GetFileName(product.UploadImg.FileName);

                    // Đường dẫn lưu trữ vật lý trên server (thư mục ~/Content/images/)
                    string path = Path.Combine(Server.MapPath("~/Content/images/"), fileName);

                    // Lưu file vào thư mục
                    product.UploadImg.SaveAs(path);

                    // Lưu đường dẫn tương đối vào database
                    product.ProductImage = "~/Content/images/" + fileName;
                }
                else
                {
                    // Nếu không chọn ảnh, gán ảnh mặc định
                    product.ProductImage = "~/Content/images/default.png";
                }

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Nếu dữ liệu không hợp lệ, đổ lại danh sách cho DropDownList
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }
    }
}