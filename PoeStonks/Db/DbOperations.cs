using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoeStonks.PoeNinja;

namespace PoeStonks.Db;

public class DbOperations
{

    public DbOperations(MainWindow mainWindow)
    {
        MainWindow = mainWindow;
    }

    protected DbOperations(){}

    public MainWindow MainWindow { get; set; }
    
    public async Task AddOrUpdateNinjaData(List<PoeItem> freshNinjaItemsList)
    {
        List<PoeItem> itemsAlreadyInDb;
        
        // bottom log
        MainWindow.PseudoLog("Database update started");
        
            await RemoveItemsFromDbIfTheyDontExistAnymore(freshNinjaItemsList);
        
            await UpdateNinjaItemsDb(freshNinjaItemsList);
            
            using (PsDbContext dbContext = new PsDbContext())
            {
                itemsAlreadyInDb = dbContext.PoeItems.ToList();
                
                List<PoeItem> itemsToAdd =
                    freshNinjaItemsList.Except(itemsAlreadyInDb, new DatabaseItemComparerById()).ToList();
                
                if (itemsToAdd.Count > 0)
                {
                dbContext.PoeItems.AddRange(itemsToAdd);
                await dbContext.SaveChangesAsync(); 
                }
            }
            
        // bottom log
        MainWindow.PseudoLog("Database update done");;
    }

    private async Task UpdateNinjaItemsDb(List<PoeItem> freshItemsFromNinjaList)
    {
        using (PsDbContext dbContext = new PsDbContext())
        {
            List<PoeItem> itemsAlreadyInDb = dbContext.PoeItems.ToList();
            List<PoeItem> itemsToUpdateList = freshItemsFromNinjaList.Intersect(itemsAlreadyInDb, new DatabaseItemComparerByPrice()).ToList();
            
            foreach (var item in itemsAlreadyInDb)
            {
                PoeItem? newItemData = itemsToUpdateList.FirstOrDefault(pi => pi.Id == item.Id);
                if (newItemData != null)
                {
                    item.ChaosEquivalent = newItemData.ChaosEquivalent;
                }
            }
            
            dbContext.PoeItems.UpdateRange(itemsAlreadyInDb);
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task RemoveItemsFromDbIfTheyDontExistAnymore(List<PoeItem> freshNinjaItemsList)
    {
        using (PsDbContext dbContext = new PsDbContext())
        {
            List<PoeItem> itemsAlreadyInDb = dbContext.PoeItems.ToList();
            List<PoeItem> itemsToDelete = itemsAlreadyInDb.Except(freshNinjaItemsList, new DatabaseItemComparerById()).ToList();
            
            foreach (var item in itemsToDelete)
            {
                itemsAlreadyInDb.Remove(item);
            }
                    
            dbContext.PoeItems.UpdateRange(itemsAlreadyInDb);
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

