using dinglevalleyapi.Extensions;
using dinglevalleyapi.Models;
using dinglevalleyapi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dinglevalleyapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly Poster.PosterClient _posterClient;
        private readonly Coiner.CoinerClient _coinerClient;
        public CategoryController(Poster.PosterClient posterClient, Coiner.CoinerClient coinerClient)
        {
            _posterClient = posterClient;
            _coinerClient = coinerClient;
        }
        [HttpGet]
        [Route("GetCategoryPosts")]

        public async Task<IActionResult> GetCategoryPosts([FromQuery] string category)
        {
            if (category is null)
            {
                return BadRequest();
            }
            HashSet<DbPost> categoryPosts = new HashSet<DbPost>();
            using (AsyncServerStreamingCall<Post> call = _posterClient.GetCategoryPosts(new CategoryPostsRequest { Category = category }))
            {
                await foreach (Post post in call.ResponseStream.ReadAllAsync())
                {
                    categoryPosts.Add(post.ConvertToDbPost());
                }
            }
            return Ok(categoryPosts);
        }
        [HttpGet]
        [Route("SearchCategories")]

        public async Task<IActionResult> SearchCategories([FromQuery] string query)
        {
            if (query is null)
            {
                return BadRequest();
            }

            HashSet<DbCategory> categories = new HashSet<DbCategory>();
            using (AsyncServerStreamingCall<Category> call = _posterClient.SearchCategories(new SearchCategoriesRequest { Query = query }))
            {
                await foreach (Category category in call.ResponseStream.ReadAllAsync())
                {
                    categories.Add(ConverCategoryToDbCategory(category));
                }
            }
            return Ok(categories);
        }
        [HttpPost]
        [Route("MakeCategory")]
        [Authorize]
        public async Task<IActionResult> MakeCategory(MakeCategoryViewModel request)
        {
            string userId = User.Identity.Name;
            UserStatsResponse coinsOwnedResponse = await _coinerClient.GetUserStatsAsync(new UserStatsRequest { UserId = userId });
            if (coinsOwnedResponse.Coins < 200)
            {
                return BadRequest();
            }
            await _coinerClient.DeductCoinsAsync(new DeductCoinRequest { UserId = userId, Amount = 200 });
            MakeCategoryResponse response = await _posterClient.MakeCategoryAsync(new Category { CategoryName = request.CategoryName, Description = request.Description, OwnerUserId = userId });
            if (response.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
        private static DbCategory ConverCategoryToDbCategory(Category request)
        {
            return new DbCategory
            {
                CategoryName = request.CategoryName,
                Description = request.Description,
                OwnerUserId = request.OwnerUserId,
                AllowedUrls = new List<string>(request.AllowedUrls)
            };
        }

    }
}
