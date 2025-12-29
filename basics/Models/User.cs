using System.ComponentModel.DataAnnotations;

namespace basics.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad gereklidir")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad gereklidir")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin")]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Geçerli bir telefon numarası girin")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Tam ad için helper property
        public string FullName => $"{FirstName} {LastName}";
    }
}
