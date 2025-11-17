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

    public int adminId
    {
        get => Config.adminId;
        set => Config.adminId = value;
    }

    public string adminPassword
    {
        get => Config.adminPassword;
        set => Config.adminPassword = value;
    }

    public string? companyAdress
    {
        get => Config.companyAdress;
        set => Config.companyAdress = value;
    }

    public double? latitude
    {
        get => Config.latitude;
        set => Config.latitude = value;
    }

    public double? longitude
    {
        get => Config.longitude;
        set => Config.longitude = value;
    }

    public double? maxAirDistance
    {
        get => Config.maxAirDistance;
        set => Config.maxAirDistance = value;
    }

    public double avgCarSpeed
    {
        get => Config.avgCarSpeed;
        set => Config.avgCarSpeed = value;
    }

    public double avgMotorcycleSpeed
    {
        get => Config.avgMotorcycleSpeed;
        set => Config.avgMotorcycleSpeed = value;
    }

    public double avgBicyleSpeed
    {
        get => Config.avgBicyleSpeed;
        set => Config.avgBicyleSpeed = value;
    }

    public double avgWalkSpeed
    {
        get => Config.avgWalkSpeed;
        set => Config.avgWalkSpeed = value;
    }

    public TimeSpan maxDelTimeRnge
    {
        get => Config.maxDelTimeRnge;
        set => Config.maxDelTimeRnge = value;
    }

    public TimeSpan riskTimeRnge
    {
        get => Config.riskTimeRnge;
        set => Config.riskTimeRnge = value;
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

