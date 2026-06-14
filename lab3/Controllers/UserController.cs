using lab3.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Net; // Cần thiết để gửi mail
using System.Net.Mail; // Cần thiết để gửi mail

namespace lab3.Controllers
{
    public class UserController : Controller
    {
        dbBookStoreDataContext db = new dbBookStoreDataContext();

        // ==========================================
        // BƯỚC 1: HÀM GỬI EMAIL CHỨA MÃ OTP 
        // ==========================================
        private void SendVerificationEmail(string toEmail, string otpCode)
        {
            // Thiết lập email gửi đi (Thay bằng email cá nhân hoặc email dự án của bạn)
            var fromAddress = new MailAddress("ouyu96502@gmail.com", "Fahasa Hutech");
            var toAddress = new MailAddress(toEmail);

            // Mật khẩu ứng dụng Gmail (App Password) gồm 16 ký tự tạo từ bảo mật 2 lớp của tài khoản Google
            string fromPassword = "wgwd cppm qkxp ttzg";

            string subject = "[Fahasa Hutech] Mã Xác Minh Đăng Ký Tài Khoản";
            string body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eee; max-width: 500px; border-radius: 8px;'>
                    <h2 style='color: #cd1818;'>Chào mừng bạn đến với Fahasa Hutech!</h2>
                    <p>Cảm ơn bạn đã đăng ký. Đây là mã kích hoạt tài khoản của bạn:</p>
                    <div style='background: #f4f4f4; padding: 15px; text-align: center; font-size: 26px; font-weight: bold; letter-spacing: 5px; color: #cd1818; border-radius: 5px;'>
                        {otpCode}
                    </div>
                    <p style='margin-top: 20px; color: #777; font-size: 12px;'>Mã này sẽ hết hạn sau ít phút. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                </div>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body, IsBodyHtml = true })
            {
                smtp.Send(message);
            }
        }

        // 1. GET: Hiển thị trang Đăng ký
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // ==========================================
        // BƯỚC 2: CẬP NHẬT XỬ LÝ ĐĂNG KÝ (LƯU TẠM & GỬI OTP)
        // ==========================================
        [HttpPost]
        public ActionResult Register(FormCollection collection, customer c)
        {
            var name = collection["customer_name"];
            var username = collection["username"];
            var password = collection["password"];
            var confirmpassword = collection["confirmpassword"];
            var email = collection["email"];
            var address = collection["address"];
            var numberphone = collection["numberphone"];
            var dob = String.Format("{0:MM/dd/yyyy}", collection["dob"]);

            // Kiểm tra xem trùng email hoặc trùng username trong DB chưa trước khi gửi OTP
            var checkUsername = db.customers.FirstOrDefault(u => u.username == username);
            var checkEmail = db.customers.FirstOrDefault(u => u.email == email);

            if (checkUsername != null)
            {
                ViewData["Error"] = "Username này đã được sử dụng!";
                return View();
            }
            if (checkEmail != null)
            {
                ViewData["Error"] = "Email này đã được sử dụng!";
                return View();
            }

            if (String.IsNullOrEmpty(confirmpassword))
            {
                ViewData["enterpassword"] = "Must enter password to confirm!";
            }
            else if (!password.Equals(confirmpassword))
            {
                ViewData["samepassword"] = "Password and confirmation password must be the same";
            }
            else
            {
                // Gán dữ liệu vào đối tượng c
                c.customer_name = name;
                c.username = username;
                c.password = password;
                c.email = email;
                c.address = address;
                c.numberphone = numberphone;
                c.dob = DateTime.Parse(dob);

                try
                {
                    // 1. Sinh ngẫu nhiên mã OTP gồm 6 chữ số
                    Random rand = new Random();
                    string otpCode = rand.Next(100000, 999999).ToString();

                    // 2. Gửi mã OTP về hòm thư Gmail của khách hàng
                    SendVerificationEmail(c.email, otpCode);

                    // 3. Lưu giữ tạm thời đối tượng dữ liệu khách hàng và mã OTP vào Session
                    Session["TempRegisterModel"] = c;
                    Session["GeneratedOTP"] = otpCode;

                    // 4. Chuyển hướng người dùng sang trang nhập mã xác nhận OTP
                    return RedirectToAction("VerifyOTP");
                }
                catch (Exception ex)
                {
                    ViewData["Error"] = "Không thể gửi email xác minh. Lỗi: " + ex.Message;
                    return View();
                }
            }
            return View();
        }

        // ==========================================
        // BƯỚC 3: CÁC ACTION XỬ LÝ MÃ XÁC THỰC OTP
        // ==========================================

        // GET: User/VerifyOTP
        [HttpGet]
        public ActionResult VerifyOTP()
        {
            // Nếu không có dữ liệu đăng ký tạm trong hàng đợi thì đẩy ngược về trang Đăng ký
            if (Session["TempRegisterModel"] == null || Session["GeneratedOTP"] == null)
            {
                return RedirectToAction("Register");
            }
            return View();
        }

        // POST: User/VerifyOTP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyOTP(string enteredOTP)
        {
            var c = Session["TempRegisterModel"] as customer;
            string correctOTP = Session["GeneratedOTP"]?.ToString();

            if (c == null || string.IsNullOrEmpty(correctOTP))
            {
                return RedirectToAction("Register");
            }

            // Kiểm tra xem mã OTP người dùng nhập vào form có khớp không
            if (enteredOTP == correctOTP)
            {
                try
                {
                    // NẾU KHỚP: Lưu thông tin chính thức xuống SQL Server dữ liệu y như cũ
                    db.customers.InsertOnSubmit(c);
                    db.SubmitChanges();

                    // Xóa sạch bộ nhớ tạm Session sau khi đăng ký thành công
                    Session.Remove("TempRegisterModel");
                    Session.Remove("GeneratedOTP");

                    // Tạo thông báo thành công hiển thị ở trang Login
                    TempData["ThongBaoSuccess"] = "Đăng ký tài khoản thành công!";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Có lỗi xảy ra trong quá trình lưu dữ liệu: " + ex.Message;
                    return View();
                }
            }

            // NẾU SAI MÃ: Trả thông báo lỗi ra ngoài View
            ViewBag.Error = "Mã OTP nhập vào không đúng. Vui lòng kiểm tra lại hòm thư email.";
            return View();
        }

        // GET: Hiển thị form Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // POST: Xử lý dữ liệu Login
        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            var username = collection["username"];
            var password = collection["password"];

            customer c = db.customers.SingleOrDefault(n => n.username == username && n.password == password);
            if (c != null)
            {
                ViewBag.ThongBao = "Congratulations on successful login!";
                Session["User"] = c;
                return RedirectToAction("Home", "Book");
            }
            else
            {
                ViewBag.ThongBao = "Username or password is incorrect";
                return View();
            }
        }

        // ==========================================
        // ĐÃ BỔ SUNG: ACTION XỬ LÝ ĐĂNG XUẤT (LOGOUT)
        // ==========================================
        [HttpGet]
        public ActionResult Logout()
        {
            // Xóa bỏ thông tin tài khoản đang đăng nhập trong Session
            Session["User"] = null;

            // Đẩy người dùng quay trở lại trang danh sách sản phẩm công khai
            return RedirectToAction("Index", "Book");
        }

        // ==========================================
        // 11. CUSTOMER PROFILE FEATURES (NÂNG CAO)
        // ==========================================

        // GET: User/Profile
        [HttpGet]
        public ActionResult Profile()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            var kh = (customer)Session["User"];
            var customerDb = db.customers.SingleOrDefault(n => n.customer_id == kh.customer_id);

            if (customerDb == null)
            {
                return HttpNotFound();
            }

            ProfileViewModel model = new ProfileViewModel
            {
                Username = customerDb.username,
                Password = customerDb.password,
                CustomerName = customerDb.customer_name,
                Email = customerDb.email,
                Phone = customerDb.numberphone,
                Address = customerDb.address
            };

            return View(model);
        }

        // POST: User/Profile
        [HttpPost]
        public ActionResult Profile(ProfileViewModel model)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            if (ModelState.IsValid)
            {
                var kh = (customer)Session["User"];
                var customerDb = db.customers.SingleOrDefault(n => n.customer_id == kh.customer_id);

                if (customerDb != null)
                {
                    customerDb.password = model.Password;
                    customerDb.customer_name = model.CustomerName;
                    customerDb.email = model.Email;
                    customerDb.numberphone = model.Phone;
                    customerDb.address = model.Address;

                    db.SubmitChanges();
                    Session["User"] = customerDb;

                    ViewBag.ThongBao = "Update profile successfully!";
                }
            }
            return View(model);
        }
    }
}