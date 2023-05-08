using System.Collections.Generic;
using System.Linq;

namespace PoeStonks.Db;

public class DbToAllItemsDisplay : DbOperations
{
    public List<PoeItem> FetchItemsToDisplayInitialOrAfterUpdate(int amount)
    {
        using PsDbContext dbContext = new();
        {
            if (dbContext.PoeItems.Any())
            {
                List<PoeItem> poeItems = dbContext.PoeItems.Take(amount).OrderByDescending(pi => pi.ChaosEquivalent).ToList();
                return poeItems;
            }
        }
        return null;
    }
}