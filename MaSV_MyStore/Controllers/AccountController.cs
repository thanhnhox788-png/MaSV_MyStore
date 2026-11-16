using System;
using System.Linq;
using System.Web.Mvc;
using MaSV_MyStore.Models;
using MaSV_MyStore.Models.ViewModels; // Thêm namespace chứa RegisterVM

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

                // ⭐️ Dùng Reflection gán Balance = 0 (Số dư ví mặc định 0đ khi đăng ký mới)
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

        // 6. Đăng xuất
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}