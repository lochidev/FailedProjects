using Microsoft.Extensions.Caching.Distributed;
using PostsService.Data;
using PostsService.Extensions;
using PostsService.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PostsService
{
    public class PosterService : Poster.PosterBase
    {
        private readonly IDistributedCache _redisCache;
        private readonly PostsDbContext _postsDbContext;
        private readonly Random _random;
        private static readonly Regex _linkParser = new(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        private readonly Notifier.NotifierClient _notifierClient;
        private readonly ILogger<PosterService> _logger;
        public PosterService(IDistributedCache redisCache, PostsDbContext postsDbContext, Random random, Notifier.NotifierClient notifierClient, ILogger<PosterService> logger)
        {
            _redisCache = redisCache;
            _postsDbContext = postsDbContext;
            _random = random;
            _notifierClient = notifierClient;
            _logger = logger;
        }

        public override async Task GetFeed(Empty request, IServerStreamWriter<Post> responseStream, ServerCallContext context)
        {
            HashSet<Post> randomPosts = await _redisCache.GetRecordAsync<HashSet<Post>>("FEED");
            if (randomPosts is null)
            {
                randomPosts = new HashSet<Post>();
                int loopCount = 0;
                while (randomPosts.Count < 10 && loopCount < 10)
                {
                    loopCount++;
                    int skip = (int)(_random.NextDouble() * _postsDbContext.Posts.Count());
                    HashSet<DbPost> posts = _postsDbContext.Posts.OrderBy(r => r.Title).Include(m => m.PostConfiguration).Skip(skip).Take(10).ToHashSet();
                    foreach (DbPost dbPost in posts)
                    {
                        DateTime postedAt = dbPost.PostedAt;
                        if (dbPost.IsAd ? DateTime.UtcNow > postedAt.AddDays(30) : (dbPost.IsFromPremium ? DateTime.UtcNow > postedAt.AddDays(30) : DateTime.UtcNow > postedAt.AddDays(30)))
                        {
                            await RemovePost(dbPost);
                        }
                        else
                        {
                            randomPosts.Add(ConvertDbPostToPost(dbPost));
                        }
                    }

                }
                if (randomPosts.Count > 0)
                {
                    await _redisCache.SetRecordAsync("FEED", randomPosts, TimeSpan.FromMinutes(3));
                }
            }
            foreach (Post post in randomPosts)
            {
                await responseStream.WriteAsync(post);
            }
        }
        public override async Task<MakeCategoryResponse> MakeCategory(Category request, ServerCallContext context)
        {
            try
            {
                await _postsDbContext.Dbcategories.AddAsync(ConvertCategoryToDbCategory(request));
                await _postsDbContext.SaveChangesAsync();
                return new MakeCategoryResponse { Succeeded = true };

            }
            catch (Exception)
            {

                return new MakeCategoryResponse { Succeeded = false };
            }
        }
        public override async Task GetCategoryPosts(CategoryPostsRequest request, IServerStreamWriter<Post> responseStream, ServerCallContext context)
        {
            try
            {
                request.Category = request.Category.Replace(" ", "").ToUpper();
                HashSet<Post> posts = await _redisCache.GetRecordAsync<HashSet<Post>>($"CATEGORY_{request.Category}");
                if (posts is not null)
                {
                    foreach (Post post in posts)
                    {
                        await responseStream.WriteAsync(post);
                    }
                    return;
                }
                posts = new HashSet<Post>();
                foreach (DbPost post in _postsDbContext.Posts.Where(x => x.Category.Replace(" ", "").ToUpper() == request.Category).Include(m => m.PostConfiguration).ToHashSet())
                {
                    DateTime postedAt = post.PostedAt;
                    if (post.IsAd ? DateTime.UtcNow > postedAt.AddDays(30) : (post.IsFromPremium ? DateTime.UtcNow > postedAt.AddDays(30) : DateTime.UtcNow > postedAt.AddDays(30)))
                    {
                        await RemovePost(post);
                    }
                    else
                    {
                        posts.Add(ConvertDbPostToPost(post));
                    }
                }
                if (posts.Count > 0)
                {
                    await _redisCache.SetRecordAsync($"CATEGORY_{request.Category}", posts, TimeSpan.FromMinutes(3));
                }
                foreach (Post post in posts)
                {
                    await responseStream.WriteAsync(post);
                }
            }
            catch (Exception)
            {

                return;
            }

        }
        public override async Task<PostResponse> MakePost(Post request, ServerCallContext context)
        {
            try
            {
                DbCategory category = _postsDbContext.Dbcategories.Find(request.Category);
                if (string.IsNullOrEmpty(request.Category) || string.IsNullOrEmpty(request.OwnerUserId) || category is null)
                {
                    return new PostResponse { Error = "Bad request" };
                }
                HashSet<string> urlsFound = GetAllUrls(request.Content);
                if (urlsFound.Count > 0 && category.AllowedUrls.Count > 0)
                {
                    if (!category.AllowedUrls.Any(u => urlsFound.Any(x => x.Contains(u))))
                    {
                        return new PostResponse { Error = "The urls in this post are not allowed" };
                    }
                }
                DbPost dbPost = ConvertPostToDbPost(request);

                await _postsDbContext.Posts.AddAsync(dbPost);
                int entries = await _postsDbContext.SaveChangesAsync();
                if (entries == 0)
                {
                    return new PostResponse { Error = "Could not save changes" };
                }
                await _redisCache.SetRecordAsync($"POST_{request.PostId}", dbPost, TimeSpan.FromDays(1));
                return new PostResponse() { PostId = dbPost.PostId };

            }
            catch (Exception)
            {
                return new PostResponse { Error = "Unknown error" };
            }

        }
        public override async Task<Post> GetPost(PostRequest request, ServerCallContext context)
        {
            try
            {
                DbPost post = await _redisCache.GetRecordAsync<DbPost>($"POST_{request.PostId}");
                if (post is not null)
                {
                    return ConvertDbPostToPost(post);
                }
                DbPost dbPost = await _postsDbContext.Posts.FindAsync(request.PostId);
                if (dbPost is not null)
                {
                    DateTime postedAt = dbPost.PostedAt.ToUniversalTime();
                    if (dbPost.IsAd ? DateTime.UtcNow > postedAt.AddDays(30) : (dbPost.IsFromPremium ? DateTime.UtcNow > postedAt.AddDays(30) : DateTime.UtcNow > postedAt.AddDays(30)))
                    {
                        await RemovePost(dbPost);
                        return null;
                    }
                    await _redisCache.SetRecordAsync($"POST_{request.PostId}", dbPost, TimeSpan.FromDays(1));
                    return ConvertDbPostToPost(dbPost);
                }
                return null;

            }
            catch (Exception)
            {

                return null;
            }
        }



        public override async Task SearchCategories(SearchCategoriesRequest request, IServerStreamWriter<Category> responseStream, ServerCallContext context)
        {
            try
            {
                request.Query = request.Query.Replace(" ", "");
                HashSet<DbCategory> categorySet = await _redisCache.GetRecordAsync<HashSet<DbCategory>>($"SEARCH_{request.Query.ToUpper()}");
                if (categorySet is not null)
                {
                    foreach (DbCategory category in categorySet)
                    {
                        await responseStream.WriteAsync(ConvertDbCategoryToCategory(category));
                    }
                    return;
                }
                categorySet = new HashSet<DbCategory>();
                if (string.Equals("All", request.Query, StringComparison.OrdinalIgnoreCase))
                {
                    categorySet = _postsDbContext.Dbcategories.ToHashSet();
                }
                else
                {
                    categorySet = _postsDbContext.Dbcategories.Where(x => x.CategoryName.ToLower().Replace(" ", "").Contains(request.Query.ToLower())).ToHashSet();
                }
                if (categorySet.Count > 0)
                {
                    await _redisCache.SetRecordAsync($"SEARCH_{request.Query.ToUpper()}", categorySet, TimeSpan.FromMinutes(10));
                }
                foreach (DbCategory category in categorySet)
                {
                    await responseStream.WriteAsync(ConvertDbCategoryToCategory(category));
                }
            }
            catch (Exception)
            {

                return;
            }
        }
        public override async Task GetUserPosts(UserPostRequest request, IServerStreamWriter<Post> responseStream, ServerCallContext context)
        {
            try
            {
                HashSet<Post> userPosts = await _redisCache.GetRecordAsync<HashSet<Post>>($"USERPOSTS_{request.UserId}");
                if (userPosts is not null)
                {
                    foreach (Post userPost in userPosts)
                    {
                        await responseStream.WriteAsync(userPost);
                    }
                    return;
                }
                userPosts = new HashSet<Post>();
                foreach (DbPost dbPost in _postsDbContext.Posts.Where(x => x.OwnerUserId == request.UserId).Include(m => m.PostConfiguration))
                {
                    userPosts.Add(ConvertDbPostToPost(dbPost));
                }
                await _redisCache.SetRecordAsync($"USERPOSTS_{request.UserId}", userPosts, TimeSpan.FromMinutes(5));
                foreach (Post userPost in userPosts)
                {
                    await responseStream.WriteAsync(userPost);
                }
            }
            catch (Exception)
            {

                return;
            }
        }
        public override async Task<DeletePostResponse> DeletePost(DeletePostRequest request, ServerCallContext context)
        {
            try
            {
                DbPost post = _postsDbContext.Posts.Include(m => m.PostConfiguration).FirstOrDefault(x => x.PostId == request.PostId);
                if (post is not null && post.OwnerUserId.ToLower() == request.UserId.ToLower())
                {
                    await RemovePost(post);
                    return new DeletePostResponse { Succeeded = true };
                }
                return new DeletePostResponse { Succeeded = false };

            }
            catch (Exception)
            {
                return new DeletePostResponse { Succeeded = false };

            }
        }
        private static Post ConvertDbPostToPost(DbPost dbPost)
        {
            return new Post
            {
                PostId = dbPost.PostId,
                Category = dbPost.Category,
                Title = dbPost.Title,
                Content = dbPost.Content,
                OwnerUserId = dbPost.OwnerUserId,
                IsAd = dbPost.IsAd,
                IsFromPremium = dbPost.IsFromPremium,
                Clouts = dbPost.Clouts,
                PostTimestamp = Timestamp.FromDateTime(dbPost.PostedAt.ToUniversalTime().ToUniversalTime()),
                PostConfiguration = new Post.Types.configuration_data
                {
                    PrimaryColor = dbPost.PostConfiguration?.PrimaryColor ?? string.Empty,
                    SecondaryColor = dbPost.PostConfiguration?.SecondaryColor ?? string.Empty,
                    TertiaryColor = dbPost.PostConfiguration?.TertiaryColor ?? string.Empty,
                    ColumnSize = dbPost.PostConfiguration?.ColumnSize,
                    HasGradient = dbPost.PostConfiguration is not null && dbPost.PostConfiguration.HasGradient,
                    Boost = dbPost.PostConfiguration?.Boost
                }
            };
        }
        private static DbPost ConvertPostToDbPost(Post post)
        {
            long postTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            return new DbPost
            {
                PostId = ($"{post.OwnerUserId}{post.Content}{post.Title}{postTime}").ToSha256(),
                Category = post.Category,
                Title = post.Title,
                Content = post.Content,
                OwnerUserId = post.OwnerUserId,
                IsAd = post.IsAd,
                IsFromPremium = post.IsFromPremium,
                Clouts = post.Clouts,
                PostedAt = DateTimeOffset.FromUnixTimeSeconds(postTime).UtcDateTime,
                PostConfiguration = new PostConfiguration
                {
                    PrimaryColor = post.PostConfiguration?.PrimaryColor,
                    SecondaryColor = post.PostConfiguration?.SecondaryColor,
                    TertiaryColor = post.PostConfiguration?.TertiaryColor,
                    ColumnSize = post.PostConfiguration?.ColumnSize,
                    HasGradient = post.PostConfiguration is not null && post.PostConfiguration.HasGradient,
                    Boost = post.PostConfiguration?.Boost
                }

            };
        }
        private static DbCategory ConvertCategoryToDbCategory(Category request)
        {
            return new DbCategory
            {
                CategoryName = request.CategoryName,
                Description = request.Description,
                OwnerUserId = request.OwnerUserId,
                AllowedUrls = new List<string>(request.AllowedUrls)
            };
        }
        private static Category ConvertDbCategoryToCategory(DbCategory request)
        {
            Category category = new Category
            {
                CategoryName = request.CategoryName,
                Description = request.Description,
                OwnerUserId = request.OwnerUserId
            };
            category.AllowedUrls.Add(request.AllowedUrls);
            return category;
        }
        private static HashSet<string> GetAllUrls(string content)
        {
            HashSet<string> returnSet = new();
            foreach (Match m in _linkParser.Matches(content))
            {
                returnSet.Add(m.Value);
            }
            return returnSet;
        }
        private async Task RemovePost(DbPost dbPost)
        {
            string title = dbPost.Title.Length > 20 ? $"{dbPost.Title.Substring(0, 20)}..." : dbPost.Title;
            string data = JsonSerializer.Serialize(dbPost, Extensions.DistributedCacheExtensions.jsonSerializerOptions);
            if (dbPost.PostConfiguration is not null)
            {
                _postsDbContext.Remove(dbPost.PostConfiguration);
                _postsDbContext.SaveChanges();
            }
            _postsDbContext.Remove(dbPost);
            _postsDbContext.SaveChanges();
            try
            {
                await _notifierClient.NotifyUserAsync(new NotifyUserRequest
                {
                    Title = $"Your post has expired!",
                    Description = $"Hello. This is a robo from the clouds. Your post: \"{title}\" has expired! Don't worry though, just click this notification and you can quickly repost the same data!",
                    UserId = dbPost.OwnerUserId,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error when notifying user: {ex.Message}");
            }

        }
    }
}
