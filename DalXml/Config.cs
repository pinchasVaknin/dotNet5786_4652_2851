namespace Dal;

/// <summary>
/// Provides configuration settings and utility methods for managing application-specific data.
/// </summary>
/// <remarks>The <c>Config</c> class contains static properties and methods to access and modify configuration
/// values stored in an XML file. It includes settings for order and delivery identifiers, administrative credentials,
/// company location, and various operational parameters such as speed and time ranges.</remarks>
internal static class Config
{
    internal const string s_data_config_xml = "data-config.xml";
    internal const string s_orders_xml = "orders.xml";
    internal const string s_deliverys_xml = "deliverys.xml";
    internal const string s_couriers_xml = "couriers.xml";


    /// <summary>
    /// Gets the next order identifier from the config and increments the stored value.
    /// </summary>
    internal static int NextOrderId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextOrderId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextOrderId", value);
    }
    /// <summary>
    /// Gets the next delivery identifier from the config and increments the stored value.
    /// </summary>
    internal static int NextDeliveryId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextDeliveryId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextDeliveryId", value);
    }

    /// <summary>
    /// Gets or sets the application clock value stored in the config XML.
    /// </summary>
    internal static DateTime Clock
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

    /// <summary>
    /// Gets or sets the administrator user identifier stored in the config.
    /// </summary>
    internal static int adminId
    {
        get => XMLTools.GetConfigIntVal(s_data_config_xml, "adminId");
        set => XMLTools.SetConfigIntVal(s_data_config_xml, "adminId", value);
    }
    /// <summary>
    /// Gets or sets the administrator password stored in the config.
    /// </summary>
    internal static string adminPassword
    {
        get => XMLTools.GetConfigStringVal(s_data_config_xml, "adminPassword");
        set => XMLTools.SetConfigStringVal(s_data_config_xml, "adminPassword", value);
    }

    /// <summary>
    /// Gets or sets the company address (nullable) stored in the config.
    /// </summary>
    internal static string? companyAdress
    {
        get => XMLTools.GetConfigStringNullableVal(s_data_config_xml, "companyAdress");
        set => XMLTools.SetConfigStringNullableVal(s_data_config_xml, "companyAdress", value);
    }
    /// <summary>
    /// Gets or sets the company latitude (nullable) stored in the config.
    /// </summary>
    internal static double? latitude
    {
        get => XMLTools.GetConfigDoubleNullableVal(s_data_config_xml, "latitude");
        set => XMLTools.SetConfigDoubleNullableVal(s_data_config_xml, "latitude", value);
    }
    /// <summary>
    /// Gets or sets the company longitude (nullable) stored in the config.
    /// </summary>
    internal static double? longitude
    {
        get => XMLTools.GetConfigDoubleNullableVal(s_data_config_xml, "longitude");
        set => XMLTools.SetConfigDoubleNullableVal(s_data_config_xml, "longitude", value);
    }

    /// <summary>
    /// Gets or sets the maximum allowed air distance (nullable) for deliveries.
    /// </summary>
    internal static double? maxAirDistance
    {
        get => XMLTools.GetConfigDoubleNullableVal(s_data_config_xml, "maxAirDistance");
        set => XMLTools.SetConfigDoubleNullableVal(s_data_config_xml, "maxAirDistance", value);
    }
    /// <summary>
    /// Gets or sets the average car speed used for time/delivery calculations.
    /// </summary>
    internal static double avgCarSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "avgCarSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "avgCarSpeed", value);
    }
    /// <summary>
    /// Gets or sets the average motorcycle speed used for time/delivery calculations.
    /// </summary>
    internal static double avgMotorcycleSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "avgMotorcycleSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "avgMotorcycleSpeed", value);
    }
    /// <summary>
    /// Gets or sets the average bicycle speed used for time/delivery calculations.
    /// </summary>
    internal static double avgBicyleSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "avgBicyleSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "avgBicyleSpeed", value);
    }
    /// <summary>
    /// Gets or sets the average walking speed used for time/delivery calculations.
    /// </summary>
    internal static double avgWalkSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "avgWalkSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "avgWalkSpeed", value);
    }

    /// <summary>
    /// Gets or sets the maximum delivery time range used for scheduling.
    /// </summary>
    internal static TimeSpan maxDelTimeRnge
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "maxDelTimeRnge");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "maxDelTimeRnge", value);
    }
    /// <summary>
    /// Gets or sets the risk time range used to identify high-risk deliveries.
    /// </summary>
    internal static TimeSpan riskTimeRnge
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "riskTimeRnge");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "riskTimeRnge", value);
    }
    /// <summary>
    /// Gets or sets the time range after which a courier is considered unactive.
    /// </summary>
    internal static TimeSpan UnactiveTimeRnge
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "UnactiveTimeRnge");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "UnactiveTimeRnge", value);
    }


    /// <summary>
    /// Resets all configuration values to their default states and updates the config XML.
    /// </summary>
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