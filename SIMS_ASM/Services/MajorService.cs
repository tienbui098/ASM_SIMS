using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public class MajorService : IMajorService
    {
        private readonly ApplicationDbContex _context; // DbContext để tương tác với database

        public MajorService(ApplicationDbContex context)
        {
            _context = context; // Inject DbContext thông qua constructor
        }

        // Lấy danh sách tất cả các ngành học
        public async Task<IEnumerable<Major>> GetAllMajorsAsync()
        {
            return await _context.Majors.ToListAsync(); // Sử dụng EF Core để lấy toàn bộ danh sách
        }

        // Lấy thông tin ngành học theo ID
        public async Task<Major> GetMajorByIdAsync(int id)
        {
            return await _context.Majors.FindAsync(id); // Tìm kiếm theo khóa chính
        }

        // Tạo mới một ngành học
        public async Task<Major> CreateMajorAsync(Major major)
        {
            _context.Majors.Add(major); // Thêm mới vào DbSet
            await _context.SaveChangesAsync(); // Lưu thay đổi vào database
            return major; // Trả về ngành học vừa tạo
        }

        // Cập nhật thông tin ngành học
        public async Task UpdateMajorAsync(Major major)
        {
            _context.Entry(major).State = EntityState.Modified; // Đánh dấu trạng thái Modified
            await _context.SaveChangesAsync(); // Lưu thay đổi vào database
        }

        // Xóa ngành học theo ID
        public async Task<bool> DeleteMajorAsync(int id)
        {
            // Tìm ngành học kèm theo các khóa học và lớp học liên quan
            var major = await _context.Majors
                .Include(m => m.Courses) // Load các khóa học liên quan
                .Include(m => m.Classes) // Load các lớp học liên quan
                .FirstOrDefaultAsync(m => m.MajorID == id);

            if (major == null)
                return false; // Không tìm thấy ngành học

            // Kiểm tra nếu ngành học đang có khóa học hoặc lớp học
            if (major.Courses.Any() || major.Classes.Any())
                return false; // Không cho phép xóa nếu có dữ liệu liên quan

            _context.Majors.Remove(major); // Xóa ngành học
            await _context.SaveChangesAsync(); // Lưu thay đổi
            return true; // Trả về true nếu xóa thành công
        }
    }
}