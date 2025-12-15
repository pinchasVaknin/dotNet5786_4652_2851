namespace Dal;
using DalApi;
using System;

//==================== Config Implementation (List) ===================\\

/// <summary>
/// Implementation of the IConfig interface for the DalList layer.
/// Acts as a wrapper around the static, internal Config class to expose settings via the interface.
/// </summary>
internal class ConfigImplementation : IConfig
{
    //==================== System Clock ===================\\

    #region SystemClock

    // Current simulated system time
    public DateTime Clock
    {
        get => Config.Clock;
        set => Config.Clock = value;
    }

    #endregion SystemClock

    //==================== Admin Credentials ===================\\

    #region AdminCredentials

    // Administrator's ID
    public int AdminId
    {
        get => Config.AdminId;
        set => Config.AdminId = value;
    }

    // Administrator's Password
    public string AdminPassword
    {
        get => Config.AdminPassword;
        set => Config.AdminPassword = value;
    }

    #endregion AdminCredentials

    //==================== Company Location ===================\\

    #region CompanyLocation

    // Company address text
    public string? CompanyAddress
    {
        get => Config.CompanyAddress;
        set => Config.CompanyAddress = value;
    }

    // Company Latitude
    public double? Latitude
    {
        get => Config.Latitude;
        set => Config.Latitude = value;
    }

    // Company Longitude
    public double? Longitude
    {
        get => Config.Longitude;
        set => Config.Longitude = value;
    }

    #endregion CompanyLocation

    //==================== Operational Parameters ===================\\

    #region OperationalParameters

    // Max allowed air distance for deliveries
    public double? MaxAirDistance
    {
        get => Config.MaxAirDistance;
        set => Config.MaxAirDistance = value;
    }

    // Average speed for Car (km/h)
    public double AvgCarSpeed
    {
        get => Config.AvgCarSpeed;
        set => Config.AvgCarSpeed = value;
    }

    // Average speed for Motorcycle (km/h)
    public double AvgMotorcycleSpeed
    {
        get => Config.AvgMotorcycleSpeed;
        set => Config.AvgMotorcycleSpeed = value;
    }

    // Average speed for Bicycle (km/h)
    public double AvgBicycleSpeed
    {
        get => Config.AvgBicycleSpeed;
        set => Config.AvgBicycleSpeed = value;
    }

    // Average speed for Walking (km/h)
    public double AvgWalkSpeed
    {
        get => Config.AvgWalkSpeed;
        set => Config.AvgWalkSpeed = value;
    }

    #endregion OperationalParameters

    //==================== Time Policies ===================\\

    #region TimePolicies

    // Max delivery time range
    public TimeSpan MaxDelTimeRnge
    {
        get => Config.MaxDelTimeRnge;
        set => Config.MaxDelTimeRnge = value;
    }

    // Risk time buffer
    public TimeSpan RiskTimeRnge
    {
        get => Config.RiskTimeRnge;
        set => Config.RiskTimeRnge = value;
    }

    // Inactivity timeout range
    public TimeSpan UnactiveTimeRnge
    {
        get => Config.UnactiveTimeRnge;
        set => Config.UnactiveTimeRnge = value;
    }

    #endregion TimePolicies

    //==================== Management ===================\\

    #region Management

    /// <summary>
    /// Resets configuration to default values via the static Config class.
    /// </summary>
    public void Reset()
    {
        Config.Reset();
    }

    #endregion Management
}