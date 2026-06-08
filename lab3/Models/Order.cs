using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace lab3.Models
{
    public class Order
    {
        public int order_ID { get; set; }
        public string customerID { get; set; }
        public bool isShip {  get; set; }
        public bool isPayment { get; set; }
        public DateTime orderDate { get; set; }
        public DateTime deliveryDate { get; set; }
        public decimal? total { get; set; }
    }
}