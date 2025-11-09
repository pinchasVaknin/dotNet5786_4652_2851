namespace Dal;

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
    internal static string adminPassword { get; set; } = "admin1234";// must to change


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







}
