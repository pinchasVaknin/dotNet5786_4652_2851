namespace BlApi;
using System.Collections.Generic;

//==================== IOrder Service Contract ===================\\

/// <summary>
/// Logical service contract for order-related operations.
/// Includes CRUD, List Retrieval, Order Actions (Cancel, Assign, Complete), and Summaries.
/// </summary>
public interface IOrder
{
    //==================== Order Management (Lists & Summaries) ===================\\

    #region ListAndSummary

    /// <summary>
    /// Returns summary counts of orders by all logical status combinations.
    /// The array index corresponds to a logical order status (combined from
    /// order status and on-time/late status).
    /// </summary>
    /// <param name="requesterId">The ID of the requester (must be an admin).</param>
    /// <returns>
    /// An array of counts, where each cell i holds the number of orders
    /// whose logical status equals status-i.
    /// </returns>
    int[] GetOrderStatusSummary(int requesterId);

    /// <summary>
    /// Retrieves a list of orders for the management screen.
    /// Each order appears at most once, with its latest delivery (if any).
    /// </summary>
    /// <param name="requesterId">The ID of the requester (admin).</param>
    /// <param name="filterField">
    /// Nullable enum indicating which field of OrderInList to filter by.
    /// If null, no filtering is applied.
    /// </param>
    /// <param name="filterValue">
    /// Nullable value to compare against the chosen filterField.
    /// Used only when filterField is not null.
    /// </param>
    /// <param name="sortBy">
    /// Nullable enum indicating which field of OrderInList to sort by.
    /// If null, the list is sorted by order status.
    /// </param>
    /// <returns>
    /// A filtered and sorted collection of <see cref="BO.OrderInList"/> objects.
    /// </returns>
    IEnumerable<BO.OrderInList> GetOrders(
        int requesterId,
        BO.OrderInListFilterBy? filterField = null,
        object? filterValue = null,
        BO.OrderInListSortBy? sortBy = null);

    /// <summary>
    /// Retrieves a list of closed deliveries handled by a specific courier.
    /// </summary>
    /// <param name="requesterId">The ID of the requester (admin or that courier).</param>
    /// <param name="courierId">The courier whose closed deliveries are requested.</param>
    /// <param name="typeFilter">
    /// Nullable enum indicating the order type to filter by.
    /// If null, all order types are returned.
    /// </param>
    /// <param name="sortBy">
    /// Nullable enum indicating the sort field.
    /// If null, the list is sorted by finish status and on-time status.
    /// </param>
    /// <returns>
    /// A filtered and sorted collection of <see cref="BO.ClosedDeliveryInList"/> objects.
    /// </returns>
    IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesByCourier(
        int requesterId,
        int courierId,
        BO.TypeOfOrder? typeFilter = null,
        BO.ClosedDeliverySortBy? sortBy = null);

    /// <summary>
    /// Retrieves a list of open orders that a courier may choose to handle.
    /// </summary>
    /// <param name="requesterId">The ID of the requester (admin or that courier).</param>
    /// <param name="courierId">
    /// The ID of the courier for whom open orders are requested,
    /// including air-distance from the courier's current location.
    /// </param>
    /// <param name="typeFilter">
    /// Nullable enum indicating the order type to filter by.
    /// If null, all open orders are returned.
    /// </param>
    /// <param name="sortBy">
    /// Nullable enum indicating the sort field.
    /// If null, the list is sorted by on-time status.
    /// </param>
    /// <returns>
    /// A filtered and sorted collection of <see cref="BO.OpenOrderInList"/> objects,
    /// including only orders that fit the courier's personal maximum air distance.
    /// </returns>
    IEnumerable<BO.OpenOrderInList> GetOpenOrdersForCourier(
        int requesterId,
        int courierId,
        BO.TypeOfOrder? typeFilter = null,
        BO.OpenOrderSortBy? sortBy = null);

    #endregion ListAndSummary

    //==================== CRUD Operations ===================\\

    #region CRUD

    /// <summary>
    /// Retrieves full logical details of a specific order.
    /// </summary>
    /// <param name="requesterId">The ID of the requester (admin or relevant courier, as defined logically).</param>
    /// <param name="orderId">The ID of the requested order.</param>
    /// <returns>A <see cref="BO.Order"/> object with full details.</returns>
    BO.Order GetOrder(int requesterId, int orderId);

    /// <summary>
    /// Adds a new order to the system.
    /// The order ID is generated automatically in the DAL.
    /// </summary>
    /// <param name="requesterId">The ID of the requester (admin).</param>
    /// <param name="order">The logical order object to add.</param>
    void AddOrder(int requesterId, BO.Order order);

    /// <summary>
    /// Updates details of an existing order.
    /// </summary>
    /// <param name="requesterId">The ID of the requester (admin).</param>
    /// <param name="order">The logical order object containing updated details.</param>
    void UpdateOrder(int requesterId, BO.Order order);

    /// <summary>
    /// Deletes an order (used only by BlTest, not by the UI).
    /// Always throws a logical exception indicating that orders cannot be deleted.
    /// </summary>
    /// <param name="requesterId">The ID of the requester.</param>
    /// <param name="orderId">The ID of the order to delete.</param>
    void DeleteOrder(int requesterId, int orderId);

    #endregion CRUD

    //==================== Order Actions (State Change) ===================\\

    #region OrderActions

    /// <summary>
    /// Cancels an order.
    /// </summary>
    /// <param name="requesterId">The ID of the requester (admin or allowed user).</param>
    /// <param name="orderId">The ID of the order to cancel.</param>
    /// <remarks>
    /// The request is legal only if the order is open or in progress but not yet supplied.
    /// The implementation will either create a "virtual" delivery (for open orders)
    /// or update the existing delivery (for orders in progress) with finish type Canceled
    /// and finish time equal to the logical system clock.
    /// </remarks>
    void CancelOrder(int requesterId, int orderId);

    /// <summary>
    /// Assigns an order to a courier for handling (start of delivery).
    /// </summary>
    /// <param name="requesterId">The ID of the requester (admin or the courier himself, as defined logically).</param>
    /// <param name="courierId">The ID of the courier taking the order.</param>
    /// <param name="orderId">The ID of the order chosen for handling.</param>
    void AssignOrderToCourier(int requesterId, int courierId, int orderId);

    /// <summary>
    /// Marks the end of handling an order by a courier (delivery supplied).
    /// </summary>
    /// <param name="requesterId">The ID of the requester (must be the delivering courier).</param>
    /// <param name="courierId">The ID of the courier (Teudat Zehut).</param>
    /// <param name="deliveryId">The ID of the delivery for the order being completed.</param>
    void CompleteOrderHandling(int requesterId, int courierId, int deliveryId);

    #endregion OrderActions

}