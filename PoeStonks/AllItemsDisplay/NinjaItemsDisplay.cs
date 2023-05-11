using System;
using System.Collections.Generic;
using PoeStonks.Db;

namespace PoeStonks.AllItemsDisplay;

public class NinjaItemsDisplay
{
    private DbToAllItemsDisplay _dbToAllItemsDisplay = new();
    
    public void NinjaItemsDisplayFill()
    {
        List<PoeItem> poeItems = _dbToAllItemsDisplay.FetchItemsToDisplayInitialOrAfterUpdate(50);
        if (poeItems != null)
        {
            foreach (var poeItem in poeItems)
            {
                MainWindow.PoeItemsName.Add(poeItem.ItemName);
                MainWindow.PoeItemsCategory.Add(poeItem.ItemType);
                MainWindow.PoeITemsChaosValue.Add(Math.Round(poeItem.ChaosEquivalent,0));
                MainWindow.NinjaItemIconLink.Add(poeItem.ImgUrl);
                MainWindow.NinjaItemLink.Add(poeItem.ItemUrl);
            } 
        }
    }
}