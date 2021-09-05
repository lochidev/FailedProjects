using CoinService.Models;
using Microsoft.EntityFrameworkCore;

namespace CoinService.Data
{
    public class CoinsDbContext : DbContext
    {
        public CoinsDbContext(DbContextOptions<CoinsDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
    }
}
