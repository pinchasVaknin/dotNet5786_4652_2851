using Helpers;

namespace BO;

/// <summary>
/// Represents an order within a list, including details such as delivery ID, order type, and status.
/// </summary>
/// <remarks>
/// This class provides properties to access various attributes of an order, such as its delivery ID, 
/// type, status, and timing information. It is designed to be immutable after initialization.
/// </remarks>
public class OrderInList
{
    public int? DeliveryId { get; init; }
    public int OrderId { get; init; }
    public TypeOfOrder TypeOfOrder { get; init; }
    public double AirDistance { get; init; }
    public OrderStatus OrderStatus { get; init; }
    public ScheduleStatus ScheduleStatus { get; init; }
    public TimeSpan TimeLeftToFinish { get; init; }
    public TimeSpan TotalHandleTime { get; init; }
    public int TotalDeliveries { get; init; }

    public override string ToString() => this.ToStringProperty();
}
