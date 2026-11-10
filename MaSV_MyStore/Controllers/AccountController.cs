using MaSV_MyStore.Models;
using MaSV_MyStore.Models.ViewModels;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MaSV_MyStore.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities1 db = new MyStoreEntities1();

        // ====================== REGISTER ======================
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra username đã tồn tại chưa
                var existingUser = db.Users.FirstOrDefault(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại!");
                    return View(model);
                }

                // 1. Tạo User
                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password,
                    Role = "Customer"
                };
                db.Users.Add(user);

                // 2. Tạo Customer
                var customer = new Customer
                {
                    Username = model.Username,
                    CustomerName = model.FullName,   // ← Tên khách hàng lúc đăng ký
                    Phone = model.Phone,
                    Email = model.Email,
                    Address = model.Address,
                    Password = model.Password
                };
                db.Customers.Add(customer);

                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Login");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.PropertyName + ": " + x.ErrorMessage);

                    string fullError = string.Join(" | ", errorMessages);
                    ModelState.AddModelError("", "Lỗi lưu dữ liệu: " + fullError);
                    return View(model);
                }
            }

            return View(model);
        }

        // ====================== LOGIN ======================
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);

                if (user != null)
                {
                    FormsAuthentication.SetAuthCookie(user.Username, false);
                    Session["Username"] = user.Username;
                    Session["Role"] = user.Role;

                    // ⭐ LẤY THÊM TÊN KHÁCH HÀNG TỪ BẢNG CUSTOMER ĐỂ HIỂN THỊ LÊN WEB ⭐
                    var customer = db.Customers.FirstOrDefault(c => c.Username == user.Username);
                    if (customer != null)
                    {
                        Session["FullName"] = customer.CustomerName; // Lưu tên đầy đủ vào Session
                    }
                    else
                    {
                        Session["FullName"] = user.Username; // Phòng hờ nếu không tìm thấy bảng Customer thì lấy tạm Username
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng!");
            }

            return View(model);
        }

        // ====================== LOGOUT ======================
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home");
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