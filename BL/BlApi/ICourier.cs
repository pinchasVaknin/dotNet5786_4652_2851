namespace BlApi;
using System.Collections.Generic;

//==================== ICourier Service Contract ===================\\

/// <summary>
/// Logical service contract for courier-related operations.
/// Includes authentication, list retrieval, and courier CRUD actions.
/// </summary>
public interface ICourier : IObservable //stage 5
{
    //==================== Authentication ===================\\

    #region Authentication

    /// <summary>
    /// Authenticates a courier by ID and password.
    /// </summary>
    /// <param name="userId">Courier ID attempting to log in.</param>
    /// <param name="password">Password for authentication.</param>
    /// <returns>The role of the authenticated user.</returns>
    BO.UserRole Login(int userId, string password);

    #endregion Authentication

    //==================== List Retrieval ===================\\

    #region ListRetrieval

    /// <summary>
    /// Retrieves a list of couriers with optional filtering and sorting.
    /// </summary>
    /// <param name="requesterId">The requester ID (must be admin).</param>
    /// <param name="filterBy">Optional filter criterion.</param>
    /// <param name="filterValue">Optional value for the selected filter.</param>
    /// <param name="sortBy">Optional sort criterion.</param>
    /// <returns>A list of couriers in list view format.</returns>
    IEnumerable<BO.CourierInList> GetCouriers(
        int requesterId,
        BO.CourierInListFilterBy? filterBy = null,
        object? filterValue = null,
        BO.CourierInListSortBy? sortBy = null);

    #endregion ListRetrieval

    //==================== CRUD Operations ===================\\

    #region CRUD

    /// <summary>
    /// Gets full courier details by courier ID.
    /// </summary>
    /// <param name="requesterId">Requester ID (admin or same courier).</param>
    /// <param name="courierId">Courier ID to retrieve.</param>
    /// <returns>The courier details.</returns>
    BO.Courier GetCourier(int requesterId, int courierId);

    /// <summary>
    /// Adds a new courier to the system.
    /// </summary>
    /// <param name="requesterId">Requester ID (must be admin).</param>
    /// <param name="courier">Courier details to add.</param>
    void AddCourier(int requesterId, BO.Courier courier);

    /// <summary>
    /// Updates an existing courier details.
    /// </summary>
    /// <param name="requesterId">Requester ID (admin or same courier).</param>
    /// <param name="courier">Updated courier details.</param>
    void UpdateCourier(int requesterId, BO.Courier courier);

    /// <summary>
    /// Deletes a courier by courier ID.
    /// </summary>
    /// <param name="requesterId">Requester ID (must be admin).</param>
    /// <param name="courierId">Courier ID to delete.</param>
    /// <remarks>
    /// Allowed only if courier has no active handling and meets logical deletion rules.
    /// </remarks>
    void DeleteCourier(int requesterId, int courierId);

    #endregion CRUD

    //==================== Additional Methods ===================\\

    #region AdditionalMethods

    /// <summary>
    /// Checks whether a courier can be deleted.
    /// </summary>
    /// <param name="courierId">Courier ID to check.</param>
    /// <returns>True if the courier is allowed to be deleted.</returns>
    public bool IsCourierDeletable(int courierId);

    #endregion AdditionalMethods

}
