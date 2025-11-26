namespace BO;

/// <summary>
/// Represents an open order in a list, including details such as order type, weight, size, and delivery information.
/// </summary>
/// <remarks>
/// This class is used to encapsulate the properties of an order that is currently open and being
/// processed. It includes information necessary for scheduling and delivery, such as courier assignment, order
/// dimensions, and delivery time constraints.
/// </remarks>
internal class OpenOrderInList
{
    public int? CourierId { get; init; }
    public int OrderId { get; init; }
    public TypeOfOrder TypeOfOrder { get; init; }
    public double OrderWeight { get; init; }
    public bool IsFragile { get; init; }
    public double OrderSize { get; init; }
    public string CustomerAddress { get; init; }
    public double AirDistance { get; init; }
    public double? ActualDistance { get; init; }
    public TimeSpan? EstimatedActualTime { get; init; }
    public ScheduleStatus ScheduleStatus { get; init; }
    public TimeSpan TimeLeftToFinish { get; init; }
    public DateTime MaxDeliveryTime { get; init; }
}
