namespace Dal;
using DalApi;

/// <summary>
/// get, set and default
/// </summary>
internal class ConfigImplementation : IConfig
{

    //------------------ set/get Config static functions ------------------\\
    /// <summary>
    /// Gets or sets the shared application clock value from the underlying Config.
    /// </summary>
    public DateTime Clock
    {
        get => Config.Clock;
        set => Config.Clock = value;
    }

    /// <summary>
    /// Gets or sets the administrator identifier stored in the underlying Config.
    /// </summary>
    public int AdminId
    {
        get => Config.AdminId;
        set => Config.AdminId = value;
    }

    /// <summary>
    /// Gets or sets the administrator password stored in the underlying Config.
    /// </summary>
    public string AdminPassword
    {
        get => Config.AdminPassword;
        set => Config.AdminPassword = value;
    }

    /// <summary>
    /// Gets or sets the company address (nullable) from the underlying Config.
    /// </summary>
    public string? CompanyAddress
    {
        get => Config.CompanyAddress;
        set => Config.CompanyAddress = value;
    }

    /// <summary>
    /// Gets or sets the company latitude (nullable) from the underlying Config.
    /// </summary>
    public double? Latitude
    {
        get => Config.Latitude;
        set => Config.Latitude = value;
    }

    /// <summary>
    /// Gets or sets the company longitude (nullable) from the underlying Config.
    /// </summary>
    public double? Longitude
    {
        get => Config.Longitude;
        set => Config.Longitude = value;
    }

    /// <summary>
    /// Gets or sets the maximum allowed air distance (nullable) from the underlying Config.
    /// </summary>
    public double? MaxAirDistance
    {
        get => Config.MaxAirDistance;
        set => Config.MaxAirDistance = value;
    }

    /// <summary>
    /// Gets or sets the average car speed used for time and delivery calculations.
    /// </summary>
    public double AvgCarSpeed
    {
        get => Config.AvgCarSpeed;
        set => Config.AvgCarSpeed = value;
    }

    /// <summary>
    /// Gets or sets the average motorcycle speed used for time and delivery calculations.
    /// </summary>
    public double AvgMotorcycleSpeed
    {
        get => Config.AvgMotorcycleSpeed;
        set => Config.AvgMotorcycleSpeed = value;
    }

    /// <summary>
    /// Gets or sets the average bicycle speed used for time and delivery calculations.
    /// </summary>
    public double AvgBicycleSpeed
    {
        get => Config.AvgBicycleSpeed;
        set => Config.AvgBicycleSpeed = value;
    }

    /// <summary>
    /// Gets or sets the average walking speed used for time and delivery calculations.
    /// </summary>
    public double AvgWalkSpeed
    {
        get => Config.AvgWalkSpeed;
        set => Config.AvgWalkSpeed = value;
    }

    /// <summary>
    /// Gets or sets the maximum delivery time range from the underlying Config.
    /// </summary>
    public TimeSpan MaxDelTimeRnge
    {
        get => Config.MaxDelTimeRnge;
        set => Config.MaxDelTimeRnge = value;
    }

    /// <summary>
    /// Gets or sets the risk time range used to identify high-risk deliveries.
    /// </summary>
    public TimeSpan RiskTimeRnge
    {
        get => Config.RiskTimeRnge;
        set => Config.RiskTimeRnge = value;
    }

    /// <summary>
    /// Gets or sets the time range after which a courier is considered unactive.
    /// </summary>
    public TimeSpan UnactiveTimeRnge
    {
        get => Config.UnactiveTimeRnge;
        set => Config.UnactiveTimeRnge = value;
    }


    //------------------------ Reset Config ------------------------\\
    /// <summary>
    /// Resets all configuration values to defaults by delegating to <see cref="Config.Reset"/>.
    /// </summary>
    public void Reset()
    {
        Config.Reset();
    }
}
