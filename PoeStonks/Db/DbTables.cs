namespace PoeStonks.Db;

public class PoeItem
{
    public int Id { get; set; }
    public string ItemType { get; set; }
    public string ItemName { get; set; }
    public int ListingAmount { get; set; }
    public double ChaosEquivalent { get; set; }
}