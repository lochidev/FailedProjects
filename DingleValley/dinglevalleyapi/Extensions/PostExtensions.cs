using dinglevalleyapi.Models;

namespace dinglevalleyapi.Extensions
{
    public static class PostExtensions
    {
        public static DbPost ConvertToDbPost(this Post post)
        {
            return new DbPost
            {
                PostId = post.PostId,
                Category = post.Category,
                Title = post.Title,
                Content = post.Content,
                OwnerUserId = post.OwnerUserId,
                IsAd = post.IsAd,
                IsFromPremium = post.IsFromPremium,
                Clouts = post.Clouts,
                PostedAt = post.PostTimestamp.ToDateTime(),
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
        public static Post ConvertDbPostToPost(this DbPost dbPost)
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
    }
}
