using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PoeStonks.AllItemsDisplay;
using PoeStonks.Db;
using PoeStonks.PoeNinja;

namespace PoeStonks;

public partial class MainWindow : Window
{
    public static ObservableCollection<string> PoeItemsName = new();
    public static ObservableCollection<string> PoeItemsCategory = new();
    public static ObservableCollection<double> PoeITemsChaosValue = new();
    public static ObservableCollection<Image> PoeItemsImage = new();

    private readonly NinjaItemsDisplay _ninjaItemsDisplay = new();
    
    public MainWindow()
    {
        using PsDbContext dbContext = new PsDbContext();
        {
            dbContext.Database.EnsureCreated();
        }
        InitializeComponent();
        
        DataContext = this;
        
        DisplayItemName.Items = PoeItemsName;
        DisplayItemCategory.Items = PoeItemsCategory;
        DisplayItemChaosValue.Items = PoeITemsChaosValue;
        DisplayItemIcon.Items = PoeItemsImage;
        
        _ninjaItemsDisplay.NinjaItemsDisplayFill();
    }

    private async void Button_FetchItemsFromPoeNinja(object? sender, RoutedEventArgs e)
    {
        PoeNinjaPriceFetcher priceFetcher = new();

        await priceFetcher.FetchPricesFromNinja();
        _ninjaItemsDisplay.NinjaItemsDisplayFill();
    }
    
}