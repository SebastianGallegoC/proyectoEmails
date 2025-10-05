namespace Domain.Entities
{
    public class Contact
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;

        public string? Phone { get; set; }
        public string? Notes { get; set; }

        public bool IsFavorite { get; set; }
        public bool IsBlocked { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
