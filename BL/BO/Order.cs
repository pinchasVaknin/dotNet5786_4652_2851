namespace BO;
using Helpers;
using System;
using System.Collections.Generic;

//==================== Order Entity ===================\\

/// <summary>
/// Represents a full order entity with detailed information.
/// Includes customer details, location, physical specifications, timing, status, and delivery history.
/// </summary>
public class Order
{
    //==================== Identification & Type ===================\\

    #region Identification

    // The unique identifier of the order (Run Number).
    public int OrderId { get; init; }

    // Textual description or details of the items in the order.
    public string? OrderDetail { get; set; }

    // The category/type of the order (e.g., Smartphone, Laptop).
    public TypeOfOrder TypeOfOrder { get; set; }

    #endregion Identification

    //==================== Location & Distance ===================\\

    #region Location

    // The destination address string.
    public string OrderAddress { get; set; }

    // The latitude coordinate of the destination.
    public double OrderLatitude { get; set; }

    // The longitude coordinate of the destination.
    public double OrderLongitude { get; set; }

    // The straight-line distance from the company/hub to the destination (in Km).
    public double AirDistance { get; set; }

    #endregion Location

    //==================== Customer Details ===================\\

    #region CustomerDetails

    // The full name of the customer placing the order.
    public string CustomerFullName { get; set; }

    // The customer's contact phone number.
    public string CustomerPhone { get; set; }

    #endregion CustomerDetails

    //==================== Physical Specs ===================\\

    #region PhysicalSpecs

    // The weight of the package (in Kg).
    public double OrderWeight { get; set; }

    // The size/volume factor of the package.
    public double OrderSize { get; set; }

    // Indicates if the package requires careful handling.
    public bool IsFragile { get; set; }

    #endregion PhysicalSpecs

    //==================== Timing & Status ===================\\

    #region TimingAndStatus

    // The time when the order was created/opened.
    public DateTime OrderOpenTime { get; init; }

    // The estimated time of arrival based on the assigned courier (nullable).
    public DateTime? ExpectedDeliveryTime { get; init; }

    // The maximum deadline for the delivery.
    public DateTime MaxDeliveryTime { get; init; }

    // The time remaining until the deadline (TimeSpan).
    public TimeSpan TimeRemaining { get; init; }

    // The current logical status of the order (Open, InProgress, Supplied, etc.).
    public OrderStatus OrderStatus { get; init; }

    // The scheduling status (OnTime, InRisk, Late).
    public ScheduleStatus ScheduleStatus { get; init; }

    #endregion TimingAndStatus

    //==================== History ===================\\

    #region History

    // A list of delivery attempts/history associated with this order.
    public List<DeliveryPerOrderInList>? DeliveryPerOrderInList { get; init; }

    #endregion History

    //==================== Overrides ===================\\

    #region Overrides

    /// <summary>
    /// Returns a string representation of the object properties using reflection helper.
    /// </summary>
    public override string ToString() => this.ToStringProperty();

    #endregion Overrides

}