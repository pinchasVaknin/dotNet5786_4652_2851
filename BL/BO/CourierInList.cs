namespace BO;
using Helpers;
using System;

//==================== Courier Entity (List View) ===================\\

/// <summary>
/// Represents a summary of a courier for list display purposes.
/// Includes identification, operational status, and performance statistics.
/// </summary>
public class CourierInList
{
    //==================== Identity ===================\\

    #region Identity

    // The unique identifier of the courier.
    public int CourierId { get; init; }

    // The courier's full name.
    public string CourierFullName { get; set; }

    #endregion Identity

    //==================== Operational Status ===================\\

    #region OperationalStatus

    // Indicates whether the courier is active in the system.
    public bool CourierIsActive { get; set; }

    // The type of vehicle used by the courier.
    public VehicleType VehicleType { get; init; }

    // The date the courier started working.
    public DateTime? StartWorkDate { get; init; }

    #endregion OperationalStatus

    //==================== Performance & State ===================\\

    #region PerformanceAndState

    // Total number of deliveries completed on time.
    public int DeliveriesInTime { get; init; }

    // Total number of deliveries completed late.
    public int DeliveriesOverTime { get; init; }

    // The ID of the order currently being handled (nullable if idle).
    public int? OrderIdInHandle { get; init; }

    #endregion PerformanceAndState

    //==================== Overrides ===================\\

    #region Overrides

    /// <summary>
    /// Returns a string representation of the object properties using reflection helper.
    /// </summary>
    public override string ToString() => this.ToStringProperty();

    #endregion Overrides

}