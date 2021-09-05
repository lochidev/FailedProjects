using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DingleValleyRebooted.Server.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        public AuthenticationController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [AllowAnonymous]
        [HttpGet("Auth/Login")]
        public ActionResult LogIn([FromQuery] string redirectUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl ?? "/" }, OpenIdConnectDefaults.AuthenticationScheme);
        }
        [Authorize]
        [HttpGet("Auth/Logout"), HttpPost("Auth/Logout")]
        public ActionResult LogOut()
        {
            // is redirected from the identity provider after a successful authorization flow and
            // to redirect the user agent to the identity provider to sign out.
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        [HttpGet("Auth/Mobile")]
        public async Task<ActionResult> MobileAuth()
        {


            if (User.Identity.IsAuthenticated)
            {


                // Get parameters to send back to the callback
                Dictionary<string, string> qs = new Dictionary<string, string>
                {
                    { "access_token", await HttpContext.GetTokenAsync("access_token") },
                    { "refresh_token", await HttpContext.GetTokenAsync("refresh_token") ?? string.Empty },
                    { "username", User.Identity.Name }
                };

                // Build the result url
                string url = "dinglevalleyclient" + "://#" + string.Join(
                    "&",
                    qs.Where(kvp => !string.IsNullOrEmpty(kvp.Value) && kvp.Value != "-1")
                    .Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

                // Redirect to final url
                return Redirect(url);
            }


            return Challenge(new AuthenticationProperties { RedirectUri = "/Auth/Mobile" }, OpenIdConnectDefaults.AuthenticationScheme);



        }


        [Authorize]
        [HttpGet]
        [Route("Auth/GetAccessToken")]
        public async Task<IActionResult> GetAccessToken()
        {
            return new ContentResult() { Content = await HttpContext.GetTokenAsync("access_token") };
        }
        [Authorize]
        [HttpGet]
        [Route("Auth/GetRefreshToken")]
        public async Task<IActionResult> GetRefreshToken()
        {
            return new ContentResult() { Content = await HttpContext.GetTokenAsync("refresh_token") };
        }
        [HttpGet]
        [Route("Auth/RenewTokens")]
        public async Task<TokenResponse> RenewTokensAsync([FromQuery] string rt)
        {
            // Initialize the token endpoint:
            HttpClient client = _clientFactory.CreateClient();
            DiscoveryDocumentResponse disco = await client.GetDiscoveryDocumentAsync("https://auth.dinglevalley.net");

            if (disco.IsError)
            {
                throw new Exception(disco.Error);
            }


            // Request a new access token:
            TokenResponse tokenResult = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "dinglevalley_server",
                ClientSecret = "Ilokedinglevalleysenpais_!#@",
                RefreshToken = rt
            });

            if (!tokenResult.IsError)
            {
                return tokenResult;
            }
            return null;
        }

    }
}