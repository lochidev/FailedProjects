namespace DingleValleyRebooted.Client.Models
{
    public struct Category
    {
        public string CategoryName { get; set; }

        public string Description { get; set; }

        public string OwnerUserId { get; set; }
        public List<string> AllowedUrls { get; set; }
    }
}
