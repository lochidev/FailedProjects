using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OAuthServer.ViewModels.Authorization
{
    public class LogoutViewModel
    {
        [BindNever]
        public string RequestId { get; set; }
    }
}
