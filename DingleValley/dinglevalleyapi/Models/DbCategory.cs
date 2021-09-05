using System.Collections.Generic;

namespace dinglevalleyapi.Models
{
    public class DbCategory
    {

        public string CategoryName { get; set; }

        public string Description { get; set; }

        public string OwnerUserId { get; set; }

        public List<string> AllowedUrls { get; set; }
    }
}
