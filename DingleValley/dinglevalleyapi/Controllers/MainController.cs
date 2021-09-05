using dinglevalleyapi.Extensions;
using dinglevalleyapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dinglevalleyapi.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class MainController : ControllerBase
    {

        private readonly Poster.PosterClient _posterClient;
        public MainController(Poster.PosterClient posterClient)
        {
            _posterClient = posterClient;
        }

        [HttpGet]
        [Route("GetFeed")]

        public async Task<HashSet<DbPost>> GetFeed()
        {
            HashSet<DbPost> posts = new();
            using (AsyncServerStreamingCall<Post> call = _posterClient.GetFeed(new Google.Protobuf.WellKnownTypes.Empty()))
            {
                await foreach (Post post in call.ResponseStream.ReadAllAsync())
                {
                    posts.Add(post.ConvertToDbPost());
                }
            }
            return posts;
        }


    }
}
