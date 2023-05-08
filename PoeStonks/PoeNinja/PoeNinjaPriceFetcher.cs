using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PoeStonks.Db;
using static PoeStonks.PoeNinja.NinjaDirectories;

namespace PoeStonks.PoeNinja;

public class PoeNinjaPriceFetcher
{
    readonly HttpClient _httpClient = new();

    private readonly string _baseNinjaUrl = "https://poe.ninja/api/data/";

    private List<PoeItem> _listOfAllItems = new();
    private DbOperations _dbOperations = new();
    
    public async Task<List<PoeItem>> FetchPricesFromNinja()
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

        DbToAllItemsDisplay dbToAllItemsDisplay = new();
        
        List<PoeItem> itemsToDisplay = dbToAllItemsDisplay.FetchItemsToDisplayInitialOrAfterUpdate(50);
        Console.WriteLine("done");
        return itemsToDisplay;
    }

    private async Task GetJsonFromNinja(string ninjaUrl, string itemType)
    {
        HttpResponseMessage responseMessage = await _httpClient.GetAsync(ninjaUrl);

        if (responseMessage.IsSuccessStatusCode)
        {
            string content = await responseMessage.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<NinjaResponse>(content);
            
            List<PoeItem> newItem;
            
            if (json?.Lines?.Count > 0 && json.Lines[0].CurrencyTypeName != null)
            {
                newItem = json.Lines.ConvertAll(j  =>
                {
                    var currencyDetail = json.CurrencyDetails?.FirstOrDefault(cd => cd.Name == j.CurrencyTypeName);
                    var iconUrl = currencyDetail?.itemIconUrl ?? string.Empty;
                    
                    return new PoeItem()
                    {
                        Id = j.Id!,
                        ItemName = j.CurrencyTypeName!,
                        ChaosEquivalent = j.ChaosEquivalent,
                        ItemType = itemType,
                        ImgUrl = iconUrl,
                        SparkLine = j.CurrencySparkLine.NinjaSparkLineData.ConvertAll(sl => new SparkLine()
                        {
                            SparkLineData = sl
                        })
                    };
                });
            }
            else
            {
                newItem = json.Lines.ConvertAll(j => new PoeItem()
                {
                    Id = j.Id!,
                    ItemName = j.Name!,
                    ChaosEquivalent = j.ChaosValue,
                    ItemType = itemType,
                    ImgUrl = j.itemIconUrl,
                    SparkLine = j.ItemSparkLine.NinjaSparkLineData.ConvertAll(sl => new SparkLine()
                    {
                        SparkLineData = sl
                    })
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
                urlBuilder = $"{_baseNinjaUrl}currencyoverview?league=Crucible&type={ninjaType.ToString()}";
                ninjaUrlList.Add((urlBuilder, ninjaType.ToString()));
                continue;
            }
            urlBuilder = $"{_baseNinjaUrl}itemoverview?league=Crucible&type={ninjaType.ToString()}";
            ninjaUrlList.Add((urlBuilder, ninjaType.ToString()));
        }

        return ninjaUrlList;
    }
}

class NinjaResponse
{
    [JsonPropertyName("lines")] 
    public List<NinjaJsonStructure>? Lines { get; set; }
    [JsonPropertyName("currencyDetails")] 
    public List<NinjaJsonStructure>? CurrencyDetails { get; set; }
}

class NinjaJsonStructure
{
    [JsonPropertyName("detailsId")]
    public string? Id { get; set; }
    [JsonPropertyName("currencyTypeName")]
    public string? CurrencyTypeName { get; set; }
    
    
    public NinjaJsonCurrencyUrl CurrencyIconUrl { get; set; }
    
    [JsonPropertyName("icon")] 
    public string itemIconUrl { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("chaosValue")]
    public double ChaosValue { get; set; }
    [JsonPropertyName("chaosEquivalent")]
    public double ChaosEquivalent { get; set; }
    [JsonPropertyName("receiveSparkLine")] 
    public NinjaJsonSparkLine CurrencySparkLine { get; set; }
    [JsonPropertyName("sparkline")] 
    public NinjaJsonSparkLine ItemSparkLine { get; set; }
}

class NinjaJsonSparkLine
{
    [JsonPropertyName("data")] 
    [JsonConverter(typeof(DoubleListWithNullHandlingConverter))]
    public List<double>? NinjaSparkLineData { get; set; }
}

class NinjaJsonCurrencyUrl
{
    [JsonPropertyName("icon")] 
    public string IconUrl { get; set; }
}

class DoubleListWithNullHandlingConverter : JsonConverter<List<double>>
{
    public override List<double>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        List<double> result = new List<double>();

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.Number)
                {
                    result.Add(reader.GetDouble());
                }
                else if (reader.TokenType == JsonTokenType.Null)
                {
                    result.Add(0);
                }
            }
        }
        
        return result;
    }

    public override void Write(Utf8JsonWriter writer, List<double> value, JsonSerializerOptions options)
    {
    }
}

