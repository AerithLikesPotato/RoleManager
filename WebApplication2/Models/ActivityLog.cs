using System;

namespace WebApplication2.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public int? ActorUserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public User? Actor { get; set; }
    }
}
