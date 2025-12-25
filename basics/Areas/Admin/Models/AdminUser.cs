using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace basics.Areas.Admin.Models
{
    public class AdminUser
    {
        public int Id { get; set; }
        [Required]
        public string userName { get; set; }
        [Required]
        public string passwordHash { get; set; } //Saklı şifre
    }
}
