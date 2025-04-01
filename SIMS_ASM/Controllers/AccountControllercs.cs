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
        private readonly IAccountService _accountService;
        private readonly AccountSingleton _singleton;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
            _singleton = AccountSingleton.Instance;
        }

        //public AccountController()
        //{
        //}

        // Hiển thị form đăng nhập
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.SystemName = "Student Information Management System";
            return View();
        }

        // Xử lý đăng nhập
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Username and password are required.");
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

            var user = await _accountService.AuthenticateAsync(username, password);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                _singleton.Log($"Failed login attempt for user {username}");
                return View();
            }


            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Username", user.Username); // thiết lập này khi đăng nhập thành công
            _singleton.Log($"User {username} logged in with role {user.Role}");

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
            {
                _singleton.Log($"Failed registration attempt: Invalid data for {user.Username}");
                ViewBag.SystemName = "Student Information Management System";
                return View(user);
            }

            // Gán cứng role là "Student"
            user.Role = "Student";

            if (!Regex.IsMatch(user.Username, "^[a-z0-9]+$"))
            {
                ModelState.AddModelError("Username", "Username can only contain lowercase letters and numbers.");
                _singleton.Log($"Failed registration attempt: Invalid username format {user.Username}");
                ViewBag.SystemName = "Student Information Management System";
                return View(user);
            }

            try
            {
                await _accountService.RegisterAsync(user);
                _singleton.Log($"User {user.Username} registered successfully with role {user.Role}");
                // Gửi thông báo thành công
                ViewBag.SuccessMessage = "Registration successful!";

                // Trả về view nhưng giữ nguyên trang đăng ký
                return RedirectToAction("Index", "Account");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                _singleton.Log($"Failed registration attempt: {ex.Message}");
                ViewBag.SystemName = "Student Information Management System";
                return View(user);
            }
            catch (Exception ex)
            {
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


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            return RedirectToAction("Index", "Account");
        }
    }
}
