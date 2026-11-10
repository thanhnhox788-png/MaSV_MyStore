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

        // 2. POST: Order/Checkout - Xử lý lưu đơn hàng khi bấm "ĐẶT HÀNG NGAY"
        [HttpPost]
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
                        // ⭐️ Fix lỗi LINQ to Entities bằng cách dùng AsEnumerable() để so sánh số điện thoại an toàn
                        var customer = db.Customers.AsEnumerable().FirstOrDefault(c =>
                            (c.GetType().GetProperty("Phone")?.GetValue(c)?.ToString() == phone) ||
                            (c.GetType().GetProperty("SoDienThoai")?.GetValue(c)?.ToString() == phone)
                        );

                        if (customer == null)
                        {
                            customer = new Customer();
                            customer.GetType().GetProperty("CustomerName")?.SetValue(customer, customerName);
                            customer.GetType().GetProperty("HoTen")?.SetValue(customer, customerName);

                            customer.GetType().GetProperty("Phone")?.SetValue(customer, phone);
                            customer.GetType().GetProperty("SoDienThoai")?.SetValue(customer, phone);

                            customer.GetType().GetProperty("CustomerAddress")?.SetValue(customer, address);
                            customer.GetType().GetProperty("DiaChi")?.SetValue(customer, address);

                            db.Customers.Add(customer);
                            db.SaveChanges(); // Lưu để sinh ra CustomerID
                        }

                        // Lấy ra CustomerID
                        int customerId = (int)(customer.GetType().GetProperty("CustomerID")?.GetValue(customer)
                                      ?? customer.GetType().GetProperty("MaKH")?.GetValue(customer) ?? 1);

                        // Tạo đối tượng Order (Đơn hàng)
                        var order = new Order();
                        order.GetType().GetProperty("CustomerID")?.SetValue(order, customerId);
                        order.GetType().GetProperty("MaKH")?.SetValue(order, customerId);

                        order.GetType().GetProperty("OrderDate")?.SetValue(order, DateTime.Now);
                        order.GetType().GetProperty("NgayDat")?.SetValue(order, DateTime.Now);

                        order.GetType().GetProperty("DeliveryAddress")?.SetValue(order, address);
                        order.GetType().GetProperty("DiaChiGiaoHang")?.SetValue(order, address);

                        order.GetType().GetProperty("PaymentMethod")?.SetValue(order, paymentMethod);
                        order.GetType().GetProperty("PhuongThucThanhToan")?.SetValue(order, paymentMethod);

                        order.GetType().GetProperty("OrderStatus")?.SetValue(order, "Đang xử lý");
                        order.GetType().GetProperty("TrangThai")?.SetValue(order, "Đang xử lý");

                        db.Orders.Add(order);
                        db.SaveChanges(); // Lưu để sinh ra OrderID

                        int orderId = (int)(order.GetType().GetProperty("OrderID")?.GetValue(order)
                                    ?? order.GetType().GetProperty("MaDonHang")?.GetValue(order) ?? 0);

                        // Lưu chi tiết đơn hàng (OrderDetails)
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

                        // Chuyển hướng đến trang thông báo đặt hàng thành công
                        return RedirectToAction("OrderSuccess", new { id = orderId });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Lỗi khi lưu đơn hàng vào CSDL: " + ex.Message);
                        return View(cart);
                    }
                }
            }
        }

        // 3. GET: Order/OrderSuccess - Trang hiển thị thông báo đặt hàng thành công
        public ActionResult OrderSuccess(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }
    }
}