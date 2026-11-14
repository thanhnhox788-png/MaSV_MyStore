using System.Collections.Generic;
using MaSV_MyStore.Models;

namespace MaSV_MyStore.Models.ViewModels
{
    public class ProductSearchVM
    {
        public string SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Đổi từ IPagedList sang List
        public List<Product> Products { get; set; }
    }
}