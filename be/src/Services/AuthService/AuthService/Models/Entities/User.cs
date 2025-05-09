namespace AuthService.Models.Entities
{
    public enum ProviderType
    {
        Local = 1,
        Google = 2,
        Facebook = 3
    }

    public class User
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public string? PasswordHash { get; set; }
        public bool IsEmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }
        public bool IsActive { get; set; } = true;
        public required ProviderType ProviderType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(7);
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow.AddHours(7);

        // Relationships
        public Role? Roles { get; set; }
        public UserProfile? UserProfile { get; set; }
        public ICollection<Token> RefreshTokens { get; set; } = new List<Token>();
    }
}
