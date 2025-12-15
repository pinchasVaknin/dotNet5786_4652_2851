namespace DO;

//========================= Enums =========================\\

/// <summary>
/// Represents the type of vehicle used by a courier.
/// </summary>
public enum CourierVehicleType
{
    Car,
    Motorcycle,
    Bicycle,
    Foot
}

/// <summary>
/// Represents the urgency or service level of a shipment.
/// </summary>
public enum ShipmentType
{
    Express,
    SameDay,
    Standard,
    Economy
}

/// <summary>
/// Represents the final outcome or status of a delivery process.
/// </summary>
public enum DeliveryFinishType
{
    Completed,
    Cancelled,
    Failed,
    Returned,
    /// <summary>
    /// Indicates the delivery has not been finalized yet.
    /// </summary>
    None
}

/// <summary>
/// Represents the main category of the ordered product.
/// </summary>
public enum TypeOfOrder
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

//========================= Catalog Details Enums =========================\\

/// <summary>
/// Contains specific model details for each product category.
/// </summary>
public class Catalog
{

    /// <summary>
    /// Specific models for Smartphone orders.
    /// </summary>
    public enum SmartphoneDetails
    {
        iPhone_14,
        Galaxy_S23,
        Pixel_8,
        Xiaomi_13,
    }

    /// <summary>
    /// Specific models for Laptop orders.
    /// </summary>
    public enum LaptopDetails
    {
        Dell_XPS_13,
        MacBook_Air_M2,
        HP_Spectre_x360,
        Lenovo_ThinkPad_X1
    }

    /// <summary>
    /// Specific models for Tablet orders.
    /// </summary>
    public enum TabletDetails
    {
        iPad_Air,
        Galaxy_Tab_S9,
        Xiaomi_Pad_6
    }

    /// <summary>
    /// Specific models for TV orders.
    /// </summary>
    public enum TVDetails
    {
        LG_OLED_C3_55,
        Samsung_QLED_Q80_65,
        Sony_Bravia_50
    }

    /// <summary>
    /// Specific models for Camera orders.
    /// </summary>
    public enum CameraDetails
    {
        Canon_EOS_R10,
        Sony_a6400,
        Nikon_Z50
    }

    /// <summary>
    /// Specific models for Audio equipment orders.
    /// </summary>
    public enum AudioDetails
    {
        Sony_WH_1000XM5,
        AirPods_Pro_2,
        Bose_QC45,
        JBL_Flip_6
    }

    /// <summary>
    /// Specific models for Smart Home devices.
    /// </summary>
    public enum SmartHomeDetails
    {
        Google_Nest_Hub,
        Amazon_Echo,
        Philips_Hue_Starter
    }

    /// <summary>
    /// Specific models for Gaming Consoles.
    /// </summary>
    public enum GamingConsoleDetails
    {
        PlayStation_5,
        Xbox_Series_X,
        Nintendo_Switch_OLED
    }

    /// <summary>
    /// Specific models for Accessories.
    /// </summary>
    public enum AccessoryDetails
    {
        USB_C_Cable_100W,
        GaN_Charger_65W,
        NVMe_SSD_1TB,
        HDMI_4K_2_1_Cable
    }
}