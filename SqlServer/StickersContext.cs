using Microsoft.EntityFrameworkCore;

namespace webapi.SqlServer
{
    public class StickersContext : DbContext
    {
        public StickersContext(DbContextOptions<StickersContext> options)
            : base(options)
        {
        }

        public DbSet<Sticker> Stickers { get; set; }
    }

    public class Sticker
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string PlayerName { get; set; }
        public string Country { get; set; }
    }
}