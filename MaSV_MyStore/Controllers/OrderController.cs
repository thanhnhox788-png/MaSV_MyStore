using System;
using System.Linq;
using System.Web.Mvc;
using MaSV_MyStore.Models;
using MaSV_MyStore.Models.ViewModels;

namespace MaSV_MyStore.Controllers
{
    public class OrderController : Controller
    {
        private CartService cartService = new CartService();

        // 1. GET: Order/Checkout - Hiển thị trang thanh toán
        public ActionResult Checkout()
        {
            var cart = cartService.GetCart();
            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "Cart");
            }
            return View(cart);
        }

        // 2. POST: Order/Checkout - Xử lý lưu đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(string customerName, string phone, string address, string paymentMethod)
        {
            var cart = cartService.GetCart();
            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "Cart");
            }

            // Tính tổng tiền giỏ hàng an toàn
            decimal totalAmount = cart.Items.Sum(x => x.Quantity * x.ProductPrice);

            using (var db = new MyStoreEntities1())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        Customer customer = null;

                        // Lấy thông tin tài khoản đang đăng nhập từ Session
                        var sessionUser = Session["Username"] ?? Session["Account"];

                        if (sessionUser != null)
                        {
                            string usernameStr = sessionUser.ToString();
                            // Tìm khách hàng trong database theo Username
                            customer = db.Customers.FirstOrDefault(c => c.Username == usernameStr);
                        }

                        // Nếu khách vãng lai chưa đăng nhập -> Tạo mới tài khoản khách vãng lai
                        if (customer == null)
                        {
                            customer = new Customer();
                            customer.CustomerName = customerName;
                            customer.Phone = phone;
                            customer.CustomerAddress = address;

                            string uniqueKey = "KH_" + DateTime.Now.Ticks.ToString().Substring(10);
                            customer.Username = uniqueKey;
                            customer.Email = uniqueKey + "@gmail.com";
                            customer.Password = "123456";

                            // Gán số dư bằng 0 nếu có thuộc tính Balance
                            customer.GetType().GetProperty("Balance")?.SetValue(customer, 0m);

                            db.Customers.Add(customer);
                            db.SaveChanges(); // Lưu để sinh CustomerID
                        }

                        // Lấy CustomerID linh hoạt theo tên property có sẵn
                        int customerId = (int)(customer.GetType().GetProperty("CustomerID")?.GetValue(customer)
                                                ?? customer.GetType().GetProperty("MaKH")?.GetValue(customer) ?? 1);

                        // ⭐️ XỬ LÝ THANH TOÁN BẰNG SỐ DƯ TÀI KHOẢN TRÊN WEB (WebBalance)
                        if (paymentMethod == "WebBalance")
                        {
                            var balanceProp = customer.GetType().GetProperty("Balance");
                            decimal currentBalance = balanceProp != null ? (decimal)(balanceProp.GetValue(customer) ?? 0m) : 0m;

                            if (currentBalance < totalAmount)
                            {
                                transaction.Rollback();
                                TempData["Error"] = $"Số dư tài khoản của bạn ({currentBalance:N0} đ) không đủ để thanh toán đơn hàng này ({totalAmount:N0} đ)!";
                                return View(cart);
                            }

                            // Tiến hành trừ tiền và cập nhật lại vào model
                            decimal newBalance = currentBalance - totalAmount;
                            balanceProp?.SetValue(customer, newBalance);
                            db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                        }

                        // Tạo Đơn hàng (Order) dùng Reflection để tự động tương thích mọi tên cột (Anh/Việt)
                        var order = new Order();
                        order.GetType().GetProperty("CustomerID")?.SetValue(order, customerId);
                        order.GetType().GetProperty("MaKH")?.SetValue(order, customerId);

                        order.GetType().GetProperty("OrderDate")?.SetValue(order, DateTime.Now);
                        order.GetType().GetProperty("NgayDat")?.SetValue(order, DateTime.Now);

                        order.GetType().GetProperty("DeliveryAddress")?.SetValue(order, address);
                        order.GetType().GetProperty("DiaChiGiaoHang")?.SetValue(order, address);
                        order.GetType().GetProperty("Address")?.SetValue(order, address);

                        order.GetType().GetProperty("PaymentMethod")?.SetValue(order, paymentMethod);
                        order.GetType().GetProperty("PhuongThucThanhToan")?.SetValue(order, paymentMethod);

                        order.GetType().GetProperty("OrderStatus")?.SetValue(order, "Đang xử lý");
                        order.GetType().GetProperty("TrangThai")?.SetValue(order, "Đang xử lý");

                        db.Orders.Add(order);
                        db.SaveChanges(); // Lưu để sinh OrderID

                        int orderId = (int)(order.GetType().GetProperty("OrderID")?.GetValue(order)
                                             ?? order.GetType().GetProperty("MaDonHang")?.GetValue(order) ?? 0);

                        // Lưu chi tiết đơn hàng (OrderDetail)
                        foreach (var item in cart.Items)
                        {
                            var detail = new OrderDetail();
                            detail.GetType().GetProperty("OrderID")?.SetValue(detail, orderId);
                            detail.GetType().GetProperty("MaDonHang")?.SetValue(detail, orderId);

                            detail.GetType().GetProperty("ProductID")?.SetValue(detail, item.ProductID);
                            detail.GetType().GetProperty("MaSanPham")?.SetValue(detail, item.ProductID);

                            detail.GetType().GetProperty("Quantity")?.SetValue(detail, item.Quantity);
                            detail.GetType().GetProperty("SoLuong")?.SetValue(detail, item.Quantity);

                            detail.GetType().GetProperty("UnitPrice")?.SetValue(detail, item.ProductPrice);
                            detail.GetType().GetProperty("DonGia")?.SetValue(detail, item.ProductPrice);

                            db.OrderDetails.Add(detail);
                        }

                        db.SaveChanges();
                        transaction.Commit();

                        // Xóa sạch giỏ hàng sau khi đặt thành công
                        cart.Clear();
                        cartService.SaveCart(cart);

                        // Chuyển hướng sang trang thành công
                        return RedirectToAction("OrderSuccess", new { id = orderId });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine("Lỗi thanh toán: " + ex.Message);
                        ModelState.AddModelError("", "Lỗi khi lưu đơn hàng vào CSDL: " + ex.Message);
                        return View(cart);
                    }
                }
            }
        }

        // 3. GET: Order/OrderSuccess
        public ActionResult OrderSuccess(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }
    }
}