﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using System.Security.Cryptography;
using System.Text;

namespace SIMS_ASM.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContex _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(ApplicationDbContex context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        //public AccountService()
        //{

        //}

        public async Task<User> AuthenticateAsync(string username, string password)
        {
            string hashedPassword = HashPassword(password);
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == hashedPassword);
        }

        public async Task<User> RegisterAsync(User user)
        {
            // Kiểm tra username duy nhất
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                throw new InvalidOperationException("Username already exists");

            // Kiểm tra email duy nhất
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                throw new InvalidOperationException("Email already exists");

            user.Password = HashPassword(user.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public Task LogoutAsync()
        {
            // Xóa tất cả các session
            _httpContextAccessor.HttpContext.Session.Clear();

            return Task.CompletedTask;
        }

        public bool IsAuthenticated()
        {
            // Kiểm tra session có chứa thông tin người dùng hay không
            return _httpContextAccessor.HttpContext.Session.GetString("UserID") != null;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var base64 = Convert.ToBase64String(hashedBytes);
                return base64.Length > 50 ? base64.Substring(0, 50) : base64; // Giới hạn VARCHAR(50)
            }
        }
    }
}
