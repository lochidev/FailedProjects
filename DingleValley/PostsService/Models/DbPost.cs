using System.ComponentModel.DataAnnotations;

namespace PostsService.Models
{
    public class DbPost
    {
        [Key]
        public string PostId { get; init; }
        [Required]
        public string Category { get; init; }
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
        [Required]
        public DateTime PostedAt { get; init; }
        [Required]
        public string OwnerUserId { get; init; }
        [Required]
        public bool IsFromPremium { get; init; }
        [Required]
        public bool IsAd { get; init; }
        public int Clouts { get; set; }
        public PostConfiguration PostConfiguration { get; set; }
    }
}
