namespace BO;

/// <summary>
/// Represents a closed delivery entry in a list, containing details about the delivery and its associated order.
/// </summary>
/// <remarks>
/// This class is used to encapsulate information about a delivery that has been completed, including
/// identifiers, order type, address, shipment details, and metrics such as actual distance and total handling
/// time.
/// </remarks>
public class ClosedDeliveryInList
{
    public int DeliveryId { get; init; }
    public int OrderId { get; init; }

    public TypeOfOrder TypeOfOrder { get; init; }
    public string OrderAddress { get; init; }

    public ShipmentType ShipmentType { get; init; }
    public double? ActualDistance { get; init; }
    public TimeSpan TotalHandleTime { get; init; }
    public DeliveryFinishType? DeliveryFinishType { get; init; }

}
