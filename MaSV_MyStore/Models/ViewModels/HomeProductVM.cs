using System.Collections.Generic;
using MaSV_MyStore.Models;
using PagedList;

namespace MaSV_MyStore.Models.ViewModel
{
    public class HomeProductVM
    {
        // Tiêu chí tìm kiếm
        public string SearchTerm { get; set; }

        // Các thuộc tính hỗ trợ phân trang
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 10;

        // Danh sách sản phẩm nổi bật (bán chạy nhất)
        public List<Product> FeaturedProducts { get; set; }

        // Danh sách sản phẩm mới đã phân trang
        public PagedList.IPagedList<Product> NewProducts { get; set; }
    }
}