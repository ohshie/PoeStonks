using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PoeStonks.Db;
using static PoeStonks.PoeNinja.NinjaDirectoriesApi;

namespace PoeStonks.PoeNinja;

public class PoeNinjaPriceFetcher
{
    private MainWindow MainWindow { get; set; }

    public PoeNinjaPriceFetcher(MainWindow mainWindow)
    {
        MainWindow = mainWindow;
        _dbOperations = new DbOperations(mainWindow);
    }

    readonly HttpClient _httpClient = new();

    private readonly string _baseNinjaUrl = "https://poe.ninja/api/data/";

    private List<PoeItem> _listOfAllItems = new();
    private DbOperations _dbOperations;
    
    public async Task<List<PoeItem>> FetchPricesFromNinja()
    {
        List<Task> fetchinFromPoeNinja = new List<Task>();

        List<(string url, string itemType)> ninjaUrlList = CreateNinjaTypeUrls();

        foreach (var (url, itemType) in ninjaUrlList)
        {
            fetchinFromPoeNinja.Add(GetJsonFromNinja(url, itemType));
        }

        await Task.WhenAll(fetchinFromPoeNinja);

        // bottom log
        MainWindow.PseudoLog("Received poe.ninja data");
        
        await _dbOperations.AddOrUpdateNinjaData(_listOfAllItems);

        DbToAllItemsDisplay dbToAllItemsDisplay = new();
        
        List<PoeItem> itemsToDisplay = dbToAllItemsDisplay.FetchItemsToDisplayInitialOrAfterUpdate(50);
        
        // bottom log
        MainWindow.PseudoLog("Prices updated.");
        
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
                        ItemUrl = CreateNinjaItemUrl(itemType,j.Id!),
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
                    ItemUrl = CreateNinjaItemUrl(itemType,j.Id!),
                    SparkLine = j.ItemSparkLine.NinjaSparkLineData.ConvertAll(sl => new SparkLine()
                    {
                        SparkLineData = sl
                    })
                });
            }

            // bottom log
            MainWindow.PseudoLog($"{itemType} data fetched from poe.ninja");
            
            _listOfAllItems.AddRange(newItem);
        }
    }
    
    private string CreateNinjaItemUrl(string itemType, string itemName)
    {
        string itemUrl = $"{baseUrlForItems[Enum.Parse<NinjaDirectoriesApi>(itemType)]}/{itemName}";
        return itemUrl;
    }

    private List<(string url, string ItemType)> CreateNinjaTypeUrls()
    {
        List<(string url, string itemType)> ninjaUrlList = new();
        
        foreach (NinjaDirectoriesApi ninjaType in Enum.GetValues(typeof(NinjaDirectoriesApi)))
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
    
     Dictionary<NinjaDirectoriesApi, string> baseUrlForItems = new Dictionary<NinjaDirectoriesApi, string>
        {
            { Currency, "https://poe.ninja/challenge/currency" },
            { NinjaDirectoriesApi.Fragment, "https://poe.ninja/challenge/fragments" },
            { NinjaDirectoriesApi.DivinationCard, "https://poe.ninja/challenge/divination-cards" },
            { NinjaDirectoriesApi.Artifact, "https://poe.ninja/challenge/artifacts" },
            { NinjaDirectoriesApi.Oil, "https://poe.ninja/challenge/oils" },
            { NinjaDirectoriesApi.Incubator, "https://poe.ninja/challenge/incubarots" },
            { NinjaDirectoriesApi.UniqueWeapon, "https://poe.ninja/challenge/unique-weapons" },
            { NinjaDirectoriesApi.UniqueArmour, "https://poe.ninja/challenge/unique-armours" },
            { NinjaDirectoriesApi.UniqueAccessory, "https://poe.ninja/challenge/unique-accessory" },
            { NinjaDirectoriesApi.UniqueFlask, "https://poe.ninja/challenge/unique-flasks" },
            { NinjaDirectoriesApi.UniqueJewel, "https://poe.ninja/challenge/unique-jewels" },
            { NinjaDirectoriesApi.SkillGem, "https://poe.ninja/challenge/skill-gems" },
            { NinjaDirectoriesApi.ClusterJewel, "https://poe.ninja/challenge/cluster-jewels" },
            { NinjaDirectoriesApi.Map, "https://poe.ninja/challenge/maps" },
            { NinjaDirectoriesApi.BlightedMap, "https://poe.ninja/challenge/blighted-maps" },
            { NinjaDirectoriesApi.BlightRavagedMap, "https://poe.ninja/challenge/bligh-ravaged-maps" },
            { NinjaDirectoriesApi.DeliriumOrb, "https://poe.ninja/challenge/delirium-orbs" },
            { NinjaDirectoriesApi.UniqueMap, "https://poe.ninja/challenge/unique-maps" },
            { NinjaDirectoriesApi.Invitation, "https://poe.ninja/challenge/invitations" },
            { NinjaDirectoriesApi.Scarab, "https://poe.ninja/challenge/scarabs" },
            { NinjaDirectoriesApi.Fossil, "https://poe.ninja/challenge/fossils" },
            { NinjaDirectoriesApi.Resonator, "https://poe.ninja/challenge/resonators" },
            { NinjaDirectoriesApi.HelmetEnchant, "https://poe.ninja/challenge/helment-enchants" },
            { NinjaDirectoriesApi.Beast, "https://poe.ninja/challenge/beatst" },
            { NinjaDirectoriesApi.Essence, "https://poe.ninja/challenge/essences" },
            { NinjaDirectoriesApi.Vial, "https://poe.ninja/challenge/vials" },
        };
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

    public override void Write(Utf8JsonWriter writer, List<double> value, JsonSerializerOptions options) {}
}

