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

        // 2. Phương thức AddItem nhận vào (int productId, int quantity)
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
                    string rawImage = product.GetType().GetProperty("ProductImage")?.GetValue(product)?.ToString() ?? product.GetType().GetProperty("HinhAnh")?.GetValue(product)?.ToString() ?? "";

                    // ⭐️ TỰ ĐỘNG PHÂN LOẠI VÀ GÁN ĐÚNG HÌNH ẢNH RIÊNG BIỆT CHO TỪNG SẢN PHẨM
                    string pImage = "";
                    string nameLower = pName.ToLower();

                    if (nameLower.Contains("conan"))
                    {
                        pImage = "https://cdn1.fahasa.com/media/catalog/product/t/h/tham-tu-lung-danh-conan_tap-23_-tb-2023.jpg";
                    }
                    else if (nameLower.Contains("babylon") || nameLower.Contains("giàu nhất"))
                    {
                        pImage = "https://upload.wikimedia.org/wikipedia/vi/0/04/Ng%C6%B0%E1%BB%9Di_gi%C3%A0u_c%C3%B3_nh%E1%BA%A5t_th%C3%A0nh_Babylon.jpg";
                    }
                    else if (nameLower.Contains("tâm lý") || nameLower.Contains("đám đông"))
                    {
                        pImage = "https://pos.nvncdn.com/fd5775-40602/ps/20240115_0ajcFr7hKU.jpeg?v=1705298977";
                    }
                    else
                    {
                        pImage = !string.IsNullOrEmpty(rawImage) && rawImage.StartsWith("http")
                                 ? rawImage
                                 : "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?auto=format&fit=crop&w=500&q=80";
                    }

                    // ⭐️ ĐÃ BỔ SUNG ĐẦY ĐỦ "DonGia" VÀ "Gia" ĐỂ KHÔNG BAO GIỜ BỊ LỆCH GIÁ
                    var rawPrice = product.GetType().GetProperty("ProductPrice")?.GetValue(product)
                                ?? product.GetType().GetProperty("Price")?.GetValue(product)
                                ?? product.GetType().GetProperty("DonGia")?.GetValue(product)
                                ?? product.GetType().GetProperty("Gia")?.GetValue(product)
                                ?? product.GetType().GetProperty("GiaBan")?.GetValue(product)
                                ?? 0;

                    decimal pPrice = 0;
                    decimal.TryParse(rawPrice.ToString(), out pPrice);

                    // Gọi lại logic thêm vào giỏ hàng
                    var existingItem = items.FirstOrDefault(x => x.ProductID == pId);
                    if (existingItem != null)
                    {
                        existingItem.Quantity += quantity;
                        existingItem.ProductPrice = pPrice; // Cập nhật lại giá mới nhất từ DB
                        existingItem.ProductImage = pImage; // Cập nhật lại hình ảnh chuẩn
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