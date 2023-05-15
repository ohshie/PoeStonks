using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DynamicData;
using PoeStonks.AllItemsDisplay;
using PoeStonks.Db;
using PoeStonks.PoeNinja;
using ReactiveUI;


namespace PoeStonks;

public partial class MainWindow : Window
{
    // view properties
    private ObservableCollection<string> ItemsNames = new();
    private ObservableCollection<string> ItemsCategories = new();
    private ObservableCollection<double> ItemsChaosValues = new();
    private ObservableCollection<string> ItemsUrl = new();
    private ObservableCollection<string> ItemsPicturesUrl = new();
    private ObservableCollection<double> ItemsDivineValues = new();
    
    public MainWindow()
    {
        InitializeComponent();
        
        DataContext = this;
        
        DisplayItemsNames.ItemsSource = ItemsNames;
        DisplayItemsCategories.ItemsSource = ItemsCategories;
        DisplayItemsChaosValue.ItemsSource = ItemsChaosValues;
        DisplayItemsUrls.ItemsSource = ItemsUrl;
        DisplayItemIcons.ItemsSource = ItemsPicturesUrl;
        DisplayItemsDivineValue.ItemsSource = ItemsDivineValues;
        
        PopulateDisplayWithDefaultValues();
        Logger.LogMessageOutput ="Ready";
    }
    
    private async void Button_FetchItemsFromPoeNinja(object? sender, RoutedEventArgs e)
    {
        // start logging
        Logger.LogMessageOutputChanged += OnLogMessageOutputChanged;

        NinjaItemsDisplay ninjaItemsDisplay = new();
        
        Logger.LogMessageOutput ="Getting data from poe.ninja";
        List<PoeItem> freshNinjaData = await ninjaItemsDisplay.GetFreshNinjaData();
        
        Logger.LogMessageOutput ="Updating Db";
        await ninjaItemsDisplay.PopulateOrUpdateDb(freshNinjaData);
        
        PopulateDisplayWithDefaultValues();
    }

    private void PopulateDisplayWithDefaultValues()
    {
        NinjaItemsDisplay ninjaItemsDisplay = new();
        
        Logger.LogMessageOutput ="Fetching items to display";
        List<PoeItem> itemsFromDb = ninjaItemsDisplay.FetchItemsDefault();
        double currentDivinePrice = ninjaItemsDisplay.GetCurrentDivinePrice();
        
        Logger.LogMessageOutput ="Ready";
        if (currentDivinePrice != null && itemsFromDb != null)
        {
            PoeItemsListToDisplay(itemsFromDb, currentDivinePrice);
        }
    }
    
    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            string? buttonUrl = button.DataContext?.ToString();
            if (buttonUrl != null)
            {
                OpenUrlInBrowser(buttonUrl);
            }
        }
    }

    private void PoeItemsListToDisplay(List<PoeItem> poeItems, double currentDivinePrice)
    {
        foreach (var poeItem in poeItems)
        {
            if (poeItem.ItemName?.Length > 25) ItemsNames.Add($"{poeItem.ItemName.Substring(0,23)}...");
            else ItemsNames.Add(poeItem.ItemName);

            ItemsCategories.Add(poeItem.ItemType);
            ItemsChaosValues.Add(Math.Round(poeItem.ChaosEquivalent,0));
            ItemsPicturesUrl.Add(poeItem.ImgUrl);
            ItemsUrl.Add(poeItem.ItemUrl);
            ItemsDivineValues.Add(Math.Round(poeItem.ChaosEquivalent/currentDivinePrice,2));
        } 
    }
    
    private void OpenUrlInBrowser(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }
    
    private void OnLogMessageOutputChanged(string newLogMessageOutput)
    {
        ConsoleOutPutTextBlock.Text = newLogMessageOutput;
    }
}

