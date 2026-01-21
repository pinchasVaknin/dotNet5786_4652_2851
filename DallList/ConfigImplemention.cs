namespace Dal;
using DalApi;
using System;
using System.Runtime.CompilerServices;

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
        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        get => Config.Clock;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        set => Config.Clock = value;
    }

    #endregion SystemClock

    //==================== Admin Credentials ===================\\

    #region AdminCredentials

    // Administrator's ID
    public int AdminId
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        get => Config.AdminId;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        set => Config.AdminId = value;
    }

    // Administrator's Password
    public string AdminPassword
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        get => Config.AdminPassword;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        set => Config.AdminPassword = value;
    }

    #endregion AdminCredentials

    //==================== Company Location ===================\\

    #region CompanyLocation

    // Company address text
    public string? CompanyAddress
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        get => Config.CompanyAddress;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        set => Config.CompanyAddress = value;
    }

    // Company Latitude
    public double? Latitude
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        get => Config.Latitude;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        set => Config.Latitude = value;
    }

    // Company Longitude
    public double? Longitude
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        get => Config.Longitude;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        set => Config.Longitude = value;
    }

    #endregion CompanyLocation

    //==================== Operational Parameters ===================\\

    #region OperationalParameters

    // Max allowed air distance for deliveries
    public double? MaxAirDistance
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        get => Config.MaxAirDistance;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        set => Config.MaxAirDistance = value;
    }

    // Average speed for Car (km/h)
    public double AvgCarSpeed
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        get => Config.AvgCarSpeed;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        set => Config.AvgCarSpeed = value;
    }

    // Average speed for Motorcycle (km/h)
    public double AvgMotorcycleSpeed
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        get => Config.AvgMotorcycleSpeed;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        set => Config.AvgMotorcycleSpeed = value;
    }

    // Average speed for Bicycle (km/h)
    public double AvgBicycleSpeed
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        get => Config.AvgBicycleSpeed;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        set => Config.AvgBicycleSpeed = value;
    }

    // Average speed for Walking (km/h)
    public double AvgWalkSpeed
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        get => Config.AvgWalkSpeed;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        set => Config.AvgWalkSpeed = value;
    }

    #endregion OperationalParameters

    //==================== Time Policies ===================\\

    #region TimePolicies

    // Max delivery time range
    public TimeSpan MaxDelTimeRnge
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        get => Config.MaxDelTimeRnge;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        set => Config.MaxDelTimeRnge = value;
    }

    // Risk time buffer
    public TimeSpan RiskTimeRnge
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        get => Config.RiskTimeRnge;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        set => Config.RiskTimeRnge = value;
    }

    // Inactivity timeout range
    public TimeSpan UnactiveTimeRnge
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        get => Config.UnactiveTimeRnge;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage7
        set => Config.UnactiveTimeRnge = value;
    }

    #endregion TimePolicies

    //==================== Management ===================\\

    #region Management

    /// <summary>
    /// Resets configuration to default values via the static Config class.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage7
    public void Reset()
    {
        Config.Reset();
    }

    #endregion Management
}