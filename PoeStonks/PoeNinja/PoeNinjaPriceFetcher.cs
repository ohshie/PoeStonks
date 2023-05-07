using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PoeStonks.Db;
using static PoeStonks.PoeNinja.NinjaDirectories;

namespace PoeStonks.PoeNinja;

public class PoeNinjaPriceFetcher
{
    HttpClient httpClient = new();
    
    private string BaseNinjaUrl = "https://poe.ninja/api/data/";

    private List<AllPoeItems> _listOfAllItems = new();
    private DbOperations _dbOperations = new();
    
    public async Task FetchPricesFromNinja()
    {
        List<Task> fetchinFromPoeNinja = new List<Task>();

        List<(string url, string itemType)> ninjaUrlList = CreateNinjaUrls();

        foreach (var (url, itemType) in ninjaUrlList)
        {
            fetchinFromPoeNinja.Add(GetJsonFromNinja(url, itemType));
        }

        await Task.WhenAll(fetchinFromPoeNinja);
        Console.WriteLine("done");
        
        await _dbOperations.AddOrUpdateNinjaData(_listOfAllItems);
        
        Console.WriteLine("done");
    }

    private async Task GetJsonFromNinja(string ninjaUrl, string itemType)
    {
        HttpResponseMessage responseMessage = await httpClient.GetAsync(ninjaUrl);

        if (responseMessage.IsSuccessStatusCode)
        {
            string content = await responseMessage.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<NinjaResponse>(content);
            
            List<AllPoeItems> newItem;
            
            if (json.Lines.Count > 0 && json.Lines[0].CurrencyTypeName != null)
            {
                newItem = json.Lines.ConvertAll(j => new AllPoeItems()
                {
                    ItemName = j.CurrencyTypeName,
                    ChaosValue = j.ChaosEquivalent,
                    ListingCount = j.ListingCount,
                    ItemType = itemType
                });
            }
            else
            {
                newItem = json.Lines.ConvertAll(j => new AllPoeItems()
                {
                    ItemName = j.Name,
                    ChaosValue = j.ChaosValue,
                    ListingCount = j.ListingCount,
                    ItemType = itemType
                });
            }

            Console.WriteLine(itemType);
            _listOfAllItems.AddRange(newItem);
        }
    }

    private List<(string url, string ItemType)> CreateNinjaUrls()
    {
        List<(string url, string itemType)> ninjaUrlList = new();
        
        foreach (NinjaDirectories ninjaType in Enum.GetValues(typeof(NinjaDirectories)))
        {
            string urlBuilder;
            if (ninjaType is Currency or Fragment)
            {
                urlBuilder = $"{BaseNinjaUrl}currencyoverview?league=Crucible&type={ninjaType.ToString()}";
                ninjaUrlList.Add((urlBuilder, ninjaType.ToString()));
                continue;
            }
            urlBuilder = $"{BaseNinjaUrl}itemoverview?league=Crucible&type={ninjaType.ToString()}";
            ninjaUrlList.Add((urlBuilder, ninjaType.ToString()));
        }

        return ninjaUrlList;
    }
}

class NinjaResponse
{
    [JsonPropertyName("lines")] 
    public List<NinjaJsonStructure> Lines { get; set; }
}

class NinjaJsonStructure
{
    [JsonPropertyName("currencyTypeName")]
    public string CurrencyTypeName { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("chaosValue")]
    public double ChaosValue { get; set; }
    [JsonPropertyName("listingCount")]
    public int ListingCount { get; set; }
    [JsonPropertyName("chaosEquivalent")]
    public double ChaosEquivalent { get; set; }
}

public class AllPoeItems
{
    public string ItemName { get; set; }
    public double ChaosValue { get; set; }
    public int ListingCount { get; set; }
    public string ItemType { get; set; }
}

