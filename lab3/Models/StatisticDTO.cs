using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace lab3.Models
{
    public class StatisticDTO
    {
        public string BookName { get; set; }

        // Số lượng bán được (dùng int? để chấp nhận cả trường hợp null)
        public int? SaleQuantity { get; set; }
    }
}