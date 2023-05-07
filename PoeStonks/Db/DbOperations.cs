using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoeStonks.PoeNinja;

namespace PoeStonks.Db;

public class DbOperations
{
    public async Task AddOrUpdateNinjaData(List<AllPoeItems> itemList)
    {
        List<PoeItem> poeItems = new List<PoeItem>();
        int counter = 0;
            using PsDbContext dbContext = new();
            {
                foreach (var item in itemList)
                {
                    Console.WriteLine(counter++);
                    PoeItem newItem = new PoeItem
                    {
                        ItemName = item.ItemName,
                        ItemType = item.ItemType,
                        ChaosEquivalent = item.ChaosValue,
                        ListingAmount = item.ListingCount
                    };
                    poeItems.Add(newItem);
                }
                
                dbContext.PoeItems.AddRange(poeItems);
                await dbContext.SaveChangesAsync(); 
            }
    }
}
