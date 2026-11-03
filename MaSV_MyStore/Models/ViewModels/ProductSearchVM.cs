using System.Collections.Generic;
using PagedList;
using MaSV_MyStore.Models;

namespace MaSV_MyStore.Models.ViewModels
{
    public class ProductSearchVM
    {
        public string SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SortOrder { get; set; }

        // Đổi từ List thành IPagedList
        public IPagedList<Product> Products { get; set; }
    }
}