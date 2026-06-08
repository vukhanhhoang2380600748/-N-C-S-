using lab3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
// THÊM 2 THƯ VIỆN NÀY ĐỂ XỬ LÝ GỬI EMAIL
using System.Net;
using System.Net.Mail;

namespace lab3.Controllers
{
    public class CartController : Controller
    {
        dbBookStoreDataContext db = new dbBookStoreDataContext();

        // 1. Lấy giỏ hàng
        public List<Cart> GetCart()
        {
            List<Cart> listCart = Session["Cart"] as List<Cart>;

            if (listCart == null)
            {
                listCart = new List<Cart>();
                Session["Cart"] = listCart;
            }

            return listCart;
        }

        // 2. Thêm giỏ hàng
        public ActionResult AddCart(int id, string strURL)
        {
            List<Cart> listCart = GetCart();

            Cart product = listCart.Find(n => n.book_id == id);

            if (product == null)
            {
                product = new Cart(id);
                listCart.Add(product);
            }
            else
            {
                product.iquantity++;
            }

            return RedirectToAction("Index", "Book");
        }

        // 3. Tổng số lượng sách
        private int sumQuantity()
        {
            List<Cart> listCart = Session["Cart"] as List<Cart>;

            if (listCart == null)
                return 0;

            return listCart.Sum(n => n.iquantity);
        }

        // 4. Tổng số loại sản phẩm khác nhau
        private int sumProductQuantity()
        {
            List<Cart> listCart = Session["Cart"] as List<Cart>;

            if (listCart == null)
                return 0;

            return listCart.Count;
        }

        // 5. Tổng thành tiền
        private double Total()
        {
            List<Cart> listCart = Session["Cart"] as List<Cart>;

            if (listCart == null)
                return 0;

            return listCart.Sum(n => n.Total);
        }

        // 6. Hiển thị giỏ hàng chính
        public ActionResult Cart()
        {
            List<Cart> listCart = GetCart();

            ViewBag.sumQuantity = sumQuantity();
            ViewBag.sumProductQuantity = sumProductQuantity();
            ViewBag.Total = Total();

            return View(listCart);
        }

        // 7. Partial cart thu nhỏ hiển thị ở Header/Layout
        public ActionResult CartPartial()
        {
            ViewBag.sumQuantity = sumQuantity();
            ViewBag.sumProductQuantity = sumProductQuantity();
            ViewBag.Total = Total();

            return PartialView();
        }

