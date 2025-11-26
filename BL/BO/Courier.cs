namespace BO;

/// <summary>
/// Represents a courier responsible for delivering orders.
/// </summary>
/// <remarks>
/// The <see cref="Courier"/> class contains information about a courier, including their contact
/// details, status, and delivery performance metrics. It is used to manage and track couriers within the delivery
/// system.
/// </remarks>
public class Courier
{
    public int CourierId { get; init; }
    public string CourierFullName { get; set; }
    public string CourierCellPhone { get; set; }

    public string CourierEmail { get; set; }
    public string CourierPassword { get; set; }

    public bool CourierIsActive { get; set; }

    public double? MaxCourierDistance { get; set; }
    public VehicleType VehicleType { get; set; }

    public DateTime? StartWorkDate { get; init; }

    public int TotalOnTimeDeliveries { get; init; }
    public int TotalLateDeliveries { get; init; }

    public BO.OrderInProgress? OrderInProgress { get; set; }
}
