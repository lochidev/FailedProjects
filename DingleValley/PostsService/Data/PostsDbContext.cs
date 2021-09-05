using PostsService.Models;

namespace PostsService.Data
{
    public class PostsDbContext : DbContext
    {

        public PostsDbContext(DbContextOptions<PostsDbContext> options) : base(options)
        {

        }
        public DbSet<DbCategory> Dbcategories { get; set; }
        public DbSet<DbPost> Posts { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<PostConfiguration>()
                .HasOne(e => e.Post)
                .WithOne(e => e.PostConfiguration)
                .OnDelete(DeleteBehavior.ClientCascade);
        }
    }
}
