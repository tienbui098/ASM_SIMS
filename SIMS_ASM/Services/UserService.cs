using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;

namespace SIMS_ASM.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContex _context; // DbContext để tương tác với database

        public UserService(ApplicationDbContex context)
        {
            _context = context; // Inject DbContext qua constructor
        }

        // Lấy thông tin người dùng theo ID
        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id); // Tìm kiếm theo khóa chính
        }

        // Cập nhật thông tin người dùng
        public async Task UpdateUserAsync(User user)
        {
            _context.Entry(user).State = EntityState.Modified; // Đánh dấu trạng thái Modified
            await _context.SaveChangesAsync(); // Lưu thay đổi vào database
        }

        // Thêm người dùng mới vào hệ thống
        public async Task AddUserAsync(User user, string role)
        {
            // Kiểm tra username chỉ chứa chữ thường và số
            if (!Regex.IsMatch(user.Username, "^[a-z0-9]+$"))
            {
                throw new ArgumentException("Username can only contain lowercase letters and numbers.");
            }

            // Kiểm tra username đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                throw new InvalidOperationException("Username already exists.");
            }

            // Kiểm tra email đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                throw new InvalidOperationException("Email already exists.");
            }

            // Gán vai trò và mã hóa mật khẩu
            user.Role = role; // Gán role được chỉ định
            user.Password = HashPassword(user.Password); // Mã hóa mật khẩu trước khi lưu

            // Thêm người dùng vào cơ sở dữ liệu
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        // Xóa người dùng theo ID
        public async Task<bool> DeleteUserAsync(int id)
        {
            // Tìm người dùng kèm theo các quan hệ liên quan
            var user = await _context.Users
               .Include(u => u.StudentClasses) // Load thông tin lớp học của sinh viên
               .Include(u => u.ClassCourseFaculties) // Load thông tin lớp giảng dạy của giảng viên
               .Include(u => u.Enrollments) // Load thông tin đăng ký học phần
               .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
                return false; // Không tìm thấy người dùng

            // Kiểm tra nếu người dùng có bất kỳ quan hệ nào
            if (user.StudentClasses.Any() || user.ClassCourseFaculties.Any() || user.Enrollments.Any())
                return false; // Không cho phép xóa nếu có dữ liệu liên quan

            try
            {
                _context.Users.Remove(user); // Xóa người dùng
                await _context.SaveChangesAsync(); // Lưu thay đổi
                return true; // Trả về true nếu xóa thành công
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu có
                Console.WriteLine($"Error deleting user: {ex.Message}");
                return false;
            }
        }

        // Lấy danh sách tất cả giảng viên
        public async Task<IEnumerable<User>> GetLecturersAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "Lecturer") // Lọc theo role Lecturer
                .ToListAsync();
        }

        // Lấy danh sách tất cả sinh viên
        public async Task<IEnumerable<User>> GetStudentsAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "Student") // Lọc theo role Student
                .ToListAsync();
        }

        // Hàm băm mật khẩu sử dụng SHA256
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password)); // Tính toán hash
                return Convert.ToBase64String(hashedBytes); // Chuyển đổi sang chuỗi base64
            }
        }

        // Tìm kiếm người dùng theo username
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}