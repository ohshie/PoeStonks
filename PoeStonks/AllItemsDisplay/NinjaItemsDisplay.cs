using PoeStonks.Db;
using PoeStonks.PoeNinja;

namespace PoeStonks.AllItemsDisplay;

public class NinjaItemsDisplay
{
    private readonly DbToItemsDisplay _dbToAllItemsDisplay = new();

    public async Task<List<PoeItem>> GetFreshNinjaData()
    {
        PoeNinjaPriceFetcher ninjaPriceFetcher = new();
        List<PoeItem> freshNinjaData = await ninjaPriceFetcher.FetchPricesFromNinja();

        return freshNinjaData;
    }

    public async Task PopulateOrUpdateDb(List<PoeItem> freshNinjaData)
    {
        DbOperations dbOperations = new();
        await dbOperations.AddOrUpdateNinjaData(freshNinjaData);
    }
    
    public List<PoeItem> FetchItemsDefault()
    {
        DbToItemsDisplay dbToAllItemsDisplay = new();
        List<PoeItem> listOfItems = dbToAllItemsDisplay.FetchItemsSortedByChaos(50);
        
        return listOfItems;
    }

    public double GetCurrentDivinePrice()
    {
        double divinePrice = _dbToAllItemsDisplay.FetchDivinePrice();
        if (divinePrice != null) return divinePrice;

        return 0;
    }
}