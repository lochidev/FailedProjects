using dinglevalleyapi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace dinglevalleyapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly Notifier.NotifierClient _notifierClient;

        public NotificationsController(Notifier.NotifierClient notifierClient)
        {
            _notifierClient = notifierClient;
        }
        [HttpPost]
        [Route("RegisterUser")]
        public async Task<IActionResult> RegisterUser(RegisterUserViewModel request)
        {
            if (ModelState.IsValid)
            {
                string userId = User.Identity.Name;
                RegisterUserResponse response = await _notifierClient.RegisterUserAsync(new RegisterUserRequest
                {
                    UserId = userId,
                    FcmToken = request.FcmToken
                });
                if (response.Success)
                {
                    return Ok();
                }
            }
            return BadRequest();
        }
    }
}
