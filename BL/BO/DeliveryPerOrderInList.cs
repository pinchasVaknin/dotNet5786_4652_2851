namespace BO;

/// <summary>
/// Represents the delivery details for a specific order in a list.
/// </summary>
/// <remarks>
/// This class provides information about the delivery status, including the courier details and delivery
/// timings.
/// </remarks>
public class DeliveryPerOrderInList
{
    public int DeliveryId { get; init; }
    public int? CourierId { get; init; }
    public string CourierFullName { get; init; }
    public ShipmentType ShipmentType { get; init; }
    public DateTime StartDeliveryDate { get; init; }
    public DeliveryFinishType? DeliveryFinishType { get; init; }
    public DateTime? FinishDeliveryTime { get; init; }

}
