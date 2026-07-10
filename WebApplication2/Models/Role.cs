namespace WebApplication2.Models
{
    public class Role
    {
        public int ID { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string? Permissions { get; set; }
        public List<User> Users { get; set; } = new();
    }
}
