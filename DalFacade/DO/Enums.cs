namespace DO;

public enum courierVehicleType
{
    Car,
    Motorcycle, 
    Bicycle, 
    Foot
}
public enum ShipmentType
{
    Express, 
    SameDay,
    Standard,
    Economy
}
public enum DeliveryFinishType
{
    Completed,
    Cancelled,
    Failed,
    Returned
}
public enum typeOfOrder
{
    Smartphone,
    Laptop,
    Tablet,
    TV,
    Camera,
    Audio,
    SmartHome,
    GamingConsole,
    Accessory
}

/// <summary>
/// Catalog of the Details in typeOfOrder (class of enum)
/// </summary>
public class Catalog
{
    public enum SmartphoneDetails
    {
        iPhone_14,
        Galaxy_S23,
        Pixel_8,
        Xiaomi_13,
    }
    public enum LaptopDetails
    {
        Dell_XPS_13,
        MacBook_Air_M2,
        HP_Spectre_x360,
        Lenovo_ThinkPad_X1
    }
    public enum TabletDetails
    {
        iPad_Air,
        Galaxy_Tab_S9,
        Xiaomi_Pad_6
    }
    public enum TVDetails
    {
        LG_OLED_C3_55,
        Samsung_QLED_Q80_65,
        Sony_Bravia_50
    }
    public enum CameraDetails
    {
        Canon_EOS_R10,
        Sony_a6400,
        Nikon_Z50
    }
    public enum AudioDetails
    {
        Sony_WH_1000XM5,
        AirPods_Pro_2,
        Bose_QC45,
        JBL_Flip_6
    }
    public enum SmartHomeDetails
    {
        Google_Nest_Hub,
        Amazon_Echo,
        Philips_Hue_Starter
    }
    public enum GamingConsoleDetails
    {
        PlayStation_5,
        Xbox_Series_X,
        Nintendo_Switch_OLED
    }
    public enum AccessoryDetails
    {
        USB_C_Cable_100W,
        GaN_Charger_65W,
        NVMe_SSD_1TB,
        HDMI_4K_2_1_Cable
    }
}

