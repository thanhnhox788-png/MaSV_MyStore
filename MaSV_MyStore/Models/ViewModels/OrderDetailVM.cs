namespace MaSV_MyStore.Models.ViewModels
{
    public class OrderDetailVM
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
    }
}