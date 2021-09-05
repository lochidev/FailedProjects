using System.ComponentModel.DataAnnotations;

namespace PostsService.Models
{
    public class DbCategory
    {
        [Required]
        [Key]
        public string CategoryName { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string OwnerUserId { get; set; }
        [Required]
        public List<string> AllowedUrls { get; set; }

    }
}
