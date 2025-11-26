namespace BO;

internal class Courier
{
    public int CourierId { get; init; }
    public string CourierFullName { get; set; }
    public string CourierCellPhone { get; set; }

    public string CourierEmail { get; set; }
    public string CourierPassword { get; set; }

    public bool CourierIsActive { get; set; }

    public double? MaxCourierDistance { get; set; }
    public CourierVehicleType CourierVehicleType { get; set; }

    public DateTime? StartWorkDate { get; init; }

    public int TotalOnTimeDeliveries { get; init; }
    public int TotalLateDeliveries { get; init; }

    public BO.OrderInProgress? OrderInProgress { get; set; }
}
