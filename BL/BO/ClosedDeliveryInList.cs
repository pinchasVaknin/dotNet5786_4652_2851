namespace BO;
using Helpers;
using System;

//==================== Closed Delivery Entity (List View) ===================\\

/// <summary>
/// Represents a summary of a closed delivery for list display purposes.
/// Contains details about the delivery outcome, timing, and associated order.
/// </summary>
public class ClosedDeliveryInList
{
    //==================== Data Properties ===================\\

    #region DataProperties

    // The unique identifier of the delivery.
    public int DeliveryId { get; init; }

    // The unique identifier of the associated order.
    public int OrderId { get; init; }

    // The category/type of the order (e.g., Smartphone, Laptop).
    public TypeOfOrder TypeOfOrder { get; init; }

    // The destination address for the order.
    public string OrderAddress { get; init; }

    // The shipment method used (e.g., Standard, Express).
    public ShipmentType ShipmentType { get; init; }

    // The actual distance traveled or calculated for this delivery (in Km). Nullable if calculation failed.
    public double? ActualDistance { get; init; }

    // The total time taken from delivery start to finish.
    public TimeSpan TotalHandleTime { get; init; }

    // The final status of the delivery (Completed, Failed, etc.).
    public DeliveryFinishType? DeliveryFinishType { get; init; }

    #endregion DataProperties

    //==================== Overrides ===================\\

    #region Overrides

    /// <summary>
    /// Returns a string representation of the object properties using reflection helper.
    /// </summary>
    public override string ToString() => this.ToStringProperty();

    #endregion Overrides

}