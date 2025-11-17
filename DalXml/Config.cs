namespace Dal;

internal static class Config
{
    internal const string s_data_config_xml = "data-config.xml";
    internal const string s_orders_xml = "orders.xml";
    internal const string s_deliverys_xml = "deliverys.xml";
    internal const string s_couriers_xml = "couriers.xml";


    internal static int NextOrderId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextOrderId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextOrderId", value);
    }
    internal static int NextDeliveryId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextDeliveryId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextDeliveryId", value);
    }

    internal static DateTime Clock
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

    internal static int adminId
    {
        get => XMLTools.GetConfigIntVal(s_data_config_xml, "adminId");
        set => XMLTools.SetConfigIntVal(s_data_config_xml, "adminId", value);
    }
    internal static string adminPassword
    {
        get => XMLTools.GetConfigStringVal(s_data_config_xml, "adminPassword");
        set => XMLTools.SetConfigStringVal(s_data_config_xml, "adminPassword", value);
    }

    internal static string? companyAdress
    {
        get => XMLTools.GetConfigStringNullableVal(s_data_config_xml, "companyAdress");
        set => XMLTools.SetConfigStringNullableVal(s_data_config_xml, "companyAdress", value);
    }
    internal static double? latitude
    {
        get => XMLTools.GetConfigDoubleNullableVal(s_data_config_xml, "latitude");
        set => XMLTools.SetConfigDoubleNullableVal(s_data_config_xml, "latitude", value);
    }
    internal static double? longitude
    {
        get => XMLTools.GetConfigDoubleNullableVal(s_data_config_xml, "longitude");
        set => XMLTools.SetConfigDoubleNullableVal(s_data_config_xml, "longitude", value);
    }

    internal static double? maxAirDistance
    {
        get => XMLTools.GetConfigDoubleNullableVal(s_data_config_xml, "maxAirDistance");
        set => XMLTools.SetConfigDoubleNullableVal(s_data_config_xml, "maxAirDistance", value);
    }
    internal static double avgCarSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "avgCarSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "avgCarSpeed", value);
    }
    internal static double avgMotorcycleSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "avgMotorcycleSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "avgMotorcycleSpeed", value);
    }
    internal static double avgBicyleSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "avgBicyleSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "avgBicyleSpeed", value);
    }
    internal static double avgWalkSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "avgWalkSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "avgWalkSpeed", value);
    }

    internal static TimeSpan maxDelTimeRnge
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "maxDelTimeRnge");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "maxDelTimeRnge", value);
    }
    internal static TimeSpan riskTimeRnge
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "riskTimeRnge");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "riskTimeRnge", value);
    }
    internal static TimeSpan UnactiveTimeRnge
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "UnactiveTimeRnge");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "UnactiveTimeRnge", value);
    }


    internal static void Reset()
    {
        NextOrderId = 0;
        NextDeliveryId = 0;

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