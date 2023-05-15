using Microsoft.EntityFrameworkCore;

namespace PoeStonks.Db;

public class DbOperations
{
    public async Task AddOrUpdateNinjaData(List<PoeItem> freshNinjaItemsList)
    {
        Logger.LogMessageOutput = "Database update started";
        
        await RemoveItemsFromDbIfTheyDontExistAnymore(freshNinjaItemsList);
        
        await UpdateNinjaItemsDb(freshNinjaItemsList);

        await AddNewItemsToDb(freshNinjaItemsList);
        
        Logger.LogMessageOutput = "Database update done";
    }

    private async Task UpdateNinjaItemsDb(List<PoeItem> freshItemsFromNinjaList)
    {
        using (PsDbContext dbContext = new PsDbContext())
        {
            var itemsToUpdateDictionary = freshItemsFromNinjaList.ToDictionary(pi => pi.Id);

            foreach (var item in dbContext.PoeItems)
            {
                if (itemsToUpdateDictionary.TryGetValue(item.Id, out PoeItem? newItemData))
                {
                    if (item.ChaosEquivalent != newItemData.ChaosEquivalent)
                    {
                        item.ChaosEquivalent = newItemData.ChaosEquivalent;
                    }
                }
            }
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task RemoveItemsFromDbIfTheyDontExistAnymore(List<PoeItem> freshNinjaItemsList)
    {
        using (PsDbContext dbContext = new PsDbContext())
        {
            var freshItemDictionary = freshNinjaItemsList.ToDictionary(pi => pi.Id);

            foreach (var item in dbContext.PoeItems.Include(pi => pi.SparkLine))
            {
                if (!freshItemDictionary.ContainsKey(item.Id))
                {
                    dbContext.PoeItems.Remove(item);
                }
            }
            
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task AddNewItemsToDb(List<PoeItem> freshNinjaItemsList)
    {
        using (PsDbContext dbContext = new PsDbContext())
        {
            var itemsInDb = dbContext.PoeItems.ToDictionary(pi => pi.Id);

            foreach (var item in freshNinjaItemsList)
            {
                if (!itemsInDb.ContainsKey(item.Id))
                {
                    dbContext.PoeItems.Add(item);
                }
            }
            
            await dbContext.SaveChangesAsync();
        }
    }
}
