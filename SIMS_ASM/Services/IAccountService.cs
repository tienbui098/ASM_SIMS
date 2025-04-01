using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public interface IAccountService
    {
        // Xác thực người dùng với username và password
        // Trả về thông tin user nếu xác thực thành công
        Task<User> AuthenticateAsync(string username, string password);

        // Đăng ký tài khoản người dùng mới
        // Trả về thông tin user đã được tạo
        Task<User> RegisterAsync(User user);

        // Đăng xuất người dùng hiện tại
        Task LogoutAsync();

        // Kiểm tra xem có người dùng nào đang đăng nhập không
        // Trả về true nếu có người dùng đã xác thực
        bool IsAuthenticated();
    }
}