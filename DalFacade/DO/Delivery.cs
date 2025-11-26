namespace DO;
public record Delivery
(
    int DeliveryId, //need to be run number
    int OrderId,
    int CourierId,
    double? DeliveryMaxDistance,
    DateTime DeliveryDate,
    DateTime DeliveryFinishDate,
    ShipmentType ShipmentType,
    DeliveryFinishType DeliveryFinishType
)
{
    public Delivery() : this(
       0,                  // deliveryId
       0,                  // orderId
       0,                  // courierId
       null,               // deliveryMaxDistance
       DateTime.MinValue,  // deliveryDate
       DateTime.MinValue,  // deliveryFinishDate
       ShipmentType.Standard,      // default
       DeliveryFinishType.Completed // no better option since there's no "None"
    )
    { }
}
