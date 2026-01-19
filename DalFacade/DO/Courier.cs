namespace DO;

public record Courier
(
    int CourierId,
    string CourierFullName,
    string CourierCellPhone,
    string CourierEmail,
    string CourierPassword,
    bool CourierEnabled,
    double? MaxCourierDistance,
    DateTime? SeniorityOfCourier,
    CourierVehicleType CourierVehicleType
)
{
    public Courier() : this(
        0,
        "",
        "",
        "",
        "",
        false,
        null,
        null,
        CourierVehicleType.Car   // default
    )
    { }
}


