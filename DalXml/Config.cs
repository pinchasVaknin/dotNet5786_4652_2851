namespace Dal;

//==================== XML Config (Static) ===================\\

/// <summary>
/// Provides configuration settings and utility methods for managing application-specific data.
/// </summary>
/// <remarks>The <c>Config</c> class contains static properties and methods to access and modify configuration
/// values stored in an XML file. It includes settings for order and delivery identifiers, administrative credentials,
/// company location, and various operational parameters such as speed and time ranges.</remarks>
internal static class Config
{
    //==================== File Names ===================\\

    #region FileNames

    internal const string s_data_config_xml = "data-config.xml";
    internal const string s_orders_xml = "orders.xml";
    internal const string s_deliverys_xml = "deliverys.xml";
    internal const string s_couriers_xml = "couriers.xml";

    #endregion FileNames

    //==================== ID Generators ===================\\

    #region IdGenerators

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

    #endregion IdGenerators

    //==================== System Clock ===================\\

    #region SystemClock

    /// <summary>
    /// Gets or sets the application clock value stored in the config XML.
    /// </summary>
    internal static DateTime Clock
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

    #endregion SystemClock

    //==================== Admin Credentials ===================\\

    #region AdminCredentials

    /// <summary>
    /// Gets or sets the administrator user identifier stored in the config.
    /// </summary>
    internal static int AdminId
    {
        get => XMLTools.GetConfigIntVal(s_data_config_xml, "AdminId");
        set => XMLTools.SetConfigIntVal(s_data_config_xml, "AdminId", value);
    }

    /// <summary>
    /// Gets or sets the administrator password stored in the config.
    /// </summary>
    internal static string AdminPassword
    {
        get => XMLTools.GetConfigStringVal(s_data_config_xml, "AdminPassword");
        set => XMLTools.SetConfigStringVal(s_data_config_xml, "AdminPassword", value);
    }

    #endregion AdminCredentials

    //==================== Company Location ===================\\

    #region CompanyLocation

    /// <summary>
    /// Gets or sets the company address (nullable) stored in the config.
    /// </summary>
    internal static string? CompanyAddress
    {
        get => XMLTools.GetConfigStringNullableVal(s_data_config_xml, "CompanyAddress");
        set => XMLTools.SetConfigStringNullableVal(s_data_config_xml, "CompanyAddress", value);
    }

    /// <summary>
    /// Gets or sets the company latitude (nullable) stored in the config.
    /// </summary>
    internal static double? Latitude
    {
        get => XMLTools.GetConfigDoubleNullableVal(s_data_config_xml, "Latitude");
        set => XMLTools.SetConfigDoubleNullableVal(s_data_config_xml, "Latitude", value);
    }

    /// <summary>
    /// Gets or sets the company longitude (nullable) stored in the config.
    /// </summary>
    internal static double? Longitude
    {
        get => XMLTools.GetConfigDoubleNullableVal(s_data_config_xml, "Longitude");
        set => XMLTools.SetConfigDoubleNullableVal(s_data_config_xml, "Longitude", value);
    }

    #endregion CompanyLocation

    //==================== Operational Parameters ===================\\

    #region OperationalParameters

    /// <summary>
    /// Gets or sets the maximum allowed air distance (nullable) for deliveries.
    /// </summary>
    internal static double? MaxAirDistance
    {
        get => XMLTools.GetConfigDoubleNullableVal(s_data_config_xml, "MaxAirDistance");
        set => XMLTools.SetConfigDoubleNullableVal(s_data_config_xml, "MaxAirDistance", value);
    }

    /// <summary>
    /// Gets or sets the average car speed used for time/delivery calculations.
    /// </summary>
    internal static double AvgCarSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AvgCarSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AvgCarSpeed", value);
    }

    /// <summary>
    /// Gets or sets the average motorcycle speed used for time/delivery calculations.
    /// </summary>
    internal static double AvgMotorcycleSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AvgMotorcycleSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AvgMotorcycleSpeed", value);
    }

    /// <summary>
    /// Gets or sets the average bicycle speed used for time/delivery calculations.
    /// </summary>
    internal static double AvgBicycleSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AvgBicycleSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AvgBicycleSpeed", value);
    }

    /// <summary>
    /// Gets or sets the average walking speed used for time/delivery calculations.
    /// </summary>
    internal static double AvgWalkSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AvgWalkSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AvgWalkSpeed", value);
    }

    #endregion OperationalParameters

    //==================== Time Policies ===================\\

    #region TimePolicies

    /// <summary>
    /// Gets or sets the maximum delivery time range used for scheduling.
    /// </summary>
    internal static TimeSpan MaxDelTimeRnge
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "MaxDelTimeRnge");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "MaxDelTimeRnge", value);
    }

    /// <summary>
    /// Gets or sets the risk time range used to identify high-risk deliveries.
    /// </summary>
    internal static TimeSpan RiskTimeRnge
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "RiskTimeRnge");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "RiskTimeRnge", value);
    }

    /// <summary>
    /// Gets or sets the time range after which a courier is considered unactive.
    /// </summary>
    internal static TimeSpan UnactiveTimeRnge
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "UnactiveTimeRnge");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "UnactiveTimeRnge", value);
    }

    #endregion TimePolicies

    //==================== Management ===================\\

    #region Management

    /// <summary>
    /// Resets all configuration values to their default states and updates the config XML.
    /// </summary>
    internal static void Reset()
    {
        NextOrderId = 0; // run number
        NextDeliveryId = 0; // run number

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