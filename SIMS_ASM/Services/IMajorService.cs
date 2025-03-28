using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public interface IMajorService
    {
        Task<IEnumerable<Major>> GetAllMajorsAsync();
        Task<Major> GetMajorByIdAsync(int id);
        Task<Major> CreateMajorAsync(Major major);
        Task UpdateMajorAsync(Major major);
        Task<bool> DeleteMajorAsync(int id); // Trả về bool để kiểm tra thành công/thất bại
    }
}
