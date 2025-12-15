namespace BO;
using Helpers;

//==================== Order In Progress Entity ===================\\

/// <summary>
/// Represents an order that is currently being processed or delivered.
/// Aggregates specific details needed for tracking active deliveries.
/// </summary>
public class OrderInProgress
{
    //==================== Identifiers & Type ===================\\

    #region Identifiers

    // The unique identifier of the delivery (if assigned).
    public int DeliveryId { get; init; }

    // The unique identifier of the order.
    public int OrderId { get; init; }

    // The type/category of the order.
    public TypeOfOrder TypeOfOrder { get; init; }

    // Textual description of items.
    public string? OrderDetail { get; init; }

    #endregion Identifiers

    //==================== Customer & Location ===================\\

    #region CustomerAndLocation

    // The destination address.
    public string CustomerAddress { get; init; }

    // The customer's full name.
    public string CostumerFullName { get; init; }

    // The customer's phone number.
    public string CostumerPhone { get; init; }

    #endregion CustomerAndLocation

    //==================== Distances ===================\\

    #region Distances

    // The calculated straight-line distance (Km).
    public double AirDistance { get; init; }

    // The actual driving/walking distance (Km), nullable if calculation failed.
    public double? ActualDistance { get; init; }

    #endregion Distances

    //==================== Timing ===================\\

    #region Timing

    // The time the order was created.
    public DateTime OrderOpenTime { get; init; }

    // The time the delivery actually started.
    public DateTime DeliveryStartTime { get; init; }

    // The calculated ETA.
    public DateTime ExpectedDeliveryTime { get; init; }

    // The maximum deadline for delivery.
    public DateTime MaxDeliveryTime { get; init; }

    // The time remaining until the deadline.
    public TimeSpan TimeLeftToFinish { get; init; }

    #endregion Timing

    //==================== Status ===================\\

    #region Status

    // The current order status (e.g., InProgress).
    public OrderStatus OrderStatus { get; init; }

    // The scheduling status (OnTime, InRisk, Late).
    public ScheduleStatus ScheduleStatus { get; init; }

    #endregion Status

    //==================== Overrides ===================\\

    #region Overrides

    /// <summary>
    /// Returns a string representation of the object properties using reflection helper.
    /// </summary>
    public override string ToString() => this.ToStringProperty();

    #endregion Overrides

}