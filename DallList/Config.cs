namespace Dal;

/// <summary>
/// default and restart
/// </summary>
internal static class Config
{
    internal const int startOrderId = 0;
    private static int s_nextOrderId = startOrderId;
    internal static int NextOrderId { get => s_nextOrderId++; }


    internal const int startDeliveryId = 0;
    private static int s_nextDeliveryId = startDeliveryId;
    internal static int NextDeliveryId { get => s_nextDeliveryId++; }


    internal static DateTime Clock { get; set; } = DateTime.Now;


    internal static int adminId { get; set; } = 0;
    internal static string adminPassword { get; set; } = "ChangeMe!1234";


    internal static string? companyAdress { get; set; } = null;
    internal static double? latitude { get; set; } = null;
    internal static double? longitude { get; set; } = null;


    internal static double? maxAirDistance { get; set; } = null;
    internal static double avgCarSpeed { get; set; } = 0;
    internal static double avgMotorcycleSpeed { get; set; } = 0;
    internal static double avgBicyleSpeed { get; set; } = 0;
    internal static double avgWalkSpeed { get; set; } = 0;

    internal static TimeSpan maxDelTimeRnge { get; set; } = TimeSpan.Zero;
    internal static TimeSpan riskTimeRnge { get; set; } = TimeSpan.Zero;
    internal static TimeSpan UnactiveTimeRnge { get; set; } = TimeSpan.Zero;

    internal static void Reset()
    {
        s_nextOrderId = startOrderId;
        s_nextDeliveryId = startDeliveryId;

        Clock = DateTime.Now;

        adminId = 0;
        adminPassword = string.Empty;

        companyAdress = null;
        latitude = null;
        longitude = null;
        maxAirDistance = null;

        avgCarSpeed = 0;
        avgMotorcycleSpeed = 0;
        avgBicyleSpeed = 0;
        avgWalkSpeed = 0;

        maxDelTimeRnge = TimeSpan.Zero;
        riskTimeRnge = TimeSpan.Zero;
        UnactiveTimeRnge = TimeSpan.Zero;
    }





}
