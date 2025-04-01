using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly ApplicationDbContex _context;
        private readonly IStudentClassService _studentClassService;

        public EnrollmentService(ApplicationDbContex context, IStudentClassService studentClassService)
        {
            _context = context;
            _studentClassService = studentClassService;
        }

        // Lấy tất cả các đăng ký học phần từ database
        public async Task<IEnumerable<Enrollment>> GetAllEnrollmentsAsync()
        {
            return await _context.Enrollments
                .Include(e => e.User) // Bao gồm thông tin người dùng
                .Include(e => e.ClassCourseFaculty) // Bao gồm thông tin lớp học phần
                    .ThenInclude(ccf => ccf.Class) // Bao gồm thông tin lớp
                .Include(e => e.ClassCourseFaculty)
                    .ThenInclude(ccf => ccf.Course) // Bao gồm thông tin môn học
                .Include(e => e.ClassCourseFaculty)
                    .ThenInclude(ccf => ccf.User) // Bao gồm thông tin giảng viên
                .ToListAsync();
        }

        // Lấy thông tin đăng ký học phần theo ID
        public async Task<Enrollment> GetEnrollmentByIdAsync(int id)
        {
            return await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.ClassCourseFaculty)
                    .ThenInclude(ccf => ccf.Class)
                .Include(e => e.ClassCourseFaculty)
                    .ThenInclude(ccf => ccf.Course)
                .Include(e => e.ClassCourseFaculty)
                    .ThenInclude(ccf => ccf.User)
                .FirstOrDefaultAsync(e => e.EnrollmentID == id);
        }

        // Thêm mới một đăng ký học phần
        public async Task AddEnrollmentAsync(Enrollment enrollment)
        {
            // Kiểm tra sinh viên đã đăng ký học phần này chưa
            if (await IsStudentAlreadyEnrolledAsync(enrollment.UserID, enrollment.ClassCourseFacultyID))
            {
                throw new InvalidOperationException("Student is already enrolled in this class and course.");
            }

            // Lấy thông tin lớp học phần và sinh viên
            var classCourseFaculty = await _context.ClassCourseFaculties
                .Include(ccf => ccf.Class)
                .FirstOrDefaultAsync(ccf => ccf.ClassCourseFacultyID == enrollment.ClassCourseFacultyID);
            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == enrollment.UserID);

            // Kiểm tra tính hợp lệ của thông tin
            if (classCourseFaculty == null || student == null || student.Role != "Student")
            {
                throw new InvalidOperationException("Invalid student or ClassCourseFaculty.");
            }

            // Kiểm tra xem sinh viên có được gán vào lớp nào không
            var studentClassIds = await _studentClassService.GetClassIdsByStudentAsync(student.UserID);
            if (!studentClassIds.Any())
            {
                throw new InvalidOperationException($"Student (UserID: {student.UserID}) has not been assigned to any class. Please assign the student to a class first.");
            }

            // Kiểm tra sinh viên có thuộc lớp của học phần không
            if (!studentClassIds.Contains(classCourseFaculty.ClassID))
            {
                var studentClassesLog = string.Join(", ", studentClassIds);
                throw new InvalidOperationException(
                    $"Student (UserID: {student.UserID}) does not belong to the class (ClassID: {classCourseFaculty.ClassID}) associated with this ClassCourseFaculty. " +
                    $"Student's classes: [{studentClassesLog}]");
            }

            // Thiết lập ngày đăng ký và lưu vào database
            enrollment.EnrollmentDate = DateTime.Now;
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
        }

        // Cập nhật thông tin đăng ký học phần
        public async Task UpdateEnrollmentAsync(Enrollment enrollment)
        {
            // Lấy thông tin lớp học phần và sinh viên
            var classCourseFaculty = await _context.ClassCourseFaculties
                .Include(ccf => ccf.Class)
                .FirstOrDefaultAsync(ccf => ccf.ClassCourseFacultyID == enrollment.ClassCourseFacultyID);
            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == enrollment.UserID);

            // Kiểm tra tính hợp lệ của thông tin
            if (classCourseFaculty == null || student == null || student.Role != "Student")
            {
                throw new InvalidOperationException("Invalid student or ClassCourseFaculty.");
            }

            // Kiểm tra lớp của sinh viên
            var studentClassIds = await _studentClassService.GetClassIdsByStudentAsync(student.UserID);
            if (!studentClassIds.Any())
            {
                throw new InvalidOperationException($"Student (UserID: {student.UserID}) has not been assigned to any class. Please assign the student to a class first.");
            }

            // Kiểm tra sinh viên có thuộc lớp của học phần không
            if (!studentClassIds.Contains(classCourseFaculty.ClassID))
            {
                var studentClassesLog = string.Join(", ", studentClassIds);
                throw new InvalidOperationException(
                    $"Student (UserID: {student.UserID}) does not belong to the class (ClassID: {classCourseFaculty.ClassID}) associated with this ClassCourseFaculty. " +
                    $"Student's classes: [{studentClassesLog}]");
            }

            // Cập nhật thông tin và lưu vào database
            _context.Entry(enrollment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Xóa một đăng ký học phần
        public async Task DeleteEnrollmentAsync(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
            }
        }

        // Kiểm tra sinh viên đã đăng ký học phần này chưa
        public async Task<bool> IsStudentAlreadyEnrolledAsync(int userId, int classCourseFacultyId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.UserID == userId && e.ClassCourseFacultyID == classCourseFacultyId);
        }

        // Kiểm tra đăng ký học phần có điểm số liên quan không
        public async Task<bool> HasAssociatedGradesAsync(int enrollmentId)
        {
            return await _context.Grades.AnyAsync(g => g.EnrollmentID == enrollmentId);
        }

        // Lấy danh sách đăng ký học phần theo UserID
        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByUserIdAsync(int userId)
        {
            return await _context.Enrollments
                .Include(e => e.ClassCourseFaculty)
                    .ThenInclude(cc => cc.Class) // Bao gồm thông tin lớp
                .Include(e => e.ClassCourseFaculty)
                    .ThenInclude(cc => cc.Course) // Bao gồm thông tin môn học
                .Include(e => e.ClassCourseFaculty)
                    .ThenInclude(cc => cc.User) // Bao gồm thông tin giảng viên
                .Where(e => e.UserID == userId)
                .ToListAsync();
        }
    }
}