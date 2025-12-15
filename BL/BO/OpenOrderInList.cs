namespace BO;
using Helpers;
using System;

//==================== Open Order Entity (List View) ===================\\

/// <summary>
/// Represents a summary of an open order specifically for courier assignment views.
/// Contains physical specs and calculated logistics data to help match orders to couriers.
/// </summary>
public class OpenOrderInList
{
    //==================== Identification ===================\\

    #region Identification

    // The identifier of the courier currently assigned (if any, usually null for open orders).
    public int? CourierId { get; init; }

    // The unique identifier of the order.
    public int OrderId { get; init; }

    // The category/type of the order.
    public TypeOfOrder TypeOfOrder { get; init; }

    #endregion Identification

    //==================== Physical Specifications ===================\\

    #region PhysicalSpecs

    // The weight of the package.
    public double OrderWeight { get; init; }

    // Indicates if the package is fragile.
    public bool IsFragile { get; init; }

    // The size/volume factor of the package.
    public double OrderSize { get; init; }

    #endregion PhysicalSpecs

    //==================== Location & Logistics ===================\\

    #region Logistics

    // The destination address string.
    public string CustomerAddress { get; init; }

    // The straight-line distance from the courier to the order (Km).
    public double AirDistance { get; init; }

    // The estimated driving/walking distance (Km), nullable if calculation failed.
    public double? ActualDistance { get; init; }

    #endregion Logistics

    //==================== Timing & Status ===================\\

    #region TimingAndStatus

    // The estimated time to travel the actual distance.
    public TimeSpan? EstimatedActualTime { get; init; }

    // The schedule status (OnTime, InRisk, Late).
    public ScheduleStatus ScheduleStatus { get; init; }

    // The time remaining until the max delivery deadline.
    public TimeSpan TimeLeftToFinish { get; init; }

    // The absolute deadline for this delivery.
    public DateTime MaxDeliveryTime { get; init; }

    #endregion TimingAndStatus

    //==================== Overrides ===================\\

    #region Overrides

    /// <summary>
    /// Returns a string representation of the object properties using reflection helper.
    /// </summary>
    public override string ToString() => this.ToStringProperty();

    #endregion Overrides
}