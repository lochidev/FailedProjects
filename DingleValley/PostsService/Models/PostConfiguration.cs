using System.ComponentModel.DataAnnotations;

namespace PostsService.Models
{
    public class PostConfiguration
    {
        [Key]
        public int Id { get; set; }
        public string PostId { get; set; }
        public DbPost Post { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }
        public string TertiaryColor { get; set; }
        public int? ColumnSize { get; set; }
        public int? Boost { get; set; }
        public bool HasGradient { get; set; }
    }
}
