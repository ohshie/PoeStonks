using System;
using System.Collections.Generic;
using PoeStonks.Db;

namespace PoeStonks.AllItemsDisplay;

public class NinjaItemsDisplay
{
    private DbToAllItemsDisplay _dbToAllItemsDisplay = new();
    public double CurrentDivinePrice { get; set; }

    public NinjaItemsDisplay()
    {
        CurrentDivinePrice =_dbToAllItemsDisplay.FetchDivinePrice();
    }
    
    public void NinjaItemsDisplayFill()
    {
        CurrentDivinePrice = _dbToAllItemsDisplay.FetchDivinePrice();
        
        List<PoeItem> poeItems = _dbToAllItemsDisplay.FetchItemsToDisplayInitialOrAfterUpdate(50);
        if (poeItems != null)
        {
            foreach (var poeItem in poeItems)
            {
                if (poeItem.ItemName.Length > 25)
                {
                    MainWindow.PoeItemsName.Add($"{poeItem.ItemName.Substring(0,23)}...");
                }
                else
                {
                    MainWindow.PoeItemsName.Add(poeItem.ItemName);
                } 
                
                MainWindow.PoeItemsCategory.Add(poeItem.ItemType);
                MainWindow.PoeItemsChaosValue.Add(Math.Round(poeItem.ChaosEquivalent,0));
                MainWindow.NinjaItemIconLink.Add(poeItem.ImgUrl);
                MainWindow.NinjaItemLink.Add(poeItem.ItemUrl);
                MainWindow.PoeItemsDivineEquivalent.Add(Math.Round(poeItem.ChaosEquivalent/CurrentDivinePrice,2));
            } 
        }
    }
}