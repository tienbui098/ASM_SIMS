using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public class StudentClassService : IStudentClassService
    {
        private readonly ApplicationDbContex _context;

        public StudentClassService(ApplicationDbContex context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StudentClass>> GetAllStudentClassesAsync()
        {
            return await _context.StudentClasses
                .Include(sc => sc.User)
                .Include(sc => sc.Class)
                .ToListAsync();
        }

        public async Task<StudentClass> GetStudentClassByIdAsync(int id)
        {
            return await _context.StudentClasses
                .Include(sc => sc.User)
                .Include(sc => sc.Class)
                .FirstOrDefaultAsync(sc => sc.StudentClassID == id);
        }

        public async Task AddStudentClassAsync(StudentClass studentClass)
        {
            if (await IsStudentAlreadyInClassAsync(studentClass.UserID, studentClass.ClassID))
            {
                throw new InvalidOperationException("Student is already assigned to this class.");
            }

            var user = await _context.Users.FindAsync(studentClass.UserID);
            if (user == null || user.Role != "Student")
            {
                throw new InvalidOperationException("User not found or not a student.");
            }

            var classEntity = await _context.Classes.FindAsync(studentClass.ClassID);
            if (classEntity == null)
            {
                throw new InvalidOperationException("Class not found.");
            }

            _context.StudentClasses.Add(studentClass);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStudentClassAsync(StudentClass studentClass)
        {
            _context.Entry(studentClass).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStudentClassAsync(int id)
        {
            var studentClass = await _context.StudentClasses.FindAsync(id);
            if (studentClass != null)
            {
                _context.StudentClasses.Remove(studentClass);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasAssociatedEnrollmentsAsync(int userId, int classId)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.ClassCourseFaculty)
                .Where(e => e.UserID == userId && e.ClassCourseFaculty.ClassID == classId)
                .AnyAsync();
            return enrollments;
        }

        public async Task<bool> IsStudentAlreadyInClassAsync(int userId, int classId)
        {
            return await _context.StudentClasses
                .AnyAsync(sc => sc.UserID == userId && sc.ClassID == classId);
        }

        public async Task<IEnumerable<int>> GetClassIdsByStudentAsync(int userId)
        {
            return await _context.StudentClasses
                .Where(sc => sc.UserID == userId)
                .Select(sc => sc.ClassID)
                .ToListAsync();
        }
    }
}
