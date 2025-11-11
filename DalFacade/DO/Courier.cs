namespace DO;
public record Courier
(
    int courierId,
    string courierFullName,
    string courierCellPhone,
    string courierEmail,
    string courierPassword,
    string courieraddress, // Address from which the courier starts work
    bool courierEnabled,
    double? maxCourierDistance,
    DateTime? seniorityOfCourier,
    courierVehicleType courierVehicleType
);


