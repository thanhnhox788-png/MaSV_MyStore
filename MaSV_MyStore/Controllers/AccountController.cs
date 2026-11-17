using System;
using System.Linq;
using System.Web.Mvc;
using MaSV_MyStore.Models;
using MaSV_MyStore.Models.ViewModels; // Namespace chứa RegisterVM và OrderDetailVM

namespace MaSV_MyStore.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities1 db = new MyStoreEntities1();

        // 1. GET: Đăng nhập
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // 2. POST: Xử lý Đăng nhập
        [HttpPost]
        public ActionResult Login(Customer model)
        {
            var user = db.Customers.FirstOrDefault(x => x.Username == model.Username && x.Password == model.Password);

            if (user != null)
            {
                // Lưu thông tin vào Session
                Session["Username"] = user.Username;
                Session["FullName"] = user.CustomerName;
                Session["Customer"] = user;

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
            return View(model);
        }

        // 3. GET: Đăng ký
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // 4. POST: Xử lý Đăng ký (Dùng RegisterVM khớp với giao diện View)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên đăng nhập đã tồn tại chưa
                var existingUser = db.Customers.FirstOrDefault(x => x.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại.");
                    return View(model);
                }

                // Chuyển đổi dữ liệu từ RegisterVM sang Customer để lưu vào Database
                var newCustomer = new Customer
                {
                    Username = model.Username,
                    Password = model.Password,
                    CustomerName = model.FullName,
                    Phone = model.CountryCode + model.Phone, // Gộp mã quốc gia và số điện thoại
                    Email = model.Email,
                    Address = model.Address
                };

                // Dùng Reflection gán Balance = 0 (Số dư ví mặc định 0đ khi đăng ký mới)
                try
                {
                    var balanceProp = newCustomer.GetType().GetProperty("Balance");
                    if (balanceProp != null)
                    {
                        balanceProp.SetValue(newCustomer, Convert.ChangeType(0m, balanceProp.PropertyType));
                    }
                }
                catch { }

                db.Customers.Add(newCustomer);
                db.SaveChanges();

                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        // 5. Trang thông tin tài khoản
        [HttpGet]
        public ActionResult AccountProfile()
        {
            string username = Session["Username"]?.ToString();

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.FirstOrDefault(x => x.Username == username);

            if (customer == null)
            {
                return HttpNotFound();
            }

            return View(customer);
        }

        // 6. Xem lịch sử "Đơn hàng của tôi" của khách hàng đang đăng nhập
        [HttpGet]
        public ActionResult MyOrders()
        {
            // Lấy thông tin khách hàng từ Session
            var customerSession = Session["Customer"] as Customer;
            string username = Session["Username"]?.ToString();

            if (customerSession == null && string.IsNullOrEmpty(username))
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem lịch sử đơn hàng!";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Xác định mã khách hàng (CustomerID)
                int customerId = 0;
                if (customerSession != null)
                {
                    customerId = customerSession.CustomerID;
                }
                else
                {
                    var cus = db.Customers.FirstOrDefault(x => x.Username == username);
                    if (cus != null) { customerId = cus.CustomerID; }
                }

                // Truy vấn danh sách đơn hàng của riêng khách hàng này (mới nhất lên đầu)
                var myOrders = db.Orders
                                 .Where(o => o.CustomerID == customerId)
                                 .OrderByDescending(o => o.OrderID)
                                 .ToList();

                return View(myOrders);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi xem MyOrders: " + ex.Message);
                return View(Enumerable.Empty<Order>());
            }
        }

        // 7. Xem chi tiết đơn hàng dành riêng cho khách hàng
        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("MyOrders");
            }

            try
            {
                var order = db.Orders.Find(id);
                if (order == null)
                {
                    return HttpNotFound();
                }

                // Lấy danh sách chi tiết đơn hàng theo OrderID
                var orderDetails = db.OrderDetails.Where(x => x.OrderID == id).ToList();

                var displayDetails = orderDetails.Select(od => {
                    var product = db.Products.FirstOrDefault(p => p.ProductID == od.ProductID);

                    string pName = product != null ? product.ProductName : "Sản phẩm";
                    string rawImg = product != null ? product.ProductImage : "";

                    // Lấy đơn giá và số lượng trực tiếp từ CSDL
                    decimal price = od.UnitPrice;
                    int qty = od.Quantity;

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

                    return new OrderDetailVM
                    {
                        ProductID = od.ProductID,
                        Quantity = qty,
                        Price = price,
                        ProductName = pName,
                        ProductImage = pImage
                    };
                }).ToList();

                ViewBag.OrderDetails = displayDetails;

                return View(order);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi chi tiết đơn hàng khách: " + ex.Message);
                return RedirectToAction("MyOrders");
            }
        }

        // 8. Đăng xuất
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}