using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public interface IGradeService
    {
        Task<IEnumerable<Grade>> GetAllGradesAsync();
        Task<Grade> GetGradeByIdAsync(int id);
        Task AddGradeAsync(Grade grade);
        Task UpdateGradeAsync(Grade grade);
        Task DeleteGradeAsync(int id);
        Task<bool> GradeExistsAsync(int id);
        Task<IEnumerable<Grade>> GetGradesByEnrollmentAsync(int enrollmentId);
    }
}

