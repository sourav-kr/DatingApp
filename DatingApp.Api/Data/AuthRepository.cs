using System;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Api.Data
{
    public class AuthRepository : IAuthRepository
    {

        private readonly DataContext _context;
        public AuthRepository(DataContext context)
        {
            _context = context;
        }
        public async Task<User> Login(string userName, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == userName);

            if (user is null)
                return null;

            if (!VerifyUserPasswordHash(user, password))
                return null;
            return user;
        }

        private bool VerifyUserPasswordHash(User user, string password)
        {

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                hmac.Key = user.PasswordSalt;
                var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computeHash.Length; i++)
                {
                    if (computeHash[i] != user.PasswordHash[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExists(string userName)
        {
            return await _context.Users.AnyAsync(x => x.Username == userName);
        }
    }
}