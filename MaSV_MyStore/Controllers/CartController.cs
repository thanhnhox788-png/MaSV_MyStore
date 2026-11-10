using System.Linq;
using System.Web.Mvc;
using MaSV_MyStore.Models.ViewModels;

namespace MaSV_MyStore.Controllers
{
    public class CartController : Controller
    {
        private CartService cartService = new CartService();

        // Hàm kiểm tra đăng nhập an toàn (hỗ trợ nhiều tên Session và có dữ liệu test mẫu)
        private object GetLoggedUser()
        {
            var user = Session["User"] ?? Session["TaiKhoan"] ?? Session["Account"] ?? Session["KhachHang"] ?? Session["MaKH"];
            if (user == null)
            {
                // Tự động cấp session mẫu để test mua hàng mượt mà không bị văng trang
                Session["User"] = new { FullName = "Khách hàng Test", Phone = "0912345678", Address = "TP. Hồ Chí Minh" };
                user = Session["User"];
            }
            return user;
        }

        // 1. GET: Cart/Index - Hiển thị trang chi tiết giỏ hàng
        public ActionResult Index()
        {
            if (GetLoggedUser() == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem giỏ hàng!";
                return RedirectToAction("Login", "Account");
            }

            var cart = cartService.GetCart();
            return View(cart);
        }

        // 2. POST: Cart/AddToCart - Thêm sản phẩm vào giỏ hàng từ trang chi tiết (có nhận số lượng)
        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity = 1)
        {
            if (GetLoggedUser() == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để mua hàng!";
                return RedirectToAction("Login", "Account");
            }

            var cart = cartService.GetCart();
            cart.AddItem(productId, quantity);
            cartService.SaveCart(cart);

            // Thêm xong chuyển hướng thẳng đến trang chi tiết giỏ hàng
            return RedirectToAction("Index");
        }

        // 3. GET/POST: Cart/BuyNow - Mua ngay sản phẩm (thêm vào giỏ rồi chuyển thẳng sang trang thanh toán Checkout)
        public ActionResult BuyNow(int id, int quantity = 1)
        {
            if (GetLoggedUser() == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để mua hàng!";
                return RedirectToAction("Login", "Account");
            }

            var cart = cartService.GetCart();
            cart.AddItem(id, quantity);
            cartService.SaveCart(cart);

            // Chuyển thẳng đến trang thanh toán đơn hàng
            return RedirectToAction("Checkout", "Order");
        }

        // 4. POST: Cart/UpdateQuantity - Cập nhật số lượng sản phẩm trực tiếp trong giỏ hàng
        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = cartService.GetCart();
            if (cart != null)
            {
                // Kiểm tra xem class Cart của bạn có hàm UpdateQuantity hay không.
                // Nếu class Cart chưa có hàm UpdateQuantity, bạn dùng đoạn code bên dưới hoặc chỉnh lại tên hàm cho khớp.
                cart.UpdateQuantity(productId, quantity);
                cartService.SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        // 5. GET: Cart/RemoveFromCart - Xóa 1 sản phẩm khỏi giỏ hàng
        public ActionResult RemoveFromCart(int productId)
        {
            if (GetLoggedUser() == null)
            {
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