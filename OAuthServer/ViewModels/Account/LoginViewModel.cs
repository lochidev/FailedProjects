using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace OAuthServer.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Identifier { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        [BindProperty(Name = "g-recaptcha-response")]
        public string RecaptchaToken { get; set; }
    }
}
