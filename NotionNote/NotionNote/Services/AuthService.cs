using NotionNote.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotionNote.Services
{
    public class AuthService : IAuthService
    {
        private readonly NoteHubDbContext _context;

        public AuthService(NoteHubDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public User? Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var user = _context.Users
                .FirstOrDefault(u => u.Username == username && u.PasswordHash == password && u.IsActive);

            return user;
        }

        public User? Register(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            if (UsernameExists(username))
            {
                return null;
            }

            var newUser = new User
            {
                Username = username,
                PasswordHash = password,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return newUser;
        }

        public bool UsernameExists(string username)
        {
            return _context.Users.Any(u => u.Username == username);
        }

        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                return false;
            }

            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return false;
            }

            if (user.PasswordHash != oldPassword)
            {
                return false;
            }

            user.PasswordHash = newPassword;
            _context.SaveChanges();
            return true;
        }

        public User? GetUserById(int userId)
        {
            var user = _context.Users.Find(userId);
            return user != null && user.IsActive ? user : null;
        }
    }
}
