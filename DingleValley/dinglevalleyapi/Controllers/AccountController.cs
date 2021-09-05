using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace dinglevalleyapi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly Coiner.CoinerClient _coinerClient;

        public AccountController(Coiner.CoinerClient coinerClient)
        {
            _coinerClient = coinerClient;
        }
        [HttpGet]
        [Route("UserStats")]
        public async Task<IActionResult> GetUserStats()
        {
            string userId = User.Identity.Name;
            UserStatsResponse userStatsResponse = await _coinerClient.GetUserStatsAsync(new UserStatsRequest { UserId = userId });
            if (userStatsResponse.Succeeded)
            {
                return Ok(userStatsResponse);
            }
            else
            {
                return StatusCode(500);
            }
        }
    }
}
