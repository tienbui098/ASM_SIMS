using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public class GradeService : IGradeService
    {
        private readonly ApplicationDbContex _context;

        public GradeService(ApplicationDbContex context)
        {
            _context = context;
        }

        // Thêm mới một điểm số
        public async Task AddGradeAsync(Grade grade)
        {
            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();
        }

        // Xóa một điểm số theo ID
        public async Task DeleteGradeAsync(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade != null)
            {
                _context.Grades.Remove(grade);
                await _context.SaveChangesAsync();
            }
        }

        // Kiểm tra xem điểm số có tồn tại không
        public async Task<bool> GradeExistsAsync(int id)
        {
            return await _context.Grades.AnyAsync(e => e.GradeID == id);
        }

        // Lấy tất cả điểm số từ database
        public async Task<IEnumerable<Grade>> GetAllGradesAsync()
        {
            return await _context.Grades
                .Include(g => g.Enrollment) // Bao gồm thông tin đăng ký
                    .ThenInclude(e => e.User) // Bao gồm thông tin người dùng
                .Include(g => g.Enrollment)
                    .ThenInclude(e => e.ClassCourseFaculty) // Bao gồm thông tin lớp học phần
                        .ThenInclude(ccf => ccf.Course) // Bao gồm thông tin môn học
                .ToListAsync();
        }

        // Lấy điểm số theo ID
        public async Task<Grade> GetGradeByIdAsync(int id)
        {
            return await _context.Grades
                .Include(g => g.Enrollment)
                    .ThenInclude(e => e.User)
                .Include(g => g.Enrollment)
                    .ThenInclude(e => e.ClassCourseFaculty)
                        .ThenInclude(ccf => ccf.Course)
                .FirstOrDefaultAsync(m => m.GradeID == id);
        }

        // Lấy danh sách điểm theo EnrollmentID
        public async Task<IEnumerable<Grade>> GetGradesByEnrollmentAsync(int enrollmentId)
        {
            return await _context.Grades
                .Where(g => g.EnrollmentID == enrollmentId) // Lọc theo EnrollmentID
                .ToListAsync();
        }

        // Cập nhật thông tin điểm số
        public async Task UpdateGradeAsync(Grade grade)
        {
            _context.Update(grade);
            await _context.SaveChangesAsync();
        }

        // Lấy danh sách điểm của sinh viên dựa trên UserID
        public async Task<IEnumerable<Grade>> GetGradesByUserId(int userId)
        {
            return await _context.Grades
                .Include(g => g.Enrollment) // Gồm thông tin đăng ký
                .ThenInclude(e => e.ClassCourseFaculty) // Gồm thông tin lớp học phần
                .ThenInclude(cc => cc.Course) // Gồm thông tin môn học
                .Include(g => g.Enrollment.User) // Gồm thông tin người dùng (sinh viên)
                .Where(g => g.Enrollment.User.UserID == userId) // Lọc theo UserID
                .ToListAsync();
        }
    }
}