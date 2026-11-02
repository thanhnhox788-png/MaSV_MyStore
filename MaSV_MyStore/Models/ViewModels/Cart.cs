using System.Collections.Generic;
using System.Linq;

namespace MaSV_MyStore.Models.ViewModels
{
    public class Cart
    {
        private List<CartItem> items = new List<CartItem>();

        public IEnumerable<CartItem> Items
        {
            get { return items; }
        }

        public void AddItem(CartItem item)
        {
            var existingItem = items.FirstOrDefault(x => x.ProductID == item.ProductID);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                items.Add(item);
            }
        }

        public void RemoveItem(int productId)
        {
            items.RemoveAll(x => x.ProductID == productId);
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var item = items.FirstOrDefault(x => x.ProductID == productId);
            if (item != null)
            {
                if (quantity > 0)
                    item.Quantity = quantity;
                else
                    RemoveItem(productId);
            }
        }

        public void Clear()
        {
            items.Clear();
        }

        public decimal TotalAmount
        {
            get { return items.Sum(x => x.Total); }
        }

        public int TotalQuantity
        {
            get { return items.Sum(x => x.Quantity); }
        }
    }
}