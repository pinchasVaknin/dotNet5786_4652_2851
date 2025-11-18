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
    public int adminId
    {
        get => Config.adminId;
        set => Config.adminId = value;
    }

    /// <summary>
    /// Gets or sets the administrator password stored in the underlying Config.
    /// </summary>
    public string adminPassword
    {
        get => Config.adminPassword;
        set => Config.adminPassword = value;
    }

    /// <summary>
    /// Gets or sets the company address (nullable) from the underlying Config.
    /// </summary>
    public string? companyAdress
    {
        get => Config.companyAdress;
        set => Config.companyAdress = value;
    }

    /// <summary>
    /// Gets or sets the company latitude (nullable) from the underlying Config.
    /// </summary>
    public double? latitude
    {
        get => Config.latitude;
        set => Config.latitude = value;
    }

    /// <summary>
    /// Gets or sets the company longitude (nullable) from the underlying Config.
    /// </summary>
    public double? longitude
    {
        get => Config.longitude;
        set => Config.longitude = value;
    }

    /// <summary>
    /// Gets or sets the maximum allowed air distance (nullable) from the underlying Config.
    /// </summary>
    public double? maxAirDistance
    {
        get => Config.maxAirDistance;
        set => Config.maxAirDistance = value;
    }

    /// <summary>
    /// Gets or sets the average car speed used for time and delivery calculations.
    /// </summary>
    public double avgCarSpeed
    {
        get => Config.avgCarSpeed;
        set => Config.avgCarSpeed = value;
    }

    /// <summary>
    /// Gets or sets the average motorcycle speed used for time and delivery calculations.
    /// </summary>
    public double avgMotorcycleSpeed
    {
        get => Config.avgMotorcycleSpeed;
        set => Config.avgMotorcycleSpeed = value;
    }

    /// <summary>
    /// Gets or sets the average bicycle speed used for time and delivery calculations.
    /// </summary>
    public double avgBicyleSpeed
    {
        get => Config.avgBicyleSpeed;
        set => Config.avgBicyleSpeed = value;
    }

    /// <summary>
    /// Gets or sets the average walking speed used for time and delivery calculations.
    /// </summary>
    public double avgWalkSpeed
    {
        get => Config.avgWalkSpeed;
        set => Config.avgWalkSpeed = value;
    }

    /// <summary>
    /// Gets or sets the maximum delivery time range from the underlying Config.
    /// </summary>
    public TimeSpan maxDelTimeRnge
    {
        get => Config.maxDelTimeRnge;
        set => Config.maxDelTimeRnge = value;
    }

    /// <summary>
    /// Gets or sets the risk time range used to identify high-risk deliveries.
    /// </summary>
    public TimeSpan riskTimeRnge
    {
        get => Config.riskTimeRnge;
        set => Config.riskTimeRnge = value;
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
