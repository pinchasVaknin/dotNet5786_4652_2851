namespace DO;
public record Courier
(
    int courierId,
    string courierFullName,
    string courierCellPhone,
    string courierEmail,
    string courierPassword,
    bool courierEnabled,
    double? maxCourierDistance,
    DateTime? seniorityOfCourier,
    courierDeliveryWay courierDeliveryWay//car/motorcycle/bicycle/walk
);

