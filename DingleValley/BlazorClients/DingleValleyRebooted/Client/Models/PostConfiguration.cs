using System.ComponentModel.DataAnnotations;

namespace DingleValleyRebooted.Client.Models
{
    public class PostConfiguration
    {
        public int Id { get; init; }
        public string PrimaryColor { get; set; } = string.Empty;
        public string SecondaryColor { get; set; } = string.Empty;
        public string TertiaryColor { get; set; } = string.Empty;
        [Range(1, 2, ErrorMessage = "Must be between 1 or 2")]
        private int? columnSize;
        public int? ColumnSize
        {
            get => columnSize;
            set
            {
                if (value > 2)
                {
                    columnSize = 2;
                }
                else if (value < 1)
                {
                    columnSize = 1;
                }
                else
                {
                    columnSize = value;
                }
            }
        }
        private int? boost;
        public int? Boost
        {
            get => boost;
            set
            {
                if (value is null or < 0)
                {
                    boost = 0;
                }
                else
                {
                    boost = (int)(Math.Round(Convert.ToDecimal(value / 100)) * 100);
                }
            }
        }
        public bool HasGradient { get; set; } = false;
    }
}
