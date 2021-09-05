using System;
using System.ComponentModel.DataAnnotations;

namespace CoinService.Models
{
    public class Invoice
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string OrderId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public DateTime DateAndTime { get; set; }
        [Required]
        public bool IsUsed { get; set; }
    }
}
