using System.Runtime.CompilerServices;

namespace Dal;

//==================== Internal Configuration Container ===================\\

/// <summary>
/// Static container for system configuration and auto-incrementing IDs.
/// Acts as the in-memory storage for configuration settings in DalList.
/// </summary>
internal static class Config
{
    //==================== ID Generators ===================\\

    #region IdGenerators

    internal const int StartOrderId = 0;
    private static int s_nextOrderId = StartOrderId;

    // Auto-incrementing ID for Orders
    internal static int NextOrderId
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get => s_nextOrderId++; 
    }


    internal const int StartDeliveryId = 0;
    private static int s_nextDeliveryId = StartDeliveryId;

    // Auto-incrementing ID for Deliveries
    internal static int NextDeliveryId
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get => s_nextDeliveryId++;
    }

    #endregion IdGenerators

    //==================== System Clock ===================\\

    #region SystemClock

    // Current simulated system time
    internal static DateTime Clock
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        set;
    } = DateTime.Now;

    #endregion SystemClock

    //==================== Admin Credentials ===================\\

    #region AdminCredentials

    // Administrator's ID
    internal static int AdminId
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        set;
    } = 0;

    // Administrator's Password
    internal static string AdminPassword
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        set;
    } = "ChangeMe!1234";

    #endregion AdminCredentials

    //==================== Company Location ===================\\

    #region CompanyLocation

    // Company address text
    internal static string? CompanyAddress
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        set;
    } = null;

    // Company Latitude
    internal static double? Latitude
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        set;
    } = null;

    // Company Longitude
    internal static double? Longitude
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        set;
    } = null;

    #endregion CompanyLocation

    //==================== Operational Parameters ===================\\

    #region OperationalParameters

    // Max allowed air distance for deliveries
    internal static double? MaxAirDistance
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        set;
    } = null;

    // Average speed for Car (km/h)
    internal static double AvgCarSpeed
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        set;
    } = 0;

    // Average speed for Motorcycle (km/h)
    internal static double AvgMotorcycleSpeed
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        set;
    } = 0;

    // Average speed for Bicycle (km/h)
    internal static double AvgBicycleSpeed
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        set;
    } = 0;

    // Average speed for Walking (km/h)
    internal static double AvgWalkSpeed
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        set;
    } = 0;

    #endregion OperationalParameters

    //==================== Time Policies ===================\\

    #region TimePolicies

    // Max delivery time range
    internal static TimeSpan MaxDelTimeRnge
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        set;
    } = TimeSpan.Zero;

    // Risk time buffer
    internal static TimeSpan RiskTimeRnge
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        set;
    } = TimeSpan.Zero;

    // Inactivity timeout range
    internal static TimeSpan UnactiveTimeRnge
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get;

        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        set;
    } = TimeSpan.Zero;

    #endregion TimePolicies

    //==================== Management ===================\\

    #region Management

    /// <summary>
    /// Resets all configuration values and ID counters to their default state.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static void Reset()
    {
        s_nextOrderId = StartOrderId;
        s_nextDeliveryId = StartDeliveryId;

        Clock = DateTime.Now;

        AdminId = 0;
        AdminPassword = string.Empty;

        CompanyAddress = null;
        Latitude = null;
        Longitude = null;
        MaxAirDistance = null;

        AvgCarSpeed = 0;
        AvgMotorcycleSpeed = 0;
        AvgBicycleSpeed = 0;
        AvgWalkSpeed = 0;

        MaxDelTimeRnge = TimeSpan.Zero;
        RiskTimeRnge = TimeSpan.Zero;
        UnactiveTimeRnge = TimeSpan.Zero;
    }

    #endregion Management

}