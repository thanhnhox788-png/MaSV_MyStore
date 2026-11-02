namespace MaSV_MyStore.Models.ViewModels
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }

        public decimal Total
        {
            get { return ProductPrice * Quantity; }
        }
    }
}