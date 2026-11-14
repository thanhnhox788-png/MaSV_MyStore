using System.Linq;
using System.Web.Mvc;
using MaSV_MyStore.Models;

namespace MaSV_MyStore.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private MyStoreEntities1 db = new MyStoreEntities1();

        // GET: Admin/Products
        public ActionResult Index()
        {
            var products = db.Products.OrderByDescending(p => p.ProductID).ToList();
            return View(products);
        }

        // GET: Admin/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product model)
        {
            if (ModelState.IsValid)
            {
                var product = db.Products.Find(model.ProductID);
                if (product != null)
                {
                    product.ProductName = model.ProductName;
                    product.ProductDescription = model.ProductDescription;
                    product.ProductPrice = model.ProductPrice;
                    product.ProductImage = model.ProductImage;
                    product.CategoryID = model.CategoryID;

                    db.SaveChanges();

                    TempData["Success"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction("Index");
                }
            }

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}