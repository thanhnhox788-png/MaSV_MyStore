using System.Collections.Generic;

namespace MaSV_MyStore.Models.ViewModels
{
    public class ProductDetailVM
    {
        public Product Product { get; set; }
        public List<Product> RelatedProducts { get; set; }   // 8 sản phẩm cùng danh mục
        public List<Product> TopProducts { get; set; }        // 8 sản phẩm bán chạy cùng danh mục
        public int Quantity { get; set; } = 1;
    }
}