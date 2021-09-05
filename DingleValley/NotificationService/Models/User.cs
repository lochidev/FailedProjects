using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models
{
    public class User
    {
        [Key]
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Token { get; set; }
    }
}
