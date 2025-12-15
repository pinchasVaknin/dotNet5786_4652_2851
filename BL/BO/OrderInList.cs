namespace BO;
using Helpers;
using System;

//==================== Order Entity (List View) ===================\\

/// <summary>
/// Represents a summary of an order for list display purposes.
/// Optimized for viewing order status, priorities, and general metrics without loading full details.
/// </summary>
public class OrderInList
{
    //==================== Identification ===================\\

    #region Identification

    // The identifier of the active/latest delivery associated with this order (nullable).
    public int? DeliveryId { get; init; }

    // The unique identifier of the order.
    public int OrderId { get; init; }

    // The category/type of the ordered items.
    public TypeOfOrder TypeOfOrder { get; init; }

    #endregion Identification

    //==================== Status & Distance ===================\\

    #region StatusAndDistance

    // The straight-line distance from the company/courier to the destination (Km).
    public double AirDistance { get; init; }

    // The current logical status of the order (Open, InProgress, etc.).
    public OrderStatus OrderStatus { get; init; }

    // The schedule adherence status (OnTime, InRisk, Late).
    public ScheduleStatus ScheduleStatus { get; init; }

    #endregion StatusAndDistance

    //==================== Metrics & Statistics ===================\\

    #region Metrics

    // The time remaining until the delivery deadline.
    public TimeSpan TimeLeftToFinish { get; init; }

    // The total duration the order has been in processing/delivery.
    public TimeSpan TotalHandleTime { get; init; }

    // The total number of delivery attempts made for this order.
    public int TotalDeliveries { get; init; }

    #endregion Metrics

    //==================== Overrides ===================\\

    #region Overrides

    /// <summary>
    /// Returns a string representation of the object properties using reflection helper.
    /// </summary>
    public override string ToString() => this.ToStringProperty();

    #endregion Overrides

}