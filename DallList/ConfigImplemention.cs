namespace Dal;
using DalApi;
using DO;
using System;

/// <summary>
/// get, set and default
/// </summary>
internal class ConfigImplementation : IConfig
{
    public DateTime Clock
    {
        get => Config.Clock;
        set => Config.Clock = value;
    }

    public int AdminId
    {
        get => Config.AdminId;
        set => Config.AdminId = value;
    }
    public string AdminPassword
    {
        get => Config.AdminPassword;
        set => Config.AdminPassword = value;
    }

    public string? CompanyAddress
    {
        get => Config.CompanyAddress;
        set => Config.CompanyAddress = value;
    }
    public double? Latitude
    {
        get => Config.Latitude;
        set => Config.Latitude = value;
    }
    public double? Longitude
    {
        get => Config.Longitude;
        set => Config.Longitude = value;
    }

    public double? MaxAirDistance
    {
        get => Config.MaxAirDistance;
        set => Config.MaxAirDistance = value;
    }
    public double AvgCarSpeed
    {
        get => Config.AvgCarSpeed;
        set => Config.AvgCarSpeed = value;
    }
    public double AvgMotorcycleSpeed
    {
        get => Config.AvgMotorcycleSpeed;
        set => Config.AvgMotorcycleSpeed = value;
    }
    public double AvgBicylceSpeed
    {
        get => Config.AvgBicycleSpeed;
        set => Config.AvgBicycleSpeed = value;
    }
    public double AvgWalkSpeed
    {
        get => Config.AvgWalkSpeed;
        set => Config.AvgWalkSpeed = value;
    }

    public TimeSpan MaxDelTimeRnge
    {
        get => Config.MaxDelTimeRnge;
        set => Config.MaxDelTimeRnge = value;
    }
    public TimeSpan RiskTimeRnge
    {
        get => Config.RiskTimeRnge;
        set => Config.RiskTimeRnge = value;
    }
    public TimeSpan UnactiveTimeRnge
    {
        get => Config.UnactiveTimeRnge;
        set => Config.UnactiveTimeRnge = value;
    }

    public void Reset()
    {
        Config.Reset();
    }
}

