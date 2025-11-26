namespace BO;

internal class OrderInProgress
{
    public int DeliveryId { get; init; }
    public int OrderId { get; init; }
    public TypeOfOrder TypeOfOrder { get; init; }
    public string? OrderDetail { get; init; }
    public string CustomerAddress { get; init; }
    public double AirDistance { get; init; }
    public double? ActualDistance { get; init; }
    public string CostumerFullName { get; init; }
    public string CostumerPhone { get; init; }
    public DateTime OrderOpenTime { get; init; }
    public DateTime DeliveryStartTime { get; init; }
    public DateTime ExpectedDeliveryTime { get; init; }
    public DateTime MaxDeliveryTime { get; init; }
    public OrderStatus OrderStatus { get; init; }
    public ScheduleStatus ScheduleStatus { get; init; }
    public TimeSpan TimeLeftToFinish { get; init; }

}
