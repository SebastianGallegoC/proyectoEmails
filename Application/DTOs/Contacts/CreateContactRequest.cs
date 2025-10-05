using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreateContactRequest
    {
        [Required, StringLength(150)]
        public string Name { get; set; } = null!;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = null!;

        [StringLength(50)]
        public string? Phone { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public bool IsFavorite { get; set; }
        public bool IsBlocked { get; set; }
    }
}
