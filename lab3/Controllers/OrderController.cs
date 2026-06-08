using lab3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace lab3.Controllers
{
    public class OrderController : Controller
    {
        dbBookStoreDataContext db = new dbBookStoreDataContext();

        // =========================================================================
        // TÍNH NĂNG 12: Purchase History (Dành cho KHÁCH HÀNG xem lịch sử của mình)
        // =========================================================================
        public ActionResult PurchaseHistory()
        {
            // Kiểm tra khách hàng đăng nhập chưa, nếu chưa bắt quay lại trang Login
            if (Session["User"] == null || string.IsNullOrEmpty(Session["User"].ToString()))
            {
                return RedirectToAction("Login", "User");
            }

            customer kh = (customer)Session["User"];

            // Lấy toàn bộ danh sách đơn hàng của khách hàng này từ SQL Server
            var listOrders = db.orders
                               .Where(o => o.customer_id == kh.customer_id)
                               .OrderByDescending(o => o.order_date)
                               .ToList();

            return View(listOrders);
        }

        // =========================================================================
        // TÍNH NĂNG 9: Quản lý danh sách đơn hàng công nghệ Linq nâng cao (Dành cho ADMIN)
        // =========================================================================
        public ActionResult Orders()
        {
            // Sửa triệt để lỗi Group By bằng cách lấy dữ liệu từ bảng orders làm gốc, sau đó kết nối sang
            var lst = (from a in db.orders
                       join c in db.customers on a.customer_id equals c.customer_id
                       select new Order
                       {
                           // Điền chính xác các thuộc tính theo Model gốc của bạn
                           order_ID = a.order_id,
                           customerID = c.customer_name, // Gán thẳng Tên khách hàng thay vì chuỗi ID số
                           isShip = a.isship ?? false,
                           isPayment = a.ispayment ?? false,
                           deliveryDate = a.delivery_date ?? DateTime.Now,
                           orderDate = a.order_date ?? DateTime.Now,

                           // Tính tổng tiền hóa đơn bằng cách Sum bảng chi tiết của chính đơn hàng đó
                           // Tránh ép sang double; tính và giữ kiểu decimal? để khớp với thuộc tính target
                           total = db.orderdetails
                                     .Where(od => od.order_id == a.order_id)
                                     .Sum(od => (decimal?)((od.quantity ?? 0) * (od.price ?? 0))) ?? 0m
                       }).ToList();

            return View(lst);
        }

        // =========================================================================
        // TÍNH NĂNG 10: Trang chi tiết đơn hàng (Detail) - Đã sửa lỗi ép kiểu dữ liệu null
        // =========================================================================
        public ActionResult Detail(int id)
        {
            // Thực hiện JOIN với bảng books để lấy Book Name thay cho ID
            var orderDetails = (from od in db.orderdetails
                                join b in db.books on od.book_id equals b.book_id
                                where od.order_id == id
                                select new OrderDetailViewModel
                                {
                                    OrderID = od.order_id,
                                    BookID = od.book_id,
                                    BookName = b.book_name, // Đạt yêu cầu: Lấy tên sách trực quan
                                    Quantity = od.quantity,
                                    Price = od.price,

                                    // Đạt yêu cầu: Cột thành tiền tự động tính
                                    TotalPrice = (od.quantity ?? 0) * (od.price ?? 0)
                                }).ToList();

            // Nếu không tìm thấy chi tiết nào của đơn hàng, trả về trang lỗi 404
            if (orderDetails == null || !orderDetails.Any())
            {
                return HttpNotFound();
            }

            return View(orderDetails);
        }
    }
}