using lab3.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Web;
namespace lab3.Models

{
    public class Cart



    {
        dbBookStoreDataContext db = new dbBookStoreDataContext();
        public int book_id { get; set; }
        [Display(Name = "Book name")]



        public string book_name { get; set; }



        [Display(Name = "Cover image")]



        public string image { get; set; }



        [Display(Name = "Price")]



        public Double price { get; set; }



        [Display(Name = "Quantity")]



        public int iquantity { get; set; }



        [Display(Name = "Total")]

        // Thêm thuộc tính này vào file Models/Cart.cs

        public int quantity_instock { get; set; }



        public Double Total



        {

            get { return iquantity * price; }

        }



        public Cart(int id)



        {



            book_id = id;



            book b = db.books.Single(n => n.book_id == book_id);



            book_name = b.book_name;



            image = b.image;



            price = double.Parse(b.price.ToString());



            iquantity = 1;

        }

    }

}