namespace BO;

public enum OrderStatus
{
    Open,
    InProgress,
    Supplied,
    Refused,
    Canceled
}
public enum ScheduleStatus
{
    OnTime,
    InRisk,
    Late
}
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
public enum VehicleType
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

