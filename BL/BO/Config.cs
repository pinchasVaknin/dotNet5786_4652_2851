namespace BO;
using Helpers;
using System;

//==================== Configuration Entity ===================\\

/// <summary>
/// Represents the global system configuration settings.
/// Includes system clock, admin credentials, company location, and operational parameters.
/// </summary>
public class Config
{
    //==================== System Clock ===================\\

    #region SystemClock

    // The current logical time of the system (simulation clock).
    public DateTime Clock { get; set; }

    #endregion SystemClock

    //==================== Admin Credentials ===================\\

    #region AdminCredentials

    // The administrator's unique ID for login.
    public int AdminId { get; set; }

    // The administrator's password.
    public string AdminPassword { get; set; }

    #endregion AdminCredentials

    //==================== Company Location ===================\\

    #region CompanyLocation

    // The textual address of the company headquarters.
    public string? CompanyAddress { get; set; }

    // The latitude coordinate of the company.
    public double? Latitude { get; set; }

    // The longitude coordinate of the company.
    public double? Longitude { get; set; }

    #endregion CompanyLocation

    //==================== Operational Parameters ===================\\

    #region OperationalParams

    // The maximum allowed air distance for deliveries (in Km).
    public double? MaxAirDistance { get; set; }

    // Average speed for cars (km/h).
    public double AvgCarSpeed { get; set; }

    // Average speed for motorcycles (km/h).
    public double AvgMotorcycleSpeed { get; set; }

    // Average speed for bicycles (km/h).
    public double AvgBicycleSpeed { get; set; }

    // Average speed for walking couriers (km/h).
    public double AvgWalkSpeed { get; set; }

    #endregion OperationalParams

    //==================== Time Policies ===================\\

    #region TimePolicies

    // The maximum target time allowed for a delivery to be completed.
    public TimeSpan MaxDelTimeRnge { get; set; }

    // The time threshold before max time where a delivery is considered "at risk".
    public TimeSpan RiskTimeRnge { get; set; }

    // The duration of inactivity after which a courier is marked as disabled/idle.
    public TimeSpan UnactiveTimeRnge { get; set; }

    #endregion TimePolicies

    //==================== Overrides ===================\\

    #region Overrides

    /// <summary>
    /// Returns a string representation of the object properties using reflection helper.
    /// </summary>
    public override string ToString() => this.ToStringProperty();

    #endregion Overrides
}