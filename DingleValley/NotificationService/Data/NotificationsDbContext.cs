using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data
{
    public class NotificationsDbContext : DbContext
    {

        public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
    }
}
