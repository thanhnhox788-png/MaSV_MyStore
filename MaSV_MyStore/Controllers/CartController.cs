using System;
using System.Linq;
using System.Web.Mvc;
using MaSV_MyStore.Models.ViewModels;

namespace MaSV_MyStore.Controllers
{
    public class CartController : Controller
    {
        private CartService cartService = new CartService();

        // ⭐ HÀM KIỂM TRA ĐĂNG NHẬP (Bắt đồng bộ với AccountController) ⭐
        private object GetLoggedUser()
        {
            var user = Session["Account"] ?? Session["Username"] ?? Session["User"] ?? Session["TaiKhoan"];
            return user;
        }

        // 1. GET: Cart/Index - Hiển thị trang chi tiết giỏ hàng
        public ActionResult Index()
        {
            if (GetLoggedUser() == null)
            {
                TempData["Error"] = "Hãy đăng nhập để mua hàng!";
                return RedirectToAction("Login", "Account");
            }

            var cart = cartService.GetCart();
            return View(cart);
        }

        // 2. Thêm sản phẩm vào giỏ hàng
        public ActionResult AddToCart(int? id, int? productId, int quantity = 1)
        {
            if (GetLoggedUser() == null)
            {
                TempData["Error"] = "Hãy đăng nhập để mua hàng!";
                return RedirectToAction("Login", "Account");
            }

            int realId = id ?? productId ?? 0;
            if (realId <= 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var cart = cartService.GetCart();
            cart.AddItem(realId, quantity);
            cartService.SaveCart(cart);

            return RedirectToAction("Index");
        }

        // 3. Hỗ trợ nút Mua ngay / Checkout từ trang chủ
        public ActionResult Checkout(int id, int quantity = 1)
        {
            if (GetLoggedUser() == null)
            {
                TempData["Error"] = "Hãy đăng nhập để mua hàng!";
                return RedirectToAction("Login", "Account");
            }

            var cart = cartService.GetCart();
            cart.AddItem(id, quantity);
            cartService.SaveCart(cart);

            return RedirectToAction("Checkout", "Order");
        }

        public ActionResult BuyNow(int id, int quantity = 1)
        {
            return Checkout(id, quantity);
        }

        // 🌟 3.1. THÊM CHỨC NĂNG MUA LẠI ĐƠN HÀNG (GIỐNG CÁC WEB LỚN)
        public ActionResult BuyAgain(int? orderId)
        {
            if (GetLoggedUser() == null)
            {
                TempData["Error"] = "Hãy đăng nhập để mua hàng!";
                return RedirectToAction("Login", "Account");
            }

            if (orderId == null)
            {
                return RedirectToAction("MyOrders", "Account");
            }

            try
            {
                using (var db = new MaSV_MyStore.Models.MyStoreEntities1())
                {
                    var order = db.Orders.Include("OrderDetails").FirstOrDefault(o => o.OrderID == orderId);
                    if (order == null || order.OrderDetails == null || !order.OrderDetails.Any())
                    {
                        TempData["Error"] = "Không tìm thấy thông tin đơn hàng để mua lại.";
                        return RedirectToAction("MyOrders", "Account");
                    }

                    var cart = cartService.GetCart();
                    foreach (var detail in order.OrderDetails)
                    {
                        if (detail.ProductID > 0 && detail.Quantity > 0)
                        {
                            cart.AddItem(detail.ProductID, detail.Quantity);
                        }
                    }
                    cartService.SaveCart(cart);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi Mua lại đơn hàng: " + ex.Message);
                return RedirectToAction("MyOrders", "Account");
            }
        }

        // 4. POST: Cart/UpdateQuantity - Cập nhật số lượng
        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            if (GetLoggedUser() == null)
            {
                TempData["Error"] = "Hãy đăng nhập để mua hàng!";
                return RedirectToAction("Login", "Account");
            }

            var cart = cartService.GetCart();
            if (cart != null)
            {
                cart.UpdateQuantity(productId, quantity);
                cartService.SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        // 5. GET: Cart/RemoveFromCart - Xóa sản phẩm
        public ActionResult RemoveFromCart(int productId)
        {
            if (GetLoggedUser() == null)
            {
                TempData["Error"] = "Hãy đăng nhập để mua hàng!";
                return RedirectToAction("Login", "Account");
            }

            var cart = cartService.GetCart();
            cart.RemoveItem(productId);
            cartService.SaveCart(cart);

            return RedirectToAction("Index");
        }

        // 6. GET: Cart/ClearCart - Xóa toàn bộ giỏ hàng
        public ActionResult ClearCart()
        {
            if (GetLoggedUser() == null)
            {
                TempData["Error"] = "Hãy đăng nhập để mua hàng!";
                return RedirectToAction("Login", "Account");
            }

            var cart = cartService.GetCart();
            if (cart != null)
            {
                cart.Clear();
                cartService.SaveCart(cart);
            }
            return RedirectToAction("Index");
        }
    }
}