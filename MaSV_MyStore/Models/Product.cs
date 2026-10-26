namespace MaSV_MyStore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Web;

    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            this.OrderDetails = new HashSet<OrderDetail>();
        }

        public int ProductID { get; set; }
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm")]
        public decimal ProductPrice { get; set; }

        public string ProductImage { get; set; }

        public virtual Category Category { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        // Trường phụ trợ để upload file ảnh (Không lưu vào CSDL)
        [NotMapped]
        public HttpPostedFileBase UploadImg { get; set; }
    }
}