namespace DO;
public record Delivery
(
    int deliveryId, //need to be run number
    int orderId,
    int courierId,
    double? deliveryMaxDistance,
    DateTime deliveryDate,
    DateTime deliveryFinishDate,
    ShipmentType shipmentType,
    DeliveryFinishType deliveryFinishType
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
