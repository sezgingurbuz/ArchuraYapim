using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace basics.Helpers
{
    /// <summary>
    /// Türkiye cep telefonu numarası formatını doğrular.
    /// Kabul edilen formatlar: 5XXXXXXXXX, +905XXXXXXXXX, 05XXXXXXXXX
    /// </summary>
    public class TurkishPhoneAttribute : ValidationAttribute
    {
        private static readonly Regex PhoneRegex = new Regex(
            @"^(\+90|0)?5[0-9]{9}$",
            RegexOptions.Compiled);

        public TurkishPhoneAttribute() : base("Geçerli bir Türkiye cep telefonu numarası giriniz (5XX XXX XX XX)")
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                // Null değerler [Required] attribute'u tarafından kontrol edilmeli
                return ValidationResult.Success;
            }

            var phoneNumber = value.ToString()!
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "");

            if (PhoneRegex.IsMatch(phoneNumber))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? "Geçerli bir Türkiye cep telefonu numarası giriniz");
        }

        /// <summary>
        /// Telefon numarasını +90 formatına normalize eder
        /// </summary>
        public static string Normalize(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return phoneNumber;

            var cleaned = phoneNumber
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "");

            // Eğer +90 ile başlıyorsa olduğu gibi döndür
            if (cleaned.StartsWith("+90"))
                return cleaned;

            // Eğer 0 ile başlıyorsa, 0'ı kaldırıp +90 ekle
            if (cleaned.StartsWith("0"))
                return "+9" + cleaned;

            // Sadece 5XXXXXXXXX formatındaysa +90 ekle
            if (cleaned.StartsWith("5") && cleaned.Length == 10)
                return "+90" + cleaned;

            return cleaned;
        }
    }
}
