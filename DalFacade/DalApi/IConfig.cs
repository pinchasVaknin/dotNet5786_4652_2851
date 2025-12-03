namespace DalApi;

public interface IConfig
{
    DateTime Clock { get; set; }
    int AdminId { get; set; }
    string AdminPassword { get; set; }


    string? CompanyAddress { get; set; }
    double? Latitude { get; set; }
    double? Longitude { get; set; }


    double? MaxAirDistance { get; set; }
    double AvgCarSpeed { get; set; }
    double AvgMotorcycleSpeed { get; set; }
    double AvgBicylceSpeed { get; set; }
    double AvgWalkSpeed { get; set; }

    TimeSpan MaxDelTimeRnge { get; set; }
    TimeSpan RiskTimeRnge { get; set; }
    TimeSpan UnactiveTimeRnge { get; set; }

    void Reset();

}
