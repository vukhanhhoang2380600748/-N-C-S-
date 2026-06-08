using lab3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace lab3.Controllers
{
    public class StatisticController : Controller
    {
        dbBookStoreDataContext db= new dbBookStoreDataContext();
        public ActionResult StatisticBook()
        {
            List<SumSaleQuantity> lst = (from a in db.books
                                         join b in db.orderdetails on a.book_id equals b.book_id
                                         group b by b.book_id into g
                                         select new SumSaleQuantity { BookName = g.First().book.book_name, SaleQuantity = g.Sum(x => x.quantity) }).ToList();
            return View(lst);
        }
    }
}