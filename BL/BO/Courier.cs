namespace BO;
using Helpers;
using System;

//==================== Courier Entity ===================\\

/// <summary>
/// Represents a courier responsible for delivering orders.
/// Contains personal details, operational status, vehicle info, and performance metrics.
/// </summary>
public class Courier
{
    //==================== Identity & Contact ===================\\

    #region IdentityAndContact

    // The unique identifier of the courier (ID card number).
    public int CourierId { get; init; }

    // The courier's full name.
    public string CourierFullName { get; set; }

    // The courier's contact phone number.
    public string CourierCellPhone { get; set; }

    // The courier's email address.
    public string CourierEmail { get; set; }

    // The password used for system login.
    public string CourierPassword { get; set; }

    #endregion IdentityAndContact

    //==================== Operational Details ===================\\

    #region OperationalDetails

    // Indicates whether the courier is currently active/enabled in the system.
    public bool CourierIsActive { get; set; }

    // The current address/location of the courier.
    public string CourierLocation { get; set; }

    // The maximum distance the courier is willing/able to travel (nullable).
    public double? MaxCourierDistance { get; set; }

    // The type of vehicle used by the courier (Car, Motorcycle, etc.).
    public VehicleType VehicleType { get; set; }

    // The date when the courier started working.
    public DateTime? StartWorkDate { get; init; }

    #endregion OperationalDetails

    //==================== Statistics ===================\\

    #region Statistics

    // Total number of deliveries completed on time.
    public int TotalOnTimeDeliveries { get; init; }

    // Total number of deliveries completed late.
    public int TotalLateDeliveries { get; init; }

    #endregion Statistics

    //==================== Current State ===================\\

    #region CurrentState

    // Details of the order currently being delivered, if any (nullable).
    public BO.OrderInProgress? OrderInProgress { get; set; }

    #endregion CurrentState

    //==================== Overrides ===================\\

    #region Overrides

    /// <summary>
    /// Returns a string representation of the object properties using reflection helper.
    /// </summary>
    public override string ToString() => this.ToStringProperty();

    #endregion Overrides

}