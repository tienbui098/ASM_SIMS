using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public interface IMajorService
    {
        // Lấy danh sách tất cả các ngành học từ hệ thống
        Task<IEnumerable<Major>> GetAllMajorsAsync();

        // Lấy thông tin chi tiết của một ngành học theo ID
        Task<Major> GetMajorByIdAsync(int id);

        // Tạo mới một ngành học trong hệ thống
        Task<Major> CreateMajorAsync(Major major);

        // Cập nhật thông tin của một ngành học
        Task UpdateMajorAsync(Major major);

        // Xóa một ngành học theo ID
        Task<bool> DeleteMajorAsync(int id);
    }
}