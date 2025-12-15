namespace BO;
using Helpers;
using System;

//==================== Delivery History Entity (List View) ===================\\

/// <summary>
/// Represents a specific delivery attempt within an order's history.
/// Used to display the timeline of deliveries for a single order.
/// </summary>
public class DeliveryPerOrderInList
{
    //==================== Identity & Courier ===================\\

    #region IdentityAndCourier

    // The unique identifier of this specific delivery attempt.
    public int DeliveryId { get; init; }

    // The ID of the courier assigned to this delivery (nullable).
    public int? CourierId { get; init; }

    // The full name of the courier (or "System" if automated/admin).
    public string CourierFullName { get; init; }

    #endregion IdentityAndCourier

    //==================== Details & Status ===================\\

    #region DetailsAndStatus

    // The type of shipment method used.
    public ShipmentType ShipmentType { get; init; }

    // The date/time when the delivery started.
    public DateTime StartDeliveryDate { get; init; }

    // The final outcome of the delivery (Completed, Failed, etc.), nullable if still active.
    public DeliveryFinishType? DeliveryFinishType { get; init; }

    // The date/time when the delivery finished (nullable).
    public DateTime? FinishDeliveryTime { get; init; }

    #endregion DetailsAndStatus

    //==================== Overrides ===================\\

    #region Overrides

    /// <summary>
    /// Returns a string representation of the object properties using reflection helper.
    /// </summary>
    public override string ToString() => this.ToStringProperty();

    #endregion Overrides

}