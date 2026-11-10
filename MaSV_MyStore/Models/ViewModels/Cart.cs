using MaSV_MyStore.Models; // Đảm bảo có namespace chứa bảng Product của bạn
using System;
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

        // 1. Giữ nguyên phương thức AddItem cũ nhận CartItem
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

        // ⭐️ 2. Phương thức AddItem nhận vào (int productId, int quantity)
        public void AddItem(int productId, int quantity)
        {
            using (var db = new MyStoreEntities1())
            {
                // Tìm sản phẩm trong cơ sở dữ liệu
                var product = db.Products.Find(productId);
                if (product != null)
                {
                    // Lấy thông tin linh hoạt từ Product
                    int pId = (int)(product.GetType().GetProperty("ProductID")?.GetValue(product) ?? product.GetType().GetProperty("MaSanPham")?.GetValue(product) ?? productId);
                    string pName = product.GetType().GetProperty("ProductName")?.GetValue(product)?.ToString() ?? product.GetType().GetProperty("TenSanPham")?.GetValue(product)?.ToString() ?? "Sản phẩm";
                    string pImage = product.GetType().GetProperty("ProductImage")?.GetValue(product)?.ToString() ?? product.GetType().GetProperty("HinhAnh")?.GetValue(product)?.ToString() ?? "";

                    // ⭐️ ĐÃ BỔ SUNG THÊM "ProductPrice" ĐỂ LẤY ĐÚNG GIÁ TIỀN TỪ CSDL
                    decimal pPrice = Convert.ToDecimal(
                        product.GetType().GetProperty("ProductPrice")?.GetValue(product) ??
                        product.GetType().GetProperty("Price")?.GetValue(product) ??
                        product.GetType().GetProperty("GiaBan")?.GetValue(product) ?? 0);

                    // Gọi lại logic thêm vào giỏ hàng
                    var existingItem = items.FirstOrDefault(x => x.ProductID == pId);
                    if (existingItem != null)
                    {
                        existingItem.Quantity += quantity;
                    }
                    else
                    {
                        items.Add(new CartItem
                        {
                            ProductID = pId,
                            ProductName = pName,
                            ProductImage = pImage,
                            ProductPrice = pPrice,
                            Quantity = quantity
                        });
                    }
                }
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