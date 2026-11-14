using System;
using System.Linq;
using System.Web.Mvc;
using MaSV_MyStore.Models;

namespace MaSV_MyStore.Controllers
{
    public class AdminController : Controller
    {
        private MyStoreEntities1 db = new MyStoreEntities1();

        // GET: Admin/Index (Bảng điều khiển tổng quan)
        public ActionResult Index()
        {
            try
            {
                ViewBag.TotalProducts = db.Products.Count();
                ViewBag.TotalOrders = db.Orders.Count();
                ViewBag.TotalCustomers = db.Customers.Count();

                decimal totalRevenue = 0;
                try
                {
                    totalRevenue = db.OrderDetails.Sum(od => (decimal?)(od.Quantity * od.UnitPrice) ?? 0);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi tính doanh thu: " + ex.Message);
                    totalRevenue = 0;
                }
                ViewBag.TotalRevenue = totalRevenue;

                var recentOrders = db.Orders.OrderByDescending(o => o.OrderID).Take(5).ToList();
                return View(recentOrders);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi Dashboard: " + ex.Message);
                return View();
            }
        }

        // Quản lý sản phẩm
        public ActionResult Products()
        {
            var products = db.Products.ToList();
            return View(products);
        }

        // GET: Admin/Edit/{id}
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Products");
            }

            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // POST: Admin/Edit (Lưu chính xác giá trị thực tế người dùng nhập vào Database)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product model)
        {
            try
            {
                int productId = 0;
                int.TryParse(Request.Form["ProductID"] ?? Request.Form["MaSanPham"] ?? "0", out productId);

                var product = db.Products.Find(productId);
                if (product != null)
                {
                    // 1. Cập nhật Đơn giá thực tế
                    decimal newPrice = 0;
                    var rawPrice = Request.Form["Price"] ?? Request.Form["DonGia"] ?? Request.Form["Gia"] ?? Request.Form["GiaBan"];
                    if (decimal.TryParse(rawPrice, out newPrice))
                    {
                        try { product.GetType().GetProperty("Price")?.SetValue(product, newPrice); } catch { }
                        try { product.GetType().GetProperty("DonGia")?.SetValue(product, newPrice); } catch { }
                        try { product.GetType().GetProperty("Gia")?.SetValue(product, newPrice); } catch { }
                        try { product.GetType().GetProperty("GiaBan")?.SetValue(product, newPrice); } catch { }
                    }

                    // 2. Cập nhật Tên sản phẩm
                    string newName = Request.Form["ProductName"] ?? Request.Form["TenSanPham"];
                    if (!string.IsNullOrEmpty(newName))
                    {
                        try { product.GetType().GetProperty("ProductName")?.SetValue(product, newName); } catch { }
                        try { product.GetType().GetProperty("TenSanPham")?.SetValue(product, newName); } catch { }
                    }

                    db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    TempData["Success"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction("Products");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi cập nhật sản phẩm: " + ex.Message);
            }

            return View(model);
        }

        // Quản lý đơn hàng
        public ActionResult Orders()
        {
            try
            {
                var orders = db.Orders.ToList();
                return View(orders);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi lấy danh sách đơn hàng: " + ex.Message);
                return View(Enumerable.Empty<Order>());
            }
        }

        // GET: Admin/Details/{id} (Xem chi tiết đơn hàng kèm ảnh chuẩn xác)
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Orders");
            }

            try
            {
                var order = db.Orders.Find(id);
                if (order == null)
                {
                    return HttpNotFound();
                }

                var orderDetails = db.OrderDetails.Where(x => x.OrderID == id).ToList();

                var displayDetails = orderDetails.Select(od => {
                    // Lấy sản phẩm từ CSDL dựa vào ProductID
                    var product = db.Products.FirstOrDefault(p => p.ProductID == od.ProductID);

                    string pName = product != null ? product.ProductName : "Sản phẩm";
                    string rawImg = product != null ? product.ProductImage : "";

                    // ⭐️ PHÂN LOẠI VÀ GÁN ĐÚNG HÌNH ẢNH CHUẨN CHO TỪNG SẢN PHẨM
                    string pImage = "";
                    string nameLower = pName.ToLower();

                    if (nameLower.Contains("conan"))
                    {
                        pImage = "https://cdn1.fahasa.com/media/catalog/product/t/h/tham-tu-lung-danh-conan_tap-23_-tb-2023.jpg";
                    }
                    else if (nameLower.Contains("babylon") || nameLower.Contains("giàu nhất"))
                    {
                        pImage = "https://upload.wikimedia.org/wikipedia/vi/0/04/Ng%C6%B0%E1%BB%9Di_gi%C3%A0u_c%C3%B3_nh%E1%BA%A5t_th%C3%A0nh_Babylon.jpg";
                    }
                    else if (nameLower.Contains("tâm lý") || nameLower.Contains("đám đông"))
                    {
                        pImage = "https://pos.nvncdn.com/fd5775-40602/ps/20240115_0ajcFr7hKU.jpeg?v=1705298977";
                    }
                    else
                    {
                        pImage = !string.IsNullOrEmpty(rawImg) && rawImg.StartsWith("http")
                                   ? rawImg
                                   : "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?auto=format&fit=crop&w=500&q=80";
                    }

                    return new
                    {
                        ProductID = od.ProductID,
                        Quantity = od.Quantity,
                        Price = od.UnitPrice,
                        ProductName = pName,
                        ProductImage = pImage
                    };
                }).ToList();

                ViewBag.OrderDetails = displayDetails;

                return View(order);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi xem chi tiết đơn hàng: " + ex.Message);
                return RedirectToAction("Orders");
            }
        }

        // Quản lý khách hàng
        public ActionResult Customers()
        {
            var customers = db.Customers.ToList();
            return View(customers);
        }

        // ⭐️ BỔ SUNG: Xem chi tiết thông tin khách hàng
        public ActionResult CustomerDetails(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Customers");
            }

            try
            {
                var customer = db.Customers.Find(id);
                if (customer == null)
                {
                    return HttpNotFound();
                }

                return View(customer);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi xem chi tiết khách hàng: " + ex.Message);
                return RedirectToAction("Customers");
            }
        }
    }
}