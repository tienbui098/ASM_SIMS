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

        public async Task AddGradeAsync(Grade grade)
        {
            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteGradeAsync(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade != null)
            {
                _context.Grades.Remove(grade);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> GradeExistsAsync(int id)
        {
            return await _context.Grades.AnyAsync(e => e.GradeID == id);
        }

        public async Task<IEnumerable<Grade>> GetAllGradesAsync()
        {
            return await _context.Grades
                .Include(g => g.Enrollment)
                    .ThenInclude(e => e.User)
                .Include(g => g.Enrollment)
                    .ThenInclude(e => e.ClassCourseFaculty)
                        .ThenInclude(ccf => ccf.Course)
                .ToListAsync();
        }

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

        public async Task<IEnumerable<Grade>> GetGradesByEnrollmentAsync(int enrollmentId)
        {
            return await _context.Grades
                .Where(g => g.EnrollmentID == enrollmentId)
                .ToListAsync();
        }

        public async Task UpdateGradeAsync(Grade grade)
        {
            _context.Update(grade);
            await _context.SaveChangesAsync();
        }
    }
}
