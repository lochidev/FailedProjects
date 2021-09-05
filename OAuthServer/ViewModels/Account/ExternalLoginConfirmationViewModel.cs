using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OAuthServer.ViewModels.Account
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [DisplayName("Choose a unique username")]
        public string UserName { get; set; }

        [Required]

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string ServiceProvider { get; set; }
    }
}
