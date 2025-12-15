namespace BlImplementation;

using BlApi;
using Helpers;
using System.Collections.Generic;

//==================== Courier Implementation ===================\\

/// <summary>
/// Implementation of the ICourier interface.
/// Acts as a facade/service layer that handles User Authentication
/// and delegates Courier CRUD operations to the CourierManager helper.
/// </summary>
internal class CourierImplementation : ICourier
{
    //==================== Authentication ===================\\

    #region Authentication

    /// <summary>
    /// Authenticates a user based on ID and Password.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>The <see cref="BO.UserRole"/> (Admin or Courier) if successful.</returns>
    /// <exception cref="BO.BlInvalidPasswordException">Thrown if authentication fails.</exception>
    /// <exception cref="BO.BlUserNotFoundException">Thrown if user does not exist.</exception>
    public BO.UserRole Login(int userId, string password)
    {
        return CourierManager.GetUserRole(userId, password);
    }

    #endregion Authentication

    //==================== List Retrieval ===================\\

    #region List Retrieval

    /// <summary>
    /// Retrieves a list of couriers, optionally filtered and sorted.
    /// Requires Admin privileges.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the list (Must be Admin).</param>
    /// <param name="isActiveFilter">Optional filter: true for active only, false for inactive.</param>
    /// <param name="sortBy">Optional sorting criteria.</param>
    /// <returns>A list of <see cref="BO.CourierInList"/>.</returns>
    public IEnumerable<BO.CourierInList> GetCouriers(
        int requesterId,
        bool? isActiveFilter = null,
        BO.CourierListSortBy? sortBy = null)
    {
        Tools.EnsureAdmin(requesterId, nameof(GetCouriers));
        return CourierManager.GetListOfCouriers(requesterId, isActiveFilter, sortBy);
    }

    #endregion List Retrieval

    //==================== CRUD Operations ===================\\

    #region CRUD Operations

    /// <summary>
    /// Adds a new courier to the system.
    /// Requires Admin privileges.
    /// </summary>
    /// <param name="requesterId">The ID of the user performing the action (Must be Admin).</param>
    /// <param name="courier">The courier details to add.</param>
    public void AddCourier(int requesterId, BO.Courier courier)
    {
        Tools.EnsureAdmin(requesterId, nameof(AddCourier));
        CourierManager.AddCourier(courier);
    }

    /// <summary>
    /// Retrieves full details of a specific courier.
    /// Requires Admin privileges.
    /// </summary>
    /// <param name="requesterId">The ID of the user performing the action (Must be Admin).</param>
    /// <param name="courierId">The ID of the courier to retrieve.</param>
    /// <returns>The <see cref="BO.Courier"/> object.</returns>
    public BO.Courier GetCourier(int requesterId, int courierId)
    {
        Tools.EnsureAdmin(requesterId, nameof(GetCourier));
        return CourierManager.GetCourier(courierId);
    }

    /// <summary>
    /// Updates an existing courier's details.
    /// Requires Admin privileges.
    /// </summary>
    /// <param name="requesterId">The ID of the user performing the action (Must be Admin).</param>
    /// <param name="courier">The courier details to update.</param>
    public void UpdateCourier(int requesterId, BO.Courier courier)
    {
        Tools.EnsureAdmin(requesterId, nameof(UpdateCourier));
        CourierManager.UpdateCourier(courier);
    }

    /// <summary>
    /// Deletes a courier from the system (if no active deliveries exist).
    /// Requires Admin privileges.
    /// </summary>
    /// <param name="requesterId">The ID of the user performing the action (Must be Admin).</param>
    /// <param name="courierId">The ID of the courier to delete.</param>
    public void DeleteCourier(int requesterId, int courierId)
    {
        Tools.EnsureAdmin(requesterId, nameof(DeleteCourier));
        CourierManager.DeleteCourier(courierId);
    }

    #endregion CRUD Operations

}