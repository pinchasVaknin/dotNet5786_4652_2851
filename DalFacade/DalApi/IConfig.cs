namespace DalApi;

public interface IConfig
{
    //==================== System Clock ===================\\

    #region SystemClock

    // Gets or sets the current simulated system time
    DateTime Clock { get; set; }

    #endregion SystemClock

    //==================== Admin Credentials ===================\\

    #region AdminCredentials

    // Administrator's ID for login
    int AdminId { get; set; }

    // Administrator's password
    string AdminPassword { get; set; }

    #endregion AdminCredentials

    //==================== Company Location ===================\\

    #region CompanyLocation

    // Textual address of the company headquarters
    string? CompanyAddress { get; set; }

    // Latitude coordinate of the company headquarters
    double? Latitude { get; set; }

    // Longitude coordinate of the company headquarters
    double? Longitude { get; set; }

    #endregion CompanyLocation

    //==================== Operational Parameters ===================\\

    #region OperationalParameters

    // Maximum air distance allowed for any delivery
    double? MaxAirDistance { get; set; }

    // Average speed (km/h) for cars
    double AvgCarSpeed { get; set; }

    // Average speed (km/h) for motorcycles
    double AvgMotorcycleSpeed { get; set; }

    // Average speed (km/h) for bicycles
    double AvgBicycleSpeed { get; set; }

    // Average walking speed (km/h)
    double AvgWalkSpeed { get; set; }

    #endregion OperationalParameters

    //==================== Time Policies ===================\\

    #region TimePolicies

    // Max allowed time range for a delivery to be considered "On Time"
    TimeSpan MaxDelTimeRnge { get; set; }

    // Time buffer before the deadline when a delivery status becomes "In Risk"
    TimeSpan RiskTimeRnge { get; set; }

    // Duration of inactivity after which a courier is considered inactive
    TimeSpan UnactiveTimeRnge { get; set; }

    #endregion TimePolicies

    //==================== Management Methods ===================\\

    #region Management

    /// <summary>
    /// Resets all configuration values to their initial defaults.
    /// </summary>
    void Reset();

    #endregion Management

}