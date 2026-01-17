namespace BO;

//==================== Enums from DAL (Converted) ===================\\

#region DAL Enums Conversion

/// <summary>
/// Represents the logical status of an order in the system.
/// </summary>
public enum OrderStatus
{
    Open,
    InProgress,
    Supplied,
    Refused,
    Cancelled
}

/// <summary>
/// Represents the type/category of the ordered product.
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
    Accessory,
}

/// <summary>
/// Represents the courier's vehicle type.
/// </summary>
public enum VehicleType
{
    Car,
    Motorcycle,
    Bicycle,
    Foot,
    All
}

/// <summary>
/// Represents the shipment service level.
/// </summary>
public enum ShipmentType
{
    Express,
    SameDay,
    Standard,
    Economy
}

/// <summary>
/// Represents the final outcome of a delivery.
/// </summary>
public enum DeliveryFinishType
{
    Completed,
    Cancelled,
    Failed,
    Returned
}

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


#endregion DAL Enums Conversion

//==================== BL Specific Enums ===================\\

#region BL Enums General

//==================== ICourier Enums ===================\\

#region ICourier Enums

/// <summary>
/// Represents the logical role of a user in the system (for login & permissions).
/// </summary>
public enum UserRole
{
    Admin,
    Courier
}

/// <summary>
/// Sorting options for the courier list view (BO.CourierInList).
/// </summary>
public enum CourierInListSortBy
{
    CourierId,
    CourierFullName,
    CourierIsActive,
    VehicleType,
    StartWorkDate,
    DeliveriesInTime,
    DeliveriesOverTime,
    OrderIdInHandle
}

/// <summary>
/// Specifies the criteria for filtering a list of couriers.
/// </summary>
/// <remarks>This enumeration provides options to filter couriers based on their active status, vehicle type, 
/// current order handling status, or to include all couriers without applying any filters.</remarks>
public enum CourierInListFilterBy
{

    // Filter by active status of the courier.
    CourierIsActive,

    // Filter by vehicle type used by the courier.
    VehicleType,

    // Filter by whether the courier is currently handling an order.
    OrderIdInHandle,

    // No filtering; include
    All
}

#endregion ICourier Enums

//==================== IConfig Enums ===================\\

#region IConfig Enums

/// <summary>
/// Units of time used to advance the logical system clock.
/// </summary>
public enum TimeUnit
{
    Minute,
    Hour,
    Day,
    Month,
    Year
}

/// <summary>
/// Represents the configuration fields that can be updated.
/// </summary>
public enum ConfigFields
{
    Clock,
    AdminId,
    AdminPassword,
    CompanyAddress,
    Latitude,
    Longitude,
    MaxAirDistance,
    AvgCarSpeed,
    AvgMotorcycleSpeed,
    AvgBicycleSpeed,
    AvgWalkSpeed,
    MaxDelTimeRnge,
    RiskTimeRnge,
    UnactiveTimeRnge
}

#endregion IConfig Enums

//==================== IOrder Enums ===================\\

#region IOrder Enums

/// <summary>
/// Represents the timing status of an order or delivery relative to its required time window.
/// </summary>
public enum ScheduleStatus
{
    OnTime,
    InRisk,
    Late
}

/// <summary>
/// Fields that can be used to filter the order list (BO.OrderInList).
/// </summary>
public enum OrderInListFilterBy
{

    /// <summary>
    /// Filter by logical order type (BO.TypeOfOrder).
    /// </summary>
    TypeOfOrder,

    /// <summary>
    /// Filter by logical order status (BO.OrderStatus).
    /// </summary>
    OrderStatus,

    /// <summary>
    /// Filter by schedule status (BO.ScheduleStatus - OnTime / InRisk / Late).
    /// </summary>
    ScheduleStatus,

    /// <summary>
    /// No filtering; include all orders.
    /// </summary>
    All
}

/// <summary>
/// Fields that can be used to sort the order list (BO.OrderInList).
/// </summary>
public enum OrderInListSortBy
{
    /// <summary>Sort by order ID.</summary>
    OrderId,

    /// <summary>Sort by logical order type (BO.TypeOfOrder).</summary>
    TypeOfOrder,

    /// <summary>Sort by air distance between the order destination and the company.</summary>
    AirDistance,

    /// <summary>Sort by logical order status (BO.OrderStatus).</summary>
    OrderStatus,

    /// <summary>Sort by schedule status (BO.ScheduleStatus).</summary>
    ScheduleStatus,

    /// <summary>Sort by time left until the required finish time.</summary>
    TimeLeftToFinish,

    /// <summary>Sort by total handling time of the order.</summary>
    TotalHandleTime,

    /// <summary>Sort by total number of deliveries associated with this order.</summary>
    TotalDeliveries
}

/// <summary>
/// Fields that can be used to sort the closed deliveries list (BO.ClosedDeliveryInList).
/// </summary>
public enum ClosedDeliverySortBy
{
    /// <summary>Sort by order id.</summary>
    OrderId,

    /// <summary>Sort by order type.</summary>
    TypeOfOrder,

    /// <summary>Sort by total handling time.</summary>
    TotalHandleTime,

    /// <summary>Sort by actual distance.</summary>
    ActualDistance,

    /// <summary>Sort by delivery finish type.</summary>
    DeliveryFinishType
}

/// <summary>
/// Fields that can be used to sort the open orders list for a courier (BO.OpenOrderInList).
/// </summary>
public enum OpenOrderSortBy
{
    OrderId,
    TypeOfOrder,
    OrderWeight,
    IsFragile,
    OrderSize,
    AirDistance,
    ScheduleStatus,
    TimeLeftToFinish,
    MaxDeliveryTime
}

/// <summary>
/// Logical combined order status used for summary counts:
/// combination of OrderStatus and ScheduleStatus where relevant.
/// </summary>
public enum LogicalOrderStatus
{
    // Open orders with timing status
    Open_OnTime,
    Open_InRisk,
    Open_Late,

    // In-progress orders with timing status
    InProgress_OnTime,
    InProgress_InRisk,
    InProgress_Late,

    // Final states (no timing distinction)
    Supplied,
    Refused,
    Canceled
}

/// <summary>
/// Fields that can be used to filter the open orders list for a courier (BO.OpenOrderInList).
/// </summary>
public enum OpenOrderFilterBy
{
    /// <summary>Filter by order type (BO.TypeOfOrder).</summary>
    TypeOfOrder
}

#endregion IOrder Enums

#endregion BL Enums General