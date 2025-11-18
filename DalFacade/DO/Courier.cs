namespace DO;
public record Courier
(
    int courierId,
    string courierFullName,
    string courierCellPhone,
    string courierEmail,
    string courierPassword,
    string courierAddress, // Address from which the courier starts work
    bool courierEnabled,
    double? maxCourierDistance,
    DateTime? seniorityOfCourier,
    courierVehicleType courierVehicleType
)
{
    public Courier() : this(
        0,
        "",
        "",
        "",
        "",
        "",
        false,
        null,
        null,
        courierVehicleType.Car   // default
    )
    { }
}


