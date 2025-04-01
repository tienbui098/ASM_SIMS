using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using SIMS_ASM.Singleton;
using System.Text.RegularExpressions;
using System.ComponentModel;
using SIMS_ASM.Services;

namespace SIMS_ASM.Controllers
{
    public class AccountController : Controller
    {
        // Dịch vụ xử lý logic tài khoản
        private readonly IAccountService _accountService;
        // Đối tượng Singleton để ghi log
        private readonly AccountSingleton _singleton;

        // Khởi tạo controller với dependency injection
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService; // Inject service
            _singleton = AccountSingleton.Instance; // Lấy instance của Singleton
        }

        //public AccountController()
        //{
        //}

        // Hiển thị form đăng nhập
        [HttpGet]
        public IActionResult Login()
        {
            //Đặt tên hệ thống cho ViewBag để hiển thị trên giao diện
            ViewBag.SystemName = "Student Information Management System";
            return View();
        }

        // Xử lý đăng nhập
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Kiểm tra nếu username hoặc password rỗng
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                //Thêm lỗi vào ModelState
                ModelState.AddModelError("", "Username and password are required.");
                //Ghi log lại lỗi   
                _singleton.Log($"Failed login attempt: Username or password is empty");
                return View();
            }

            // Kiểm tra username chỉ chứa chữ thường và số
            if (!Regex.IsMatch(username, "^[a-z0-9]+$"))
            {
                ModelState.AddModelError("", "Wrong username. PLease try again!");
                _singleton.Log($"Failed login attempt: Invalid username format ({username})");
                return View();
            }

            // Xác thực người dùng bằng dịch vụ AccountService
            var user = await _accountService.AuthenticateAsync(username, password);
            if (user == null)
            {
                //Thêm thông báo lỗi vào ModelState
                ModelState.AddModelError("", "Invalid login attempt.");
                _singleton.Log($"Failed login attempt for user {username}");
                return View();
            }

            // Lưu thông tin người dùng vào Session khi đăng nhập thành công
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Username", user.Username); // thiết lập này khi đăng nhập thành công
            _singleton.Log($"User {username} logged in with role {user.Role}");

            //chuyển hướng đến trang chính tương ứng với role
            if (user != null)
            {
                HttpContext.Session.SetString("UserID", user.UserID.ToString());
                switch (user.Role)
                {
                    case "Student":
                        return RedirectToAction("Index", "Student");
                    case "Lecturer":
                        return RedirectToAction("Index", "Lecturer");
                    case "Admin":
                        return RedirectToAction("Index", "Admin");
                    default:
                        return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                //khi role không được xác định, chuyển hướng về trang chủ
                return RedirectToAction("Index", "Home");
            }
        }


        // Hiển thị form đăng ký
        public IActionResult Register()
        {
            ViewBag.SystemName = "Student Information Management System";
            return View();
        }

        // Xử lý đăng ký
        [HttpPost]
        //Bảo vệ khỏi tấn công CSRF
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            // Kiểm tra tính hợp lệ của dữ liệu nhập vào
            if (!ModelState.IsValid)
            {
                //ghi kại lỗi vào log
                _singleton.Log($"Failed registration attempt: Invalid data for {user.Username}");
                //đặt lại tên hệ thống cho ViewBag
                ViewBag.SystemName = "Student Information Management System";
                return View(user); // Trả về view với dữ liệu đã nhập
            }
            // Gán cứng role là "Student"
            user.Role = "Student";
            // Kiểm tra username chỉ chứa chữ thường và số
            if (!Regex.IsMatch(user.Username, "^[a-z0-9]+$"))
            {
                // Thêm lỗi vào ModelState khi username không đúng với định dạng
                ModelState.AddModelError("Username", "Username can only contain lowercase letters and numbers.");
                _singleton.Log($"Failed registration attempt: Invalid username format {user.Username}");
                ViewBag.SystemName = "Student Information Management System";
                return View(user);
            }

            try
            {
                //thực hiện đăng ký người dùng qua AccountService
                await _accountService.RegisterAsync(user);
                _singleton.Log($"User {user.Username} registered successfully with role {user.Role}");
                // Gửi thông báo thành công
                ViewBag.SuccessMessage = "Registration successful!";
                // Trả về view nhưng giữ nguyên trang đăng ký
                return RedirectToAction("Index", "Account");
            }
            catch (InvalidOperationException ex)
            {
                // Xử lý lỗi khi username đã tồn tại hoặc lỗi cụ thể khác
                ModelState.AddModelError("", ex.Message);
                _singleton.Log($"Failed registration attempt: {ex.Message}");
                ViewBag.SystemName = "Student Information Management System";
                return View(user);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi không xác định
                _singleton.Log($"Failed to register user {user.Username}: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while registering.");
                ViewBag.SystemName = "Student Information Management System";
                return View(user);
            }

        }

        public IActionResult Index()
        {
            return View();
        }

        //xử lý đăng xuất
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Gọi phương thức đăng xuất từ dịch vụ
            await _accountService.LogoutAsync();
            //Chuyển hướng về trang login
            return RedirectToAction("Login", "Account");
        }
    }
}
