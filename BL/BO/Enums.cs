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
    Canceled
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
    Accessory
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
    Returned,
    /// <summary>
    /// Delivery has not been finished yet (still in progress / open).
    /// </summary>
    None
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
public enum CourierListSortBy
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
    /// 
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