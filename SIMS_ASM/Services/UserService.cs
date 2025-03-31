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
        private readonly ApplicationDbContex _context;
        
        public UserService(ApplicationDbContex context)
        {
            _context = context;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

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
            user.Role = role;
            user.Password = HashPassword(user.Password);

            // Thêm người dùng vào cơ sở dữ liệu
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users
               .Include(u => u.StudentClasses)
               .Include(u => u.ClassCourseFaculties)
               .Include(u => u.Enrollments)
               .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
                return false;

            if (user.StudentClasses.Any() || user.ClassCourseFaculties.Any() || user.Enrollments.Any())
                return false; // Không xóa được nếu có liên kết

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error deleting user: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<User>> GetLecturersAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "Lecturer")
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetStudentsAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "Student")
                .ToListAsync();
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
