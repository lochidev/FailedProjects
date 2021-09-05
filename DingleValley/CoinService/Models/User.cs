using System;
using System.ComponentModel.DataAnnotations;

namespace CoinService.Models
{
    public class User
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public int Coins { get; set; }
        public bool IsPremium { get; set; }
        public bool IsPartner { get; set; }
        public DateTime MadePremiumAt { get; set; }
    }
}
