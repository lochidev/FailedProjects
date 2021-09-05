using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OAuthServer.Models;

namespace OAuthServer.Controllers
{
    [Authorize]

    public class ManageController : Controller
    {
        private readonly IDataBaseClass _dataBaseClass;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManageController(IDataBaseClass dataBaseClass = null, SignInManager<ApplicationUser> signInManager = null, UserManager<ApplicationUser> userManager = null)
        {
            _dataBaseClass = dataBaseClass;
            _signInManager = signInManager;
            _userManager = userManager;
        }
        [Route("Account/Manage/Index")]
        public async Task<IActionResult> Index()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            return View(user);
        }
        [Route("Account/Manage/DeleteConfirmation")]
        public IActionResult DeleteConfirmation()
        {
            return View();
        }
        [Route("Account/Manage/Delete")]

        public async Task<IActionResult> Delete()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            try
            {
                List<DatabaseServices.Models.UserPlayList> playlists = await _dataBaseClass.GetUserPlayLists(user.Id);
                foreach (DatabaseServices.Models.UserPlayList playlist in playlists)
                {
                    await _dataBaseClass.DeleteUserPlayList(playlist.Id);
                }
            }
            catch (Exception)
            {

            }


            await _signInManager.SignOutAsync();
            await _userManager.DeleteAsync(user);
            return RedirectToAction("LogIn", "Account");
        }
    }
}
