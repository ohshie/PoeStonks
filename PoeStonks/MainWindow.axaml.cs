using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PoeStonks.Db;
using PoeStonks.PoeNinja;

namespace PoeStonks;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        using PsDbContext dbContext = new PsDbContext();
        {
            dbContext.Database.EnsureCreated();
        }

        InitializeComponent();
    }

    private async void Button_FetchItemsFromPoeNinja(object? sender, RoutedEventArgs e)
    {
        PoeNinjaPriceFetcher priceFetcher = new();

        await priceFetcher.FetchPricesFromNinja();
    }
}