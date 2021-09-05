using dinglevalleyapi.Extensions;
using dinglevalleyapi.Models;
using dinglevalleyapi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace dinglevalleyapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly Poster.PosterClient _posterClient;
        private readonly Coiner.CoinerClient _coinerClient;
        private readonly Banner.BannerClient _bannerClient;
        private readonly ProfanityFilter.ProfanityFilter _profanityFilter;
        private static readonly RNGCryptoServiceProvider rng = new();
        public PostsController(Poster.PosterClient posterClient, Coiner.CoinerClient coinerClient, ProfanityFilter.ProfanityFilter profanityFilter, Banner.BannerClient bannerClient)
        {
            _posterClient = posterClient;
            _coinerClient = coinerClient;
            _profanityFilter = profanityFilter;
            _bannerClient = bannerClient;
        }
        [HttpGet]
        [Route("GetUserPosts")]
        public async Task<IActionResult> GetUserPosts([FromQuery] string userId)
        {
            if (userId is null)
            {
                return BadRequest();
            }
            HashSet<DbPost> userPosts = new HashSet<DbPost>();
            using (AsyncServerStreamingCall<Post> call = _posterClient.GetUserPosts(new UserPostRequest { UserId = userId }))
            {
                await foreach (Post post in call.ResponseStream.ReadAllAsync())
                {
                    userPosts.Add(post.ConvertToDbPost());
                }
            }
            return Ok(userPosts);
        }
        [HttpGet]
        [Route("GetPost")]
        public async Task<IActionResult> GetPost([FromQuery] GetPostViewModel request)
        {
            if (request is null || string.IsNullOrEmpty(request.PostId))
            {
                return BadRequest();
            }

            try
            {
                return Ok((await _posterClient.GetPostAsync(new PostRequest { PostId = request.PostId })).ConvertToDbPost());

            }
            catch (Exception)
            {
                return NotFound();
            }
        }
        [HttpPost]
        [Route("MakePost")]
        [Authorize]
        public async Task<IActionResult> MakePost(DbPost request)
        {
            if (request.Category.Length is > 25 or < 4 || request.Title.Length is > 75 or < 10 || request.Content.Length is > 500 or < 20)
            {
                return BadRequest();
            }
            string userId = User.Identity.Name;
            string serviceId = $"MAKE_POST_DV_{request.Category}";
            IsBannedResponse response = await _bannerClient.IsBannedAsync(new IsBannedRequest { Username = userId, ServiceId = serviceId });
            if (response.IsBanned)
            {
                return BadRequest("You cannot make more posts in this category until your current one expires!");
            }
            if (_profanityFilter.DetectAllProfanities(request.Content).Count >= 5)
            {
                request.Content = _profanityFilter.CensorString(request.Content);
            }
            if (_profanityFilter.DetectAllProfanities(request.Title).Count >= 3)
            {
                request.Title = _profanityFilter.CensorString(request.Title);
            }

            UserStatsResponse userStatsResponse = await _coinerClient.GetUserStatsAsync(new UserStatsRequest { UserId = userId });
            if (userStatsResponse.Succeeded)
            {
                if (!userStatsResponse.IsPartner)
                {
                    if (request.IsFromPremium && !userStatsResponse.IsPremium)
                    {
                        return BadRequest("You have not purchased premium");
                    }
                }
                if (request.PostConfiguration is not null)
                {
                    int cost = CalculateCost(userStatsResponse, request.PostConfiguration);
                    if (userStatsResponse.Coins - cost < 0)
                    {
                        return BadRequest("Not enough coins");
                    }
                    await _coinerClient.DeductCoinsAsync(new DeductCoinRequest { UserId = userId, Amount = cost });
                }

            }
            else
            {
                return BadRequest("Sorry something went wrong when checking your user stats :( Please report this issue to us");
            }
            int? boost = request.PostConfiguration.Boost;
            request.Clouts = (request.IsFromPremium ? NextInt(300, 401) : NextInt(100, 201)) + (boost is null ? 0 : (int)boost);
            request.OwnerUserId = userId;
            PostResponse postResponse = await _posterClient.MakePostAsync(request.ConvertDbPostToPost());
            switch (postResponse.ResultCase)
            {
                case PostResponse.ResultOneofCase.None:
                    return null;
                case PostResponse.ResultOneofCase.Error:
                    return BadRequest(postResponse.Error);
                case PostResponse.ResultOneofCase.PostId:
                    await _bannerClient.BanUserAsync(new BanRequest { Username = userId, ServiceId = serviceId, Duration = Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(TimeSpan.FromDays(3)) });
                    return Ok(postResponse.PostId);
                default:
                    return null;
            }


        }
        [HttpGet]
        [Route("Delete")]
        [Authorize]
        public async Task<IActionResult> DeletePost([FromQuery] string postId, [FromQuery] string category)
        {
            string userId = "Lochi";
            DeletePostResponse response = await _posterClient.DeletePostAsync(new DeletePostRequest { PostId = postId, UserId = userId });
            if (response.Succeeded)
            {
                string serviceId = $"MAKE_POST_DV_{category}";
                await _bannerClient.UnbanUserAsync(new UnbanRequest { Username = userId, ServiceId = serviceId });
                return Ok();
            }
            return Forbid();
        }
        [HttpGet]
        [Route("ClaimReward")]
        [Authorize]
        public async Task<IActionResult> ClaimReward([FromQuery] string postId)
        {
            string userId = User.Identity.Name;
            if (string.IsNullOrEmpty(postId))
            {
                return BadRequest();
            }
            Post post = await _posterClient.GetPostAsync(new PostRequest { PostId = postId });
            string serviceId = $"CLAIM_DV_{post.PostId}";
            IsBannedResponse response = await _bannerClient.IsBannedAsync(new IsBannedRequest { Username = userId, ServiceId = serviceId });
            if (response.IsBanned || userId.ToLower() == post.OwnerUserId.ToLower())
            {
                return BadRequest("Already claimed!");
            }
            int clouts = post.Clouts == 0 ? NextInt(100, 201) : post.Clouts;
            await _coinerClient.IssueCoinsAsync(new IssueCoinsRequest { Coins = clouts, UserId = userId });
            await _bannerClient.BanUserAsync(new BanRequest { Username = userId, ServiceId = serviceId, Duration = Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(TimeSpan.FromDays(10)) });
            return Ok();
        }
        [HttpPost]
        [Route("HasClaimedReward")]
        [Authorize]
        public async Task<IActionResult> HasClaimedReward([FromBody] HashSet<string> postIds)
        {
            string userId = User.Identity.Name;
            Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
            if (postIds.Count == 0)
            {
                return BadRequest();
            }
            foreach (string postId in postIds)
            {
                string serviceId = $"CLAIM_DV_{postId}";
                IsBannedResponse response = await _bannerClient.IsBannedAsync(new IsBannedRequest { Username = userId, ServiceId = serviceId });
                dictionary.Add(postId, response.IsBanned);
            }
            return Ok(dictionary);
        }
        private static int NextInt(int min, int max)
        {
            byte[] buffer = new byte[4];
            rng.GetBytes(buffer);
            int result = BitConverter.ToInt32(buffer, 0);

            return new Random(result).Next(min, max);
        }
        private static int CalculateCost(UserStatsResponse userStats, PostConfiguration config)
        {
            int cost = 0;
            if (userStats.IsPartner)
            {
                config.Boost = 500;
                return cost;
            }
            if (config.ColumnSize is not null && config.ColumnSize == 2)
            {
                cost += 2500;
            }
            if (!userStats.IsPremium && config.HasGradient)
            {
                cost += 2500;
            }
            if (config.Boost is not null and > 0)
            {
                cost += (int)(config.Boost / 100) * 1000;
            }
            if (cost < 0)
            {
                cost = 0;
            }
            return cost;
        }
    }
}
