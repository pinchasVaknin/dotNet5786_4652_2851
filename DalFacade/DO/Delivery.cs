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
);
