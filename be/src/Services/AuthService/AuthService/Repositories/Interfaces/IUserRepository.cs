
using AuthService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetUsers();
        Task<User> GetUserById(Guid userId);
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByEmailAsync(string email);
        bool DeleteUserById(Guid userId);
        Task<User> RegisterOrUpdateUser(string username, string email, string firstName, 
                                        string lastName, string avatarUrl, int provider);
        Task<bool> ConfirmEmailAsync(Guid userId, string token);
        Task<bool> RegisterAsync(string email, string username, string firstName, string lastName, string password);
        User Authenticate(string email, string password);
        Task SaveRefreshToken(Guid userId, string refreshToken);
        Task<Token> GetRefreshToken(Guid userId);
        Task<Token> GetRefreshTokenByToken(string token);
        Task RevokeRefreshToken(Token refreshToken);
        Task<bool> ForgotPassword(string mail);
        Task<bool> VerifyCode(string email, string code);
        Task<bool> ChangePass(string email, string newPass);
    }
}
