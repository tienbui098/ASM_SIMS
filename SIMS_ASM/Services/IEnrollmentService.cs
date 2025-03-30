using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public interface IEnrollmentService
    {
        Task<IEnumerable<Enrollment>> GetAllEnrollmentsAsync();
        Task<Enrollment> GetEnrollmentByIdAsync(int id);
        Task AddEnrollmentAsync(Enrollment enrollment);
        Task UpdateEnrollmentAsync(Enrollment enrollment);
        Task DeleteEnrollmentAsync(int id);
        Task<bool> IsStudentAlreadyEnrolledAsync(int userId, int classCourseFacultyId);
        Task<bool> HasAssociatedGradesAsync(int enrollmentId);
    }
}
