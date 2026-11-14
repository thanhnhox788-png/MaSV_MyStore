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