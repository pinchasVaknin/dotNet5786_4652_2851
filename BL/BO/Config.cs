namespace BO;

public class Config
{
    public DateTime Clock { get; set; }
    public int AdminId { get; set; }
    public string AdminPassword { get; set; }

    public string? CompanyAddress { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public double? MaxAirDistance { get; set; }
    public double AvgCarSpeed { get; set; }
    public double AvgMotorcycleSpeed { get; set; }
    public double AvgBicycleSpeed { get; set; }
    public double AvgWalkSpeed { get; set; }

    public TimeSpan MaxDelTimeRnge { get; set; }
    public TimeSpan RiskTimeRnge { get; set; }
    public TimeSpan UnactiveTimeRnge { get; set; }
}