        // 8. Xóa sản phẩm khỏi giỏ hàng
        public ActionResult CartDelete(int id)
        {
            List<Cart> listCart = GetCart();

            Cart product = listCart.SingleOrDefault(n => n.book_id == id);

            if (product != null)
            {
                listCart.RemoveAll(n => n.book_id == id);
            }

            if (Request.UrlReferrer != null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            return RedirectToAction("Cart");
        }

        // 9. Cập nhật số lượng sản phẩm
        [HttpPost]
        public ActionResult CartUpdate(int id, FormCollection collection)
        {
            List<Cart> listCart = GetCart();

            Cart product = listCart.SingleOrDefault(n => n.book_id == id);

            if (product != null)
            {
                if (!string.IsNullOrEmpty(collection["txtSoLg"]))
                {
                    product.iquantity = int.Parse(collection["txtSoLg"]);
                }
            }

            if (Request.UrlReferrer != null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            return RedirectToAction("Cart");
        }

        // 10. Xóa toàn bộ giỏ hàng
        public ActionResult AllCartDelete()
        {
            List<Cart> listCart = GetCart();

            listCart.Clear();

            return RedirectToAction("Cart");
        }

        // 11. [GET] Hiển thị giao diện đặt hàng
        [HttpGet]
        public ActionResult PlaceOrder()
        {
            if (Session["User"] == null || string.IsNullOrEmpty(Session["User"].ToString()))
            {
                return RedirectToAction("Login", "User");
            }
            if (Session["Cart"] == null)
            {
                return RedirectToAction("Index", "Book");
            }

            List<Cart> lstCart = GetCart();
            ViewBag.sumQuantity = sumQuantity();
            ViewBag.sumProductQuantity = sumProductQuantity();
            ViewBag.Total = Total();
            return View(lstCart);
        }

        // 12. [POST] Xử lý bấm Xác nhận đặt hàng và lưu vào SQL Server
        [HttpPost]
        public ActionResult PlaceOrder(FormCollection collection)
        {
            if (string.IsNullOrEmpty(collection["delivery_date"]))
            {
                return RedirectToAction("PlaceOrder");
            }

            order dh = new order();
            customer kh = (customer)Session["User"];
            book s = new book();
            List<Cart> gh = GetCart();

            var delivery_date = String.Format("{0:MM/dd/yyyy}", collection["delivery_date"]);
            dh.customer_id = kh.customer_id;
            dh.order_date = DateTime.Now;
            dh.delivery_date = DateTime.Parse(delivery_date);
            dh.isship = false;
            dh.ispayment = false;

            db.orders.InsertOnSubmit(dh);
            db.SubmitChanges();

            double tongTienFormail = 0; // Biến phụ dùng tính tổng tiền gửi vào Email

            foreach (var item in gh)
            {
                orderdetail ctdh = new orderdetail();
                ctdh.order_id = dh.order_id;
                ctdh.book_id = item.book_id;
                ctdh.quantity = item.iquantity;
                ctdh.price = (decimal)item.price;

                tongTienFormail += item.Total;

                s = db.books.Single(n => n.book_id == item.book_id);
                s.quantity_instock -= ctdh.quantity;

                db.orderdetails.InsertOnSubmit(ctdh);
            }

            db.SubmitChanges();

            // ==========================================
            // TRIỂN KHAI TÍNH NĂNG 11 (ĐỎ): GỬI EMAIL TỰ ĐỘNG
            // ==========================================
            if (kh != null && !string.IsNullOrEmpty(kh.email))
            {
                SendOrderEmail(kh.email, kh.customer_name, dh.order_id, tongTienFormail);
            }

            // Xóa sạch giỏ hàng sau khi mua xong
            Session["Cart"] = null;

            return RedirectToAction("ConfirmOrder", "Cart");
        }

        // Hàm hỗ trợ logic gửi Email qua SMTP Server của Gmail (Đã cập nhật sửa lỗi bảo mật chặn thư)
        private void SendOrderEmail(string customerEmail, string customerName, int orderId, double totalAmount)
        {
            try
            {
                // 1. Cấu hình tài khoản gửi Gmail của bạn
                string fromEmail = "ouyu96502@gmail.com";
                string appPassword = "yqakyltnmpojgyuc"; // Mật khẩu ứng dụng 16 ký tự viết liền

                // 2. Tạo nội dung Email dạng HTML
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromEmail, "Hutech BookStore");
                mail.To.Add(customerEmail); // Gửi tới Gmail khách
                mail.Subject = "Order Confirmation #" + orderId;

                mail.Body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
                        <h2 style='color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px;'>Hutech BookStore Confirmation</h2>
                        <p>Dear <strong>{customerName}</strong>,</p>
                        <p>Thank you for purchasing at our library. Your order has been registered successfully!</p>
                        <table style='width: 100%; border-collapse: collapse; margin-top: 15px;'>
                            <tr style='background-color: #f2f2f2;'>
                                <th style='padding: 8px; text-align: left; border: 1px solid #ddd;'>Order Info</th>
                                <th style='padding: 8px; text-align: left; border: 1px solid #ddd;'>Details</th>
                            </tr>
                            <tr>
                                <td style='padding: 8px; border: 1px solid #ddd;'>Order ID</td>
                                <td style='padding: 8px; border: 1px solid #ddd;'><strong>#{orderId}</strong></td>
                            </tr>
                            <tr>
                                <td style='padding: 8px; border: 1px solid #ddd;'>Total Payment</td>
                                <td style='padding: 8px; border: 1px solid #ddd; color: #e74c3c; font-weight: bold;'>{string.Format("{0:0,0} đ", totalAmount)}</td>
                            </tr>
                        </table>
                        <p style='margin-top: 20px;'>Our delivery team will contact you soon via your number phone.</p>
                        <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'/>
                        <p style='font-size: 12px; color: #7f8c8d; text-align: center;'>&copy; {DateTime.Now.Year} Hutech BookStore. All rights reserved.</p>
                    </div>";
                mail.IsBodyHtml = true;

                // 3. Thiết lập kết nối SMTP mã hóa nâng cao của Gmail
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(fromEmail, appPassword);

                // Ép hệ thống Local mở các giao thức bảo mật bắt buộc (Tls12) khi truyền file đi dữ liệu ngầm lên Google Server
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                // Tiến hành bắn mail đi ngầm
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                // Ghi nhận lỗi chi tiết ra cửa sổ Output để Debug nếu có sự cố mạng
                System.Diagnostics.Debug.WriteLine("Lỗi gửi Email thực tế: " + ex.Message);
            }
        }

        // 13. [GET] Trang hiển thị thông báo đặt hàng thành công
        public ActionResult ConfirmOrder()
        {
            return View();
        }

        // 14. [GET] Chức năng Thống kê số lượng sách bán được
        public ActionResult Statistic()
        {
            var statisticResult = db.orderdetails
                                    .GroupBy(ct => ct.book.book_name)
                                    .Select(g => new StatisticDTO
                                    {
                                        BookName = g.Key,
                                        SaleQuantity = g.Sum(ct => ct.quantity)
                                    }).ToList();

            return View(statisticResult);
        }
    }
}