namespace BO;

internal class OrderInList
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

}
