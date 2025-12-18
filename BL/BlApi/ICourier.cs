namespace BlApi;
using System.Collections.Generic;

//==================== ICourier Service Contract ===================\\

/// <summary>
/// Logical service contract for courier-related operations.
/// Includes Authentication, List Management, and CRUD operations for Couriers.
/// </summary>
public interface ICourier : IObservable //stage 5
{
    //==================== Authentication ===================\\

    #region Authentication

    /// <summary>
    /// Logs a user (courier or admin) into the system using ID and password.
    /// </summary>
    /// <param name="userId">The ID of the user trying to log in (Teudat Zehut).</param>
    /// <param name="password">The password provided by the user.</param>
    /// <returns>
    /// The logical role of the user (Admin or Courier).
    /// </returns>
    /// <exception cref="BO.BlLoginFailedException">
    /// Thrown if the user does not exist or the password is incorrect.
    /// </exception>
    BO.UserRole Login(int userId, string password);

    #endregion Authentication

    //==================== List Retrieval ===================\\

    #region ListRetrieval

    /// <summary>
    /// Retrieves a list of couriers for management screens.
    /// </summary>
    /// <param name="requesterId">The ID of the requester (must be an admin).</param>
    /// <param name="isActiveFilter">
    /// Optional filter for active/inactive couriers:
    /// null - no filter (all),
    /// true - only active couriers,
    /// false - only inactive couriers.
    /// </param>
    /// <param name="sortBy">
    /// Optional sorting criteria. If null, the list is sorted by ID.
    /// </param>
    /// <returns>
    /// A collection of <see cref="BO.CourierInList"/> objects.
    /// </returns>
    IEnumerable<BO.CourierInList> GetCouriers(
        int requesterId,
        bool? isActiveFilter = null,
        BO.VehicleType? vehicleFilter = null,
        BO.CourierListSortBy? sortBy = null);

    #endregion ListRetrieval

    //==================== CRUD Operations ===================\\

    #region CRUD

    /// <summary>
    /// Retrieves full details of a specific courier.
    /// </summary>
    /// <param name="requesterId">
    /// The ID of the requester (admin or the courier himself).
    /// </param>
    /// <param name="courierId">The ID of the courier whose details are requested.</param>
    /// <returns>A <see cref="BO.Courier"/> object with full details.</returns>
    BO.Courier GetCourier(int requesterId, int courierId);

    /// <summary>
    /// Adds a new courier to the system.
    /// </summary>
    /// <param name="requesterId">The ID of the requester (must be an admin).</param>
    /// <param name="courier">
    /// The new courier object to add (logical entity BO.Courier).
    /// </param>
    void AddCourier(int requesterId, BO.Courier courier);

    /// <summary>
    /// Updates details of an existing courier.
    /// </summary>
    /// <param name="requesterId">
    /// The ID of the requester (admin or the courier himself,
    /// depending on which fields are being changed).
    /// </param>
    /// <param name="courier">
    /// The courier object containing updated details.
    /// </param>
    void UpdateCourier(int requesterId, BO.Courier courier);

    /// <summary>
    /// Deletes a courier from the system.
    /// </summary>
    /// <param name="requesterId">The ID of the requester (must be an admin).</param>
    /// <param name="courierId">The ID of the courier to delete.</param>
    /// <remarks>
    /// A courier can be deleted only if they never handled any order
    /// or are not currently handling an order, according to the logical rules.
    /// </remarks>
    void DeleteCourier(int requesterId, int courierId);

    #endregion CRUD

}