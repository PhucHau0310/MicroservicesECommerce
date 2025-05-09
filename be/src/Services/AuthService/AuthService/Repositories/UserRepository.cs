using AuthService.Events;
using AuthService.Services.Interfaces;
using AuthService.Data;
using AuthService.Repositories.Interfaces;
using AuthService.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;
        private readonly IEmailService _emailService;  
        private readonly IAccountProducer _accountProducer;

        public UserRepository(AuthDbContext context, IEmailService emailService, IAccountProducer accountProducer)
        {
            _context = context;
            _emailService = emailService;
            _accountProducer = accountProducer;
        }

        public async Task<List<User>> GetUsers()
        {
            return await _context.Users.Include(u => u.UserProfile)
                         .Include(u => u.Roles)
                         .Include(u => u.RefreshTokens)
                         .ToListAsync();
        }

        public async Task<User> GetUserById(Guid userId)
        {
            if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));

            var user = await _context.Users.Include(u => u.UserProfile)
                         .Include(u => u.Roles)
                         .Include(u => u.RefreshTokens)
                         .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) throw new ArgumentNullException(nameof(user));
            return user;
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));

            var user = await _context.Users.Include(u => u.UserProfile)
                         .Include(u => u.Roles)
                         .Include(u => u.RefreshTokens)
                         .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) throw new ArgumentNullException(nameof(user));
            return user;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));

            var user = await _context.Users.Include(u => u.UserProfile)
                         .Include(u => u.Roles)
                         .Include(u => u.RefreshTokens)
                         .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) throw new ArgumentNullException(nameof(user));
            return user;
        }

        public bool DeleteUserById(Guid userId)
        {
            if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            return true;
        }

        public async Task<User> RegisterOrUpdateUser(string username, string email, string firstName,
                                        string lastName, string avatarUrl, int provider)
        {
            var existingUser = await _context.Users.Include(u => u.Roles)
                                                   .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null) return existingUser;

            var user = new User
            {
                Username = username,
                Email = email,
                ProviderType = provider == 2 ? ProviderType.Google : ProviderType.Facebook,
                Roles = new Role { RoleType = RoleType.User },
                IsEmailConfirmed = true,
                UserProfile = new UserProfile
                {
                    FirstName = firstName,
                    LastName = lastName,
                    AvatarUrl = avatarUrl
                }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> ConfirmEmailAsync(Guid userId, string token)
        {
            if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }

            if (user.EmailConfirmationToken == token)
            {
                user.IsEmailConfirmed = true;
                user.EmailConfirmationToken = null;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
        
        public User Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

            var user = _context.Users.Include(u => u.Roles)
                .FirstOrDefault(u => u.Email == email);

            if (user == null) throw new ArgumentNullException(nameof(user));

            if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        public async Task<bool> RegisterAsync(string email, string username, string firstName, string lastName, string password)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                return false;
            }
            var user = new User
            {
                Email = email,
                Username = username,
                ProviderType = ProviderType.Local,
                Roles = new Role { RoleType = RoleType.User },
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                UserProfile = new UserProfile
                {
                    FirstName = firstName,
                    LastName = lastName,
                },
                EmailConfirmationToken = Guid.NewGuid().ToString(),
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var confirmationLink = $"https://localhost:6003/api/auth/confirm-email?userId={user.Id}&token={user.EmailConfirmationToken}";

            var emailBody = $@"
            <h2>Confirm your registration email</h2>
            <p>Hi, {user.Username}</p>
            <p>Please click the link below to confirm your account:</p>
            <a href='{confirmationLink}'>Confirm email</a>";

            await _emailService.SendEmailAsync(user.Email, "Xác nhận đăng ký tài khoản", emailBody);
            return true;
        }

        public async Task SaveRefreshToken(Guid userId, string refreshToken)
        {
            if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(refreshToken)) throw new ArgumentNullException(nameof(refreshToken));

            var user = await _context.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return;
            }

            // Mark old refresh tokens as revoked
            foreach (var token in user.RefreshTokens)
            {
                token.IsRevoked = true;
            }

            user.RefreshTokens.Add(new Token
            {
                UserId = userId,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddDays(7),
                IsUsed = false,
                IsRevoked = false
            });

            await _context.SaveChangesAsync();
        }

        public async Task<Token> GetRefreshToken(Guid userId)
        {
            if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));
            
            var token = await _context.Tokens
                .Where(t => t.UserId == userId && t.Expiration > DateTime.UtcNow && !t.IsRevoked)
                .OrderByDescending(t => t.Expiration)
                .FirstOrDefaultAsync();

            //if (token == null) throw new Exception("Token not found");

            return token;
        }

        public async Task<Token> GetRefreshTokenByToken(string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));

            var tokenRes = await _context.Tokens
                    .Where(t => t.RefreshToken == token && !t.IsRevoked) // Get the token that is not revoked
                    .FirstOrDefaultAsync();

            if (tokenRes == null) throw new Exception("Token not found");
            return tokenRes;
        }

        public async Task RevokeRefreshToken(Token refreshToken)
        {
            if (refreshToken == null) throw new ArgumentNullException(nameof(refreshToken));

            refreshToken.IsRevoked = true;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ForgotPassword(string mail)
        {
            if (string.IsNullOrEmpty(mail)) throw new ArgumentNullException(nameof(mail));

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == mail);
            if (user == null) return false;

            var random = new Random();
            var confirmationCode = random.Next(1000, 9999).ToString();

            user.EmailConfirmationToken = confirmationCode;

            await _context.SaveChangesAsync();

            var code = $"{confirmationCode}";

            var emailBody = $@"
            <h2>Change password</h2>
            <p>Hi, {user.Username}</p>
            <p>Here is the code, enter it on the verification page to proceed to the next step: <b>{code}</b></p>";

            await _emailService.SendEmailAsync(user.Email, "Change password", emailBody);

            var message = new AccountEvent
            {
                UserId = user.Id,
                Message = "Please sign in Email account to get the code verify",
                CreatedAt = DateTime.Now,
            };

            // Send to RabbitMQ
            await _accountProducer.SendMessage(message);

            return true;
        }

        public async Task<bool> VerifyCode(string email, string code)
        {
            if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));
            if (string.IsNullOrEmpty(code)) throw new ArgumentNullException(nameof(code));

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email && u.EmailConfirmationToken == code);
            if (user == null)
            {
                return false;
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangePass(string email, string newPass)
        {
            if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));
            if (string.IsNullOrEmpty(newPass)) throw new Exception("New password is required");

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPass);
            await _context.SaveChangesAsync();

            var message = new AccountEvent
            {
                UserId = user.Id,
                Message = "Change password successfully.",
                CreatedAt = DateTime.Now,
            };

            // Send to RabbitMQ
            await _accountProducer.SendMessage(message);

            return true;
        }
    }
}
