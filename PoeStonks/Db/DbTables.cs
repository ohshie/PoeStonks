namespace PoeStonks.Db;

public class PoeItem
{
    public string? Id { get; set; }
    public string? ImgUrl { get; set; }
    public string? ItemUrl { get; set; }
    public string? ItemType { get; set; }
    public string? ItemName { get; set; }
    public double ChaosEquivalent { get; set; }
    public double UserChaosValue { get; set; }
    public List<SparkLine>? SparkLine { get; set; } = new();
}

public class SparkLine
{
    [Key]
    public int Id { get; set; }
    public double SparkLineData { get; set; }
}
