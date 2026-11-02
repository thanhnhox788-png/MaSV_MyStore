using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MaSV_MyStore.Models; // Thay MaSV bằng mã sinh viên của bạn

namespace MaSV_MyStore.Areas.Admin.Controllers
{
    public class CategoriesController : Controller
    {
        private MyStoreEntities1 db = new MyStoreEntities1();

        // GET: Admin/Categories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // Các Action khác (Index, Edit, Delete, Details) giữ nguyên...
    }
}