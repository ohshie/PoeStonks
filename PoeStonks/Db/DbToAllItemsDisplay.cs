using System;
using System.Collections.Generic;
using System.Linq;

namespace PoeStonks.Db;

public class DbToAllItemsDisplay : DbOperations
{
    public List<PoeItem> FetchItemsToDisplayInitialOrAfterUpdate(int amount)
    {
        using (PsDbContext dbContext = new())
        {
            if (dbContext.PoeItems.Any())
            {
                List<PoeItem> poeItems = dbContext.PoeItems.Take(amount).OrderByDescending(pi => pi.ChaosEquivalent).ToList();
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