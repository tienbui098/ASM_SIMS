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

        public async Task<IEnumerable<Enrollment>> GetAllEnrollmentsAsync()
        {
            return await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.ClassCourseFaculty)
                    .ThenInclude(ccf => ccf.Class)
                .Include(e => e.ClassCourseFaculty)
                    .ThenInclude(ccf => ccf.Course)
                .Include(e => e.ClassCourseFaculty)
                    .ThenInclude(ccf => ccf.User)
                .ToListAsync();
        }

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

        public async Task AddEnrollmentAsync(Enrollment enrollment)
        {
            if (await IsStudentAlreadyEnrolledAsync(enrollment.UserID, enrollment.ClassCourseFacultyID))
            {
                throw new InvalidOperationException("Student is already enrolled in this class and course.");
            }

            var classCourseFaculty = await _context.ClassCourseFaculties
                .Include(ccf => ccf.Class)
                .FirstOrDefaultAsync(ccf => ccf.ClassCourseFacultyID == enrollment.ClassCourseFacultyID);
            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == enrollment.UserID);

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

            // Kiểm tra xem sinh viên có thuộc lớp của ClassCourseFaculty không
            if (!studentClassIds.Contains(classCourseFaculty.ClassID))
            {
                var studentClassesLog = string.Join(", ", studentClassIds);
                throw new InvalidOperationException(
                    $"Student (UserID: {student.UserID}) does not belong to the class (ClassID: {classCourseFaculty.ClassID}) associated with this ClassCourseFaculty. " +
                    $"Student's classes: [{studentClassesLog}]");
            }

            enrollment.EnrollmentDate = DateTime.Now;
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEnrollmentAsync(Enrollment enrollment)
        {
            {
                var classCourseFaculty = await _context.ClassCourseFaculties
                    .Include(ccf => ccf.Class)
                    .FirstOrDefaultAsync(ccf => ccf.ClassCourseFacultyID == enrollment.ClassCourseFacultyID);
                var student = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserID == enrollment.UserID);

                if (classCourseFaculty == null || student == null || student.Role != "Student")
                {
                    throw new InvalidOperationException("Invalid student or ClassCourseFaculty.");
                }
                var studentClassIds = await _studentClassService.GetClassIdsByStudentAsync(student.UserID);
                if (!studentClassIds.Any())
                {
                    throw new InvalidOperationException($"Student (UserID: {student.UserID}) has not been assigned to any class. Please assign the student to a class first.");
                }

                if (!studentClassIds.Contains(classCourseFaculty.ClassID))
                {
                    var studentClassesLog = string.Join(", ", studentClassIds);
                    throw new InvalidOperationException(
                        $"Student (UserID: {student.UserID}) does not belong to the class (ClassID: {classCourseFaculty.ClassID}) associated with this ClassCourseFaculty. " +
                        $"Student's classes: [{studentClassesLog}]");
                }

                _context.Entry(enrollment).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteEnrollmentAsync(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsStudentAlreadyEnrolledAsync(int userId, int classCourseFacultyId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.UserID == userId && e.ClassCourseFacultyID == classCourseFacultyId);
        }

        public async Task<bool> HasAssociatedGradesAsync(int enrollmentId)
        {
            return await _context.Grades.AnyAsync(g => g.EnrollmentID == enrollmentId);
        }
    }
}
