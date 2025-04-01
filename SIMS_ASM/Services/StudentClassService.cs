using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public class StudentClassService : IStudentClassService
    {
        private readonly ApplicationDbContex _context; // DbContext để tương tác với database

        public StudentClassService(ApplicationDbContex context)
        {
            _context = context; // Inject DbContext qua constructor
        }

        // Lấy toàn bộ danh sách quan hệ sinh viên - lớp học
        public async Task<IEnumerable<StudentClass>> GetAllStudentClassesAsync()
        {
            return await _context.StudentClasses
                .Include(sc => sc.User) // Load thông tin sinh viên
                .Include(sc => sc.Class) // Load thông tin lớp học
                .ToListAsync();
        }

        // Lấy thông tin quan hệ sinh viên - lớp học theo ID
        public async Task<StudentClass> GetStudentClassByIdAsync(int id)
        {
            return await _context.StudentClasses
                .Include(sc => sc.User)
                .Include(sc => sc.Class)
                .FirstOrDefaultAsync(sc => sc.StudentClassID == id);
        }

        // Thêm mới quan hệ sinh viên - lớp học
        public async Task AddStudentClassAsync(StudentClass studentClass)
        {
            // Kiểm tra sinh viên đã có trong lớp chưa
            if (await IsStudentAlreadyInClassAsync(studentClass.UserID, studentClass.ClassID))
            {
                throw new InvalidOperationException("Student is already assigned to this class.");
            }

            // Kiểm tra user có tồn tại và là sinh viên không
            var user = await _context.Users.FindAsync(studentClass.UserID);
            if (user == null || user.Role != "Student")
            {
                throw new InvalidOperationException("User not found or not a student.");
            }

            // Kiểm tra lớp học có tồn tại không
            var classEntity = await _context.Classes.FindAsync(studentClass.ClassID);
            if (classEntity == null)
            {
                throw new InvalidOperationException("Class not found.");
            }

            _context.StudentClasses.Add(studentClass);
            await _context.SaveChangesAsync();
        }

        // Cập nhật thông tin quan hệ sinh viên - lớp học
        public async Task UpdateStudentClassAsync(StudentClass studentClass)
        {
            _context.Entry(studentClass).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Xóa quan hệ sinh viên - lớp học
        public async Task DeleteStudentClassAsync(int id)
        {
            var studentClass = await _context.StudentClasses.FindAsync(id);
            if (studentClass != null)
            {
                _context.StudentClasses.Remove(studentClass);
                await _context.SaveChangesAsync();
            }
        }

        // Kiểm tra sinh viên có đăng ký học phần trong lớp không
        public async Task<bool> HasAssociatedEnrollmentsAsync(int userId, int classId)
        {
            return await _context.Enrollments
                .Include(e => e.ClassCourseFaculty)
                .Where(e => e.UserID == userId && e.ClassCourseFaculty.ClassID == classId)
                .AnyAsync();
        }

        // Kiểm tra sinh viên đã có trong lớp chưa
        public async Task<bool> IsStudentAlreadyInClassAsync(int userId, int classId)
        {
            return await _context.StudentClasses
                .AnyAsync(sc => sc.UserID == userId && sc.ClassID == classId);
        }

        // Lấy danh sách ID lớp của một sinh viên
        public async Task<IEnumerable<int>> GetClassIdsByStudentAsync(int userId)
        {
            return await _context.StudentClasses
                .Where(sc => sc.UserID == userId)
                .Select(sc => sc.ClassID)
                .ToListAsync();
        }

        // Thêm nhiều sinh viên vào lớp cùng lúc
        public async Task AddMultipleStudentsToClassAsync(List<int> studentIds, int classId)
        {
            // Lấy danh sách sinh viên đã có trong lớp
            var existingStudents = await _context.StudentClasses
                .Where(sc => sc.ClassID == classId)
                .Select(sc => sc.UserID)
                .ToListAsync();

            // Lọc ra các sinh viên chưa có trong lớp
            var newStudents = studentIds.Except(existingStudents).ToList();

            foreach (var studentId in newStudents)
            {
                if (await IsStudentAlreadyInClassAsync(studentId, classId))
                    continue;

                var studentClass = new StudentClass
                {
                    UserID = studentId,
                    ClassID = classId
                };
                _context.StudentClasses.Add(studentClass);
            }

            await _context.SaveChangesAsync();
        }

        // Lấy danh sách sinh viên trong một lớp
        public async Task<List<User>> GetStudentsInClassAsync(int classId)
        {
            return await _context.StudentClasses
                .Where(sc => sc.ClassID == classId)
                .Include(sc => sc.User)
                .Select(sc => sc.User)
                .ToListAsync();
        }

        // Xóa tất cả sinh viên khỏi một lớp
        public async Task RemoveAllStudentsFromClassAsync(int classId)
        {
            var studentsInClass = await _context.StudentClasses
                .Where(sc => sc.ClassID == classId)
                .ToListAsync();

            _context.StudentClasses.RemoveRange(studentsInClass);
            await _context.SaveChangesAsync();
        }
    }
}