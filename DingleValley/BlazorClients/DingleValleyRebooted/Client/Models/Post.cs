using System.ComponentModel.DataAnnotations;

namespace DingleValleyRebooted.Client.Models
{
    public class Post
    {

        public string PostId { get; set; }

        [Required(ErrorMessage = "This is required")]
        [MaxLength(75, ErrorMessage = "Title is too long. Must be shorter than 75 characters")]
        [MinLength(10, ErrorMessage = "Title is too short. Must be longer than 10 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "This is required")]
        [MaxLength(25, ErrorMessage = "Category name is too long. Must be shorter than 25 characters")]
        [MinLength(4, ErrorMessage = "Category name is too short. Must be longer than 4 characters")]
        public string Category { get; set; }

        [Required(ErrorMessage = "This is required")]
        [MaxLength(500, ErrorMessage = "Content is too long. Must be shorter than 500 characters")]
        [MinLength(20, ErrorMessage = "Content name is too short. Must be longer than 20 characters")]
        public string Content { get; set; }
        public DateTime PostedAt { get; set; }
        public string OwnerUserId { get; set; }
        public bool IsFromPremium { get; set; }
        public bool IsAd { get; set; }
        public int Clouts { get; set; }
        public bool IsStoredPost { get; set; }
        public PostConfiguration PostConfiguration { get; set; }
    }

}
