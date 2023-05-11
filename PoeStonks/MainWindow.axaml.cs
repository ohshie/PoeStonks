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
    public static ObservableCollection<double> PoeITemsChaosValue = new();
    public static ObservableCollection<Image> PoeItemsImage = new();
    public static ObservableCollection<string> NinjaImageTest = new();


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
        DisplayItemChaosValue.ItemsSource = PoeITemsChaosValue;
        DisplayItemIcon.ItemsSource = PoeItemsImage;
        DisplayItemNinjaIcon.ItemsSource = NinjaImageTest;

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
                try
                {
                    Process.Start(buttonUrl);
                }
                catch
                {
                    // hack because of this: https://github.com/dotnet/corefx/issues/10361
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        buttonUrl = buttonUrl.Replace("&", "^&");
                        Process.Start(new ProcessStartInfo(buttonUrl) { UseShellExecute = true });
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        Process.Start("xdg-open", buttonUrl);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        Process.Start("open", buttonUrl);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
    
}

