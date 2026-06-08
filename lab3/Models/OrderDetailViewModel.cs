using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace lab3.Models
{
    public class OrderDetailViewModel
    {
        public int OrderID { get; set; }
        public int BookID { get; set; }
        public string BookName { get; set; } // Thay cho Book ID đơn điệu
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? TotalPrice { get; set; } // Cột Thành tiền = Quantity * Price
    }
}