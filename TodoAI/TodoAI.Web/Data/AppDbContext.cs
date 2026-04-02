using Microsoft.EntityFrameworkCore;
using TodoAI.Web.Models;

namespace TodoAI.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Todo> Todos => Set<Todo>();
    }
}

