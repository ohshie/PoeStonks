using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
    public static ObservableCollection<string> PoeItemsName = new();
    public static ObservableCollection<string> PoeItemsCategory = new();
    public static ObservableCollection<double> PoeItemsChaosValue = new();
    public static ObservableCollection<Image> PoeItemsImage = new();
    public static ObservableCollection<string> NinjaItemLink = new();
    public static ObservableCollection<string> NinjaItemIconLink = new();
    public static ObservableCollection<double> PoeItemsDivineEquivalent = new();

    private readonly NinjaItemsDisplay _ninjaItemsDisplay = new();
    
    public MainWindow()
    {
        using PsDbContext dbContext = new PsDbContext();
        {
            dbContext.Database.EnsureCreated();
        }
        
        InitializeComponent();
        
        DataContext = this;
        
        var test = DisplayItemChaosValue.ItemsSource;
        DisplayItemName.ItemsSource = PoeItemsName;
        DisplayItemCategory.ItemsSource = PoeItemsCategory;
        DisplayItemChaosValue.ItemsSource = PoeItemsChaosValue;
        DisplayItemIcon.ItemsSource = PoeItemsImage;
        DisplayItemNinjaLink.ItemsSource = NinjaItemLink;
        DisplayItemIcon.ItemsSource = NinjaItemIconLink;
        DisplayItemDivineEquivalent.ItemsSource = PoeItemsDivineEquivalent;

        _ninjaItemsDisplay.NinjaItemsDisplayFill();
    }
    
    private async void Button_FetchItemsFromPoeNinja(object? sender, RoutedEventArgs e)
    {
        PoeNinjaPriceFetcher priceFetcher = new(this);

        await priceFetcher.FetchPricesFromNinja();
        _ninjaItemsDisplay.NinjaItemsDisplayFill();
    }

    public void PseudoLog(string logMessage)
    {
        ConsoleOutPutTextBlock.Text = logMessage;
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
}

