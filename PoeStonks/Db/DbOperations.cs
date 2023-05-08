using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoeStonks.PoeNinja;

namespace PoeStonks.Db;

public class DbOperations
{
    public async Task AddOrUpdateNinjaData(List<PoeItem> freshNinjaItemsList)
    {
        List<PoeItem> itemsAlreadyInDb;
        
        using (PsDbContext dbContext = new())
        {
            Console.WriteLine(DateTime.Now);
            itemsAlreadyInDb = dbContext.PoeItems.ToList();
        }

        List<PoeItem> itemsToAdd =
            freshNinjaItemsList.Except(itemsAlreadyInDb, new DatabaseItemComparerById()).ToList();
        List<PoeItem> itemsToUpdate = freshNinjaItemsList.Intersect(itemsAlreadyInDb, new DatabaseItemComparerByPrice()).ToList();
        List<PoeItem> itemsToDelete = itemsAlreadyInDb.Except(freshNinjaItemsList, new DatabaseItemComparerById()).ToList();
        
        if (itemsToDelete.Count > 0)
        {
            await RemoveItemsFromDbIfTheyDontExistAnymore(itemsToDelete, itemsAlreadyInDb);
        }
            
        if (itemsToUpdate.Count > 0)
        {
            await UpdateNinjaItemsDb(itemsToUpdate, freshNinjaItemsList);
        }

        if (itemsToAdd.Count > 0)
        {
            using (PsDbContext dbContext = new PsDbContext())
            {
                dbContext.PoeItems.AddRange(itemsToAdd);
                await dbContext.SaveChangesAsync(); 
            }
        }
        
        Console.WriteLine(DateTime.Now);
    }

    private async Task UpdateNinjaItemsDb(List<PoeItem> itemsToUpdateList, List<PoeItem> freshItemsFromNinjaList)
    {
        using (PsDbContext dbContext = new PsDbContext())
        {
            foreach (var item in itemsToUpdateList)
            {
                PoeItem? itemToUpdate = freshItemsFromNinjaList.FirstOrDefault(pi => pi.ItemName == item.ItemName);
                if (itemToUpdate != null)
                {
                    itemToUpdate.ChaosEquivalent = item.ChaosEquivalent;
                }
            }
            dbContext.PoeItems.UpdateRange(itemsToUpdateList);
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task RemoveItemsFromDbIfTheyDontExistAnymore(List<PoeItem> itemsToRemoveList,
        List<PoeItem> currentItemsInDb)
    {
        using (PsDbContext dbContext = new PsDbContext())
        {
            foreach (var item in itemsToRemoveList)
            {
                currentItemsInDb.Remove(item);
            }
                    
            dbContext.PoeItems.UpdateRange(currentItemsInDb);
            await dbContext.SaveChangesAsync();
        }
    }
}

internal class DatabaseItemComparerByPrice : IEqualityComparer<PoeItem>
{
    public bool Equals(PoeItem x, PoeItem y)
    {
        return x.ChaosEquivalent == y.ChaosEquivalent;
    }

    public int GetHashCode(PoeItem obj)
    {
        return obj.ItemName.GetHashCode();
    }
}

internal class DatabaseItemComparerById : IEqualityComparer<PoeItem>
{
    public bool Equals(PoeItem x, PoeItem y)
    {
        return x.Id == y.Id;
    }

    public int GetHashCode(PoeItem obj)
    {
        return obj.ItemName.GetHashCode();
    }
}

