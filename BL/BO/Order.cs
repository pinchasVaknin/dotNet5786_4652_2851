namespace BO;

/// <summary>
/// Represents an order with details such as type, address, customer information, and delivery schedule.
/// </summary>
/// <remarks>
/// This class encapsulates the essential information required to process and track an order, including
/// its geographical location, customer details, and delivery constraints. It supports various order types and statuses,
/// and provides properties to manage delivery expectations and constraints.
/// </remarks>
public class Order
{
    public int OrderId { get; init; } //need to be run number 
    public string? OrderDetail { get; set; }
    public string OrderAddress { get; set; }
    public double OrderLatitude { get; set; }
    public double OrderLongitude { get; set; }
    public double AirDistance { get; set; }
    public string CustomerFullName { get; set; }
    public string CustomerPhone { get; set; }
    public double OrderWeight { get; set; }
    public bool IsFragile { get; set; }
    public double OrderSize { get; set; }
    public DateTime OrderOpenTime { get; init; }
    public DateTime? ExpectedDeliveryTime { get; init; }
    public DateTime MaxDeliveryTime { get; init; }
    public TypeOfOrder TypeOfOrder { get; set; }
    public OrderStatus OrderStatus { get; init; }
    public ScheduleStatus ScheduleStatus { get; init; }
    public TimeSpan TimeRemaining { get; init; }

    public List<DeliveryPerOrderInList>? DeliveryPerOrderInList { get; init; }

}