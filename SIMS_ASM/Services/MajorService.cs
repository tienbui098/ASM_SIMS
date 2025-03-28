using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public class MajorService : IMajorService
    {
        private readonly ApplicationDbContex _context;

        public MajorService(ApplicationDbContex context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Major>> GetAllMajorsAsync()
        {
            return await _context.Majors.ToListAsync();
        }

        public async Task<Major> GetMajorByIdAsync(int id)
        {
            return await _context.Majors.FindAsync(id);
        }

        public async Task<Major> CreateMajorAsync(Major major)
        {
            _context.Majors.Add(major);
            await _context.SaveChangesAsync();
            return major;
        }

        public async Task UpdateMajorAsync(Major major)
        {
            _context.Entry(major).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteMajorAsync(int id)
        {
            var major = await _context.Majors
                .Include(m => m.Courses)
                .Include(m => m.Classes)
                .FirstOrDefaultAsync(m => m.MajorID == id);

            if (major == null)
                return false;

            if (major.Courses.Any() || major.Classes.Any())
                return false; // Không xóa được nếu có liên kết

            _context.Majors.Remove(major);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
