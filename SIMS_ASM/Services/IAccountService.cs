using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public interface IAccountService
    {
        Task<User> AuthenticateAsync(string username, string password);
        Task<User> RegisterAsync(User user);
    }
}
