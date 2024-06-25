using Ksiegarnia.Models;
using Microsoft.EntityFrameworkCore;

namespace Ksiegarnia.Data
{
    public class KsiegarniaDbContext : DbContext
    {
        public KsiegarniaDbContext(DbContextOptions<KsiegarniaDbContext> options) : base(options)
        {

        }

        public DbSet<Book> Books { get; set; }
    }
}
