namespace Dal;

/// <summary>
/// default and restart
/// </summary>
internal static class Config
{
    internal const int StartOrderId = 0;
    private static int s_nextOrderId = StartOrderId;
    internal static int NextOrderId { get => s_nextOrderId++; }


    internal const int StartDeliveryId = 0;
    private static int s_nextDeliveryId = StartDeliveryId;
    internal static int NextDeliveryId { get => s_nextDeliveryId++; }


    internal static DateTime Clock { get; set; } = DateTime.Now;


    internal static int AdminId { get; set; } = 0;
    internal static string AdminPassword { get; set; } = "ChangeMe!1234";


    internal static string? CompanyAddress { get; set; } = null;
    internal static double? Latitude { get; set; } = null;
    internal static double? Longitude { get; set; } = null;


    internal static double? MaxAirDistance { get; set; } = null;
    internal static double AvgCarSpeed { get; set; } = 0;
    internal static double AvgMotorcycleSpeed { get; set; } = 0;
    internal static double AvgBicycleSpeed { get; set; } = 0;
    internal static double AvgWalkSpeed { get; set; } = 0;

    internal static TimeSpan MaxDelTimeRnge { get; set; } = TimeSpan.Zero;
    internal static TimeSpan RiskTimeRnge { get; set; } = TimeSpan.Zero;
    internal static TimeSpan UnactiveTimeRnge { get; set; } = TimeSpan.Zero;

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

}
