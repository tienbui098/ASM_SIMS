﻿using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(int id);
        Task UpdateUserAsync(User user);
        Task AddUserAsync(User user, string role); // Thêm phương thức mới với role linh hoạt
        Task<bool> DeleteUserAsync(int id);
        Task<IEnumerable<User>> GetLecturersAsync(); // Thêm phương thức mới
        Task<IEnumerable<User>> GetStudentsAsync();
        Task<User> GetUserByUsernameAsync(string username); // Add this method
    }
}
