namespace DO;
public record Delivery
(
    int deliveryId,
    int orderId,
    int courierId,
    double? deliveryMaxDistance,
    DateTime deliveryDate,
    DateTime deliveryFinishDate,
    DeliveryWay deliveryWay,
    DeliveryFinishType deliveryFinishType
);
