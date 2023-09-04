using ARD_Grup_WebApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ARD_Grup_WebApi.Services
{
    public class UserService
    {
        private readonly ARD_DbContext _dbContext;

        public UserService(ARD_DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> IsUserValid(string email, string password)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
            return user != null;
        }

        public async Task<bool> IsEmailUnique(string email)
        {
            return await _dbContext.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> IsNameSurnameUnique(string nameSurname)
        {
            return await _dbContext.Users.AnyAsync(u => u.NameSurname == nameSurname);
        }

        public async Task<bool> CreateUser(User user)
        {
            try
            {
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<User> GetUserById(int id)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _dbContext.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserByName(string name)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.NameSurname == name);
        }

        public async Task<string?> GetUserFirstRole(int Id)
        {
            var us = await _dbContext.Users
                .Include(u => u.Roles) // Roller de dahil edilmeli
                .FirstOrDefaultAsync(x => x.Id == Id);

            if (us != null && us.Roles.Any()) // Roller varsa
                return us.Roles.First().Name;

            return null; // Roller yoksa veya kullanıcı yoksa null dönebilirsiniz
        }

        public async Task<int?> GetUserIdByEmail(string email)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user?.Id;
        }

    }
}
