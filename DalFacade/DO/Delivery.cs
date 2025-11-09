namespace DO;
public record Delivery
(
    int deliveryId,
    int orderId,
    int courierId,
    double? deliveryMaxDistance,
    DateTime deliveryDate,
    DateTime deliveryFinishDate,
    ShipmentType shipmentType,
    DeliveryFinishType deliveryFinishType
);
