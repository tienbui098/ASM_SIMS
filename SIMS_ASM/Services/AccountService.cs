using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using System.Security.Cryptography;
using System.Text;

namespace SIMS_ASM.Services
{
    public class AccountService : IAccountService
    {
        // Khai báo các dependency cần thiết
        private readonly ApplicationDbContex _context; 
        private readonly IHttpContextAccessor _httpContextAccessor; 

        // Constructor: Inject context và httpContextAccessor (đã giải thích ở các controller trước)
        public AccountService(ApplicationDbContex context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // Constructor mặc định bị comment
        //public AccountService()
        //{
        //}

        // Phương thức xác thực người dùng
        public async Task<User> AuthenticateAsync(string username, string password)
        {
            // Băm mật khẩu đầu vào để so sánh với mật khẩu đã lưu
            string hashedPassword = HashPassword(password);

            // Tìm người dùng trong cơ sở dữ liệu với username và mật khẩu đã băm
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == hashedPassword);
            // Trả về null nếu không tìm thấy
        }

        // Phương thức đăng ký người dùng mới
        public async Task<User> RegisterAsync(User user)
        {
            // Kiểm tra username đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                throw new InvalidOperationException("Username already exists"); // Ném lỗi nếu trùng

            // Kiểm tra email đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                throw new InvalidOperationException("Email already exists"); // Ném lỗi nếu trùng

            // Băm mật khẩu trước khi lưu
            user.Password = HashPassword(user.Password);

            // Thêm người dùng mới vào cơ sở dữ liệu
            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // Lưu thay đổi

            return user; // Trả về thông tin người dùng vừa đăng ký
        }

        // Phương thức đăng xuất
        public Task LogoutAsync()
        {
            // Xóa tất cả thông tin trong session để đăng xuất
            _httpContextAccessor.HttpContext.Session.Clear();

            return Task.CompletedTask; // Trả về task hoàn thành (không có tác vụ bất đồng bộ)
        }

        // Phương thức kiểm tra trạng thái xác thực
        public bool IsAuthenticated()
        {
            // Kiểm tra session có chứa UserID hay không để xác định người dùng đã đăng nhập
            return _httpContextAccessor.HttpContext.Session.GetString("UserID") != null;
        }

        // Phương thức băm mật khẩu (private helper)
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create()) // Sử dụng thuật toán SHA256 để băm
            {
                // Chuyển đổi mật khẩu thành mảng byte UTF-8 và băm
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Chuyển mảng byte thành chuỗi Base64
                var base64 = Convert.ToBase64String(hashedBytes);

                // Giới hạn độ dài chuỗi Base64 tối đa 50 ký tự (phù hợp với VARCHAR(50) trong User)
                return base64.Length > 50 ? base64.Substring(0, 50) : base64;
            }
        }
    }
}