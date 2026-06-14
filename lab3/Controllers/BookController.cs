using lab3.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace lab3.Controllers
{
    public class BookController : Controller
    {
        dbBookStoreDataContext db = new dbBookStoreDataContext();

        // GET: Book/Home
        public ActionResult Home(int? size, int? page)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "3", Value = "3" });
            items.Add(new SelectListItem { Text = "6", Value = "6" });
            items.Add(new SelectListItem { Text = "12", Value = "12" });
            items.Add(new SelectListItem { Text = "24", Value = "24" });
            items.Add(new SelectListItem { Text = "48", Value = "48" });

            foreach (var item in items)
            {
                if (item.Value == size.ToString()) { item.Selected = true; }
            }
            ViewBag.size = items;

            var all_books = from book in db.books select book;
            int pageSize = (size ?? 3);
            int pageNum = (page ?? 1);
            return View(all_books.ToPagedList(pageNum, pageSize));
        }

        // GET: Index (Tìm kiếm & Phân trang)
        public ActionResult Index(int? page, string searchString)
        {
            ViewBag.Keyword = searchString;
            var all_books = from s in db.books select s;

            if (!string.IsNullOrEmpty(searchString))
            {
                all_books = all_books.Where(a => a.book_name.Contains(searchString));
            }

            int pageSize = 3;
            int pageNum = (page ?? 1);
            return View(all_books.OrderBy(a => a.book_id).ToPagedList(pageNum, pageSize));
        }

        // GET: Detail
        public ActionResult Detail(int id)
        {
            var D_book = db.books.Where(m => m.book_id == id).First();
            return View(D_book);
        }

        // GET: Edit
        public ActionResult Edit(int id)
        {
            var E_sach = db.books.First(m => m.book_id == id);
            return View(E_sach);
        }

        // POST: Edit
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            var E_book = db.books.First(m => m.book_id == id);
            var E_name = collection["book_name"];
            var E_image = collection["image"];

            if (string.IsNullOrEmpty(E_name))
            {
                ViewData["Error"] = "Không được để trống!";
                return View(E_book);
            }

            E_book.book_name = E_name;
            E_book.image = E_image;
            E_book.price = Convert.ToDecimal(collection["price"]);
            E_book.update_date = Convert.ToDateTime(collection["update_date"]);
            E_book.quantity_instock = Convert.ToInt32(collection["quantity_instock"]);

            UpdateModel(E_book);
            db.SubmitChanges();
            return RedirectToAction("Index");
        }

        // GET: Create
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.DefaultDate = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        // POST: Create (Đã cập nhật an toàn)
        [HttpPost]
        public ActionResult Create(FormCollection collection, book s)
        {
            var E_name = collection["book_name"];
            var E_image = collection["image"];
            var E_price_str = collection["price"];
            var E_updatedate_str = collection["update_date"];
            var E_quantity_str = collection["quantity_instock"];

            if (string.IsNullOrEmpty(E_name))
            {
                ViewData["Error"] = "Tên sách không được để trống!";
                ViewBag.DefaultDate = DateTime.Now.ToString("yyyy-MM-dd");
                return View();
            }

            try
            {
                s.book_name = E_name;
                s.image = E_image;

                decimal price;
                decimal.TryParse(E_price_str, out price);
                s.price = price;

                DateTime updatedate;
                DateTime.TryParse(E_updatedate_str, out updatedate);
                s.update_date = updatedate;

                int quantity;
                int.TryParse(E_quantity_str, out quantity);
                s.quantity_instock = quantity;

                db.books.InsertOnSubmit(s);
                db.SubmitChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["Error"] = "Lỗi hệ thống: " + ex.Message;
                return View();
            }
        }

        // GET: Delete
        public ActionResult Delete(int id)
        {
            var D_book = db.books.First(m => m.book_id == id);
            return View(D_book);
        }

        // POST: Delete
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            var D_sach = db.books.Where(m => m.book_id == id).First();
            db.books.DeleteOnSubmit(D_sach);
            db.SubmitChanges();
            return RedirectToAction("Index");
        }

        public string ProcessUpload(HttpPostedFileBase file)
        {
            if (file == null) return "";
            file.SaveAs(Server.MapPath("~/Content/img/" + file.FileName));
            return "/Content/img/" + file.FileName;
        }

        public ActionResult About() { return View(); }
        public ActionResult Contact() { return View(); }
    }
}