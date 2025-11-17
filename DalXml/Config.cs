namespace Dal;

internal static class Config
{
    internal const string s_data_config_xml = "data-config.xml";
    internal const string s_orders_xml = "orders.xml";
    internal const string s_deliverys_xml = "deliverys.xml";


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
        get => XMLTools.GetConfigStringVal(s_data_config_xml, "companyAdress");
        set => XMLTools.SetConfigStringVal(s_data_config_xml, "companyAdress", value);
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


}