using System.Web;

namespace MaSV_MyStore.Models.ViewModels
{
    public class CartService
    {
        private const string CartSessionKey = "Cart";

        public Cart GetCart()
        {
            var cart = HttpContext.Current.Session[CartSessionKey] as Cart;
            if (cart == null)
            {
                cart = new Cart();
                HttpContext.Current.Session[CartSessionKey] = cart;
            }
            return cart;
        }

        public void SaveCart(Cart cart)
        {
            HttpContext.Current.Session[CartSessionKey] = cart;
        }
    }
}