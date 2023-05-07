using Microsoft.EntityFrameworkCore;

namespace PoeStonks.Db;

public class PsDbContext : DbContext
{
    public DbSet<PoeItem> PoeItems { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=PsDb.sqlite");
    }
}