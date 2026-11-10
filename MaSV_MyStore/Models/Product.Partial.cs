namespace MaSV_MyStore.Models
{
    using System.Web;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Product
    {
        [NotMapped]
        public HttpPostedFileBase UploadImg { get; set; }
    }
}