namespace DalApi;


public interface IConfig
{
    DateTime Clock { get; set; }
    int adminId { get; set; }
    string adminPassword { get; set; }


    string? companyAdress { get; set; }
    double? latitude { get; set; }
    double? longitude { get; set; }


    double? maxAirDistance { get; set; }
    double avgCarSpeed { get; set; }
    double avgMotorcycleSpeed { get; set; }
    double avgBicyleSpeed { get; set; }
    double avgWalkSpeed { get; set; }

    TimeSpan maxDelTimeRnge { get; set; }
    TimeSpan riskTimeRnge { get; set; }
    TimeSpan UnactiveTimeRnge { get; set; }

    void Reset();

}
