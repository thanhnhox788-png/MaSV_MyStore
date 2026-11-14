using System;
using System.Data.Entity.Validation;
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

        // 2. POST: Order/Checkout - Xử lý lưu đơn hàng từ form của bạn
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

            using (var db = new MyStoreEntities1())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Tạo khách hàng mới (hoặc vãng lai)
                        var customer = new Customer();

                        customer.GetType().GetProperty("CustomerName")?.SetValue(customer, customerName);
                        customer.GetType().GetProperty("HoTen")?.SetValue(customer, customerName);

                        customer.GetType().GetProperty("Phone")?.SetValue(customer, phone);
                        customer.GetType().GetProperty("SoDienThoai")?.SetValue(customer, phone);

                        customer.GetType().GetProperty("CustomerAddress")?.SetValue(customer, address);
                        customer.GetType().GetProperty("DiaChi")?.SetValue(customer, address);

                        string uniqueKey = "KH_" + DateTime.Now.Ticks.ToString().Substring(10);
                        customer.GetType().GetProperty("Username")?.SetValue(customer, uniqueKey);
                        customer.GetType().GetProperty("TenDangNhap")?.SetValue(customer, uniqueKey);
                        customer.GetType().GetProperty("Account")?.SetValue(customer, uniqueKey);
                        customer.GetType().GetProperty("TaiKhoan")?.SetValue(customer, uniqueKey);

                        customer.GetType().GetProperty("Email")?.SetValue(customer, uniqueKey + "@gmail.com");
                        customer.GetType().GetProperty("Password")?.SetValue(customer, "123456");
                        customer.GetType().GetProperty("MatKhau")?.SetValue(customer, "123456");

                        db.Customers.Add(customer);
                        db.SaveChanges(); // Lưu để sinh CustomerID

                        int customerId = (int)(customer.GetType().GetProperty("CustomerID")?.GetValue(customer)
                                              ?? customer.GetType().GetProperty("MaKH")?.GetValue(customer) ?? 1);

                        // Tạo Đơn hàng (Order)
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

                        // Chuyển hướng sang trang thành công kèm theo Mã đơn hàng
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