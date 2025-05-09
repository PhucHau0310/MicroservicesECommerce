namespace AuthService.Models.Entities
{
    public enum RoleType
    {
        Admin = 1,
        User = 2
    }
    public class Role
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public required RoleType RoleType { get; set; }
        public User? User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(7);
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow.AddHours(7);
    }
}
