namespace ECommerce.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public enum UserRole
    {
        User,
        Admin
    }
}
