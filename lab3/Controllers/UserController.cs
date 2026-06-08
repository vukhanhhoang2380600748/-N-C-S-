using lab3.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace lab3.Controllers
{
    public class UserController : Controller
    {
        dbBookStoreDataContext db = new dbBookStoreDataContext();

        // 1. GET: Hiển thị trang Đăng ký
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // 2. POST: Xử lý dữ liệu Đăng ký khi nhấn nút Submit
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
                c.customer_name = name;
                c.username = username;
                c.password = password;
                c.email = email;
                c.address = address;
                c.numberphone = numberphone;
                c.dob = DateTime.Parse(dob);

                db.customers.InsertOnSubmit(c);
                db.SubmitChanges();

                return RedirectToAction("Login");
            }
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
                Session["User"] = c; // Lưu vào Session["User"] theo đúng code của bạn
                return RedirectToAction("Home", "Book");
            }
            else
            {
                ViewBag.ThongBao = "Username or password is incorrect";
                return View();
            }
        }

        // ==========================================
        // 11. CUSTOMER PROFILE FEATURES (NÂNG CAO)
        // ==========================================

        // GET: User/Profile
        [HttpGet]
        public ActionResult Profile()
        {
            // Kiểm tra xem đã đăng nhập bằng Session["User"] chưa
            if (Session["User"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Lấy thông tin khách hàng từ Session["User"]
            var kh = (customer)Session["User"];

            // Truy vấn lấy dữ liệu mới nhất từ database
            var customerDb = db.customers.SingleOrDefault(n => n.customer_id == kh.customer_id);

            if (customerDb == null)
            {
                return HttpNotFound();
            }

            // Đổ dữ liệu vào ProfileViewModel
            // (Bạn kiểm tra lại các thuộc tính trong ProfileViewModel.cs xem viết hoa/thường giống thế này chưa nhé)
            ProfileViewModel model = new ProfileViewModel
            {
                Username = customerDb.username,
                Password = customerDb.password,
                CustomerName = customerDb.customer_name,
                Email = customerDb.email,
                Phone = customerDb.numberphone, // Khớp với trường numberphone trong DB của bạn
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
                    // Tiến hành cập nhật thông tin mới từ form vào database
                    customerDb.password = model.Password;
                    customerDb.customer_name = model.CustomerName;
                    customerDb.email = model.Email;
                    customerDb.numberphone = model.Phone; // Gán lại vào cột numberphone
                    customerDb.address = model.Address;

                    // Lưu thay đổi xuống SQL Server
                    db.SubmitChanges();

                    // Cập nhật lại Session để hiển thị thông tin mới nhất trên Layout chung
                    Session["User"] = customerDb;

                    ViewBag.ThongBao = "Update profile successfully!";
                }
            }
            return View(model);
        }
    }
}