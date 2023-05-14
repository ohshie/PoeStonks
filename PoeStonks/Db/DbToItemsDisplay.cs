using Microsoft.EntityFrameworkCore;

namespace PoeStonks.Db;

public class DbToItemsDisplay : DbOperations
{
    public List<PoeItem> FetchItemsSortedByChaos(int amount)
    {
        using (PsDbContext dbContext = new())
        {
            if (dbContext.PoeItems.Any())
            {
                List<PoeItem> poeItems = dbContext.PoeItems
                    .Include(pi => pi.SparkLine)
                    .OrderByDescending(pi => pi.ChaosEquivalent)
                    .Take(amount)
                    .ToList();
                
                return poeItems;
            }
        }
        return null;
    }

    public double FetchDivinePrice()
    {
        using (PsDbContext dbContext = new())
        {
            if (dbContext.PoeItems.Any())
            {
                PoeItem? currentDivinePrice = dbContext.PoeItems.FirstOrDefault(pi => pi.Id == "divine-orb");
                if (currentDivinePrice != null)
                {
                    return Math.Round(currentDivinePrice.ChaosEquivalent,2);
                }
            }
        }

        return 0;
    }
}