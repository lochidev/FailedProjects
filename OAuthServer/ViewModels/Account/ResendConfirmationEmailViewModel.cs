using System.ComponentModel.DataAnnotations;

namespace OAuthServer.ViewModels.Account
{
    public class ResendConfirmationEmailViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
