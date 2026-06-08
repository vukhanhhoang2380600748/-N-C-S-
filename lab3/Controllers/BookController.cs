using lab3.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Security.Policy;
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
                if (item.Value == size.ToString())
                {
                    item.Selected = true;
                }
            }
            ViewBag.size = items;
            if (page == null) page = 1;
            var all_books = from book in db.books select book;
            int pageSize = (size ?? 3);
            int pageNum = page ?? 1;
            return View(all_books.ToPagedList(pageNum, pageSize));
        }

        // ĐÂY LÀ HÀM INDEX DUY NHẤT - ĐÃ GỘP TÌM KIẾM VÀ PHÂN TRANG ĐỂ HẾT LỖI
        public ActionResult Index(int? page, string searchString)
        {
            // Lưu từ khóa tìm kiếm để hiển thị lại trên View nếu cần
            ViewBag.Keyword = searchString;

            var all_books = from s in db.books select s;

            // Xử lý Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                all_books = all_books.Where(a => a.book_name.Contains(searchString));
            }

            // Xử lý Phân trang
            int pageSize = 3;
            int pageNum = (page ?? 1);

            return View(all_books.OrderBy(a => a.book_id).ToPagedList(pageNum, pageSize));
        }

        // GET: Book/Detail/5
        public ActionResult Detail(int id)
        {
            var D_book = db.books.Where(m => m.book_id == id).First();
            return View(D_book);
        }

        // GET: Book/Edit/5
        public ActionResult Edit(int id)
        {
            var E_sach = db.books.First(m => m.book_id == id);
            return View(E_sach);
        }

        // POST: Book/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            var E_book = db.books.First(m => m.book_id == id);
            var E_name = collection["book_name"];
            var E_image = collection["image"];
            var E_price = Convert.ToDecimal(collection["price"]);
            var E_updateddate = Convert.ToDateTime(collection["update_date"]);
            var E_quantity = Convert.ToInt32(collection["quantity_instock"]);

            if (string.IsNullOrEmpty(E_name))
            {
                ViewData["Error"] = "Dont't empty!";
            }
            else
            {
                E_book.book_name = E_name;
                E_book.image = E_image;
                E_book.price = E_price;
                E_book.update_date = E_updateddate;
                E_book.quantity_instock = E_quantity;

                UpdateModel(E_book);
                db.SubmitChanges();

                return RedirectToAction("Index");
            }
            return this.Edit(id);
        }

        public string ProcessUpload(HttpPostedFileBase file)
        {
            if (file == null)
            {
                return "";
            }

            file.SaveAs(Server.MapPath("~/Content/img/" + file.FileName));
            return "/Content/img/" + file.FileName;
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(FormCollection collection, book s)
        {
            var E_name = collection["book_name"];
            var E_image = collection["image"];
            var E_price = Convert.ToDecimal(collection["price"]);
            var E_updatedate = Convert.ToDateTime(collection["update_date"]);
            var E_quantity = Convert.ToInt32(collection["quantity_instock"]);

            if (string.IsNullOrEmpty(E_name))
            {
                ViewData["Error"] = "Don't empty!";
            }
            else
            {
                s.book_name = E_name.ToString();
                s.image = E_image.ToString();
                s.price = E_price;
                s.update_date = E_updatedate;
                s.quantity_instock = E_quantity;

                db.books.InsertOnSubmit(s);
                db.SubmitChanges(); // Đã sửa từ GetChangeSet() sang SubmitChanges() để lưu dữ liệu

                return RedirectToAction("Index");
            }
            return this.Create();
        }

        public ActionResult Delete(int id)
        {
            var D_book = db.books.First(m => m.book_id == id);
            return View(D_book);
        }

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            var D_sach = db.books.Where(m => m.book_id == id).First();
            db.books.DeleteOnSubmit(D_sach);
            db.SubmitChanges();
            return RedirectToAction("Index");
        }
        public ActionResult About()
        {
           
            return View();
        }
        public ActionResult Contact()
        {
           
            return View();
        }

    }
}