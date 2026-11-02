using System.Linq;
using System.Web.Mvc;
using MaSV_MyStore.Models;
using MaSV_MyStore.Models.ViewModels;

namespace MaSV_MyStore.Controllers
{
    public class CartController : Controller
    {
        private MyStoreEntities1 db = new MyStoreEntities1();
        private CartService cartService = new CartService();

        // GET: Cart
        public ActionResult Index()
        {
            var cart = cartService.GetCart();
            return View(cart);
        }

        // Thêm sản phẩm vào giỏ
        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = db.Products.Find(productId);
            if (product == null)
                return HttpNotFound();

            var cart = cartService.GetCart();

            cart.AddItem(new CartItem
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                ProductImage = product.ProductImage,
                ProductPrice = product.ProductPrice,
                Quantity = quantity,
                Category = product.Category != null ? product.Category.CategoryName : ""
            });

            cartService.SaveCart(cart);

            return RedirectToAction("Index");
        }

        // Xóa sản phẩm khỏi giỏ
        [HttpPost]
        public ActionResult RemoveFromCart(int productId)
        {
            var cart = cartService.GetCart();
            cart.RemoveItem(productId);
            cartService.SaveCart(cart);
            return RedirectToAction("Index");
        }

        // Cập nhật số lượng
        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = cartService.GetCart();
            cart.UpdateQuantity(productId, quantity);
            cartService.SaveCart(cart);
            return RedirectToAction("Index");
        }

        // Xóa toàn bộ giỏ hàng
        public ActionResult ClearCart()
        {
            var cart = cartService.GetCart();
            cart.Clear();
            cartService.SaveCart(cart);
            return RedirectToAction("Index");
        }
    }
}