namespace BlApi;
using System.Collections.Generic;

//==================== IOrder Service Contract ===================\\

/// <summary>
/// Logical service contract for order-related operations.
/// Includes CRUD, list retrieval, order actions, and summaries.
/// </summary>
public interface IOrder : IObservable //stage 5
{
    //==================== Order Management (Lists & Summaries) ===================\\

    #region ListAndSummary

    /// <summary>
    /// Returns counts of orders by logical status.
    /// </summary>
    /// <param name="requesterId">Requester ID (must be admin).</param>
    /// <returns>Array of counts by logical status index.</returns>
    int[] GetOrderStatusSummary(int requesterId);

    /// <summary>
    /// Gets orders list with optional filtering and sorting.
    /// </summary>
    /// <param name="requesterId">Requester ID (must be admin).</param>
    /// <param name="filterField">Optional field to filter by.</param>
    /// <param name="filterValue">Optional value for the filter.</param>
    /// <param name="sortBy">Optional field to sort by.</param>
    /// <returns>Collection of <see cref="BO.OrderInList"/>.</returns>
    IEnumerable<BO.OrderInList> GetOrders(
        int requesterId,
        BO.OrderInListFilterBy? filterField = null,
        object? filterValue = null,
        BO.OrderInListSortBy? sortBy = null);

    /// <summary>
    /// Gets closed deliveries for a specific courier.
    /// </summary>
    /// <param name="requesterId">Requester ID (admin or that courier).</param>
    /// <param name="courierId">Courier ID.</param>
    /// <param name="typeFilter">Optional order type filter.</param>
    /// <param name="sortBy">Optional sort field.</param>
    /// <returns>Collection of <see cref="BO.ClosedDeliveryInList"/>.</returns>
    IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesByCourier(
        int requesterId,
        int courierId,
        BO.TypeOfOrder? typeFilter = null,
        BO.ClosedDeliverySortBy? sortBy = null);

    /// <summary>
    /// Gets open orders that a courier can choose.
    /// </summary>
    /// <param name="requesterId">Requester ID (admin or that courier).</param>
    /// <param name="courierId">Courier ID requesting the list.</param>
    /// <param name="typeFilter">Optional order type filter.</param>
    /// <param name="sortBy">Optional sort field.</param>
    /// <returns>Collection of <see cref="BO.OpenOrderInList"/>.</returns>
    Task<IEnumerable<BO.OpenOrderInList>> GetOpenOrdersForCourier(
        int requesterId,
        int courierId,
        BO.TypeOfOrder? typeFilter = null,
        BO.OpenOrderSortBy? sortBy = null);

    #endregion ListAndSummary

    //==================== CRUD Operations ===================\\

    #region CRUD

    /// <summary>
    /// Gets full order details by order ID.
    /// </summary>
    /// <param name="requesterId">Requester ID (admin or allowed courier).</param>
    /// <param name="orderId">Order ID.</param>
    /// <returns>The <see cref="BO.Order"/> details.</returns>
    BO.Order GetOrder(int requesterId, int orderId);

    /// <summary>
    /// Adds a new order to the system.
    /// </summary>
    /// <param name="requesterId">Requester ID (must be admin).</param>
    /// <param name="order">Order details to add.</param>
    Task AddOrder(int requesterId, BO.Order order);

    /// <summary>
    /// Updates an existing order.
    /// </summary>
    /// <param name="requesterId">Requester ID (must be admin).</param>
    /// <param name="order">Updated order details.</param>
    Task UpdateOrder(int requesterId, BO.Order order);

    /// <summary>
    /// Deletes an order by order ID.
    /// </summary>
    /// <param name="requesterId">Requester ID.</param>
    /// <param name="orderId">Order ID to delete.</param>
    void DeleteOrder(int requesterId, int orderId);

    #endregion CRUD

    //==================== Order Actions (State Change) ===================\\

    #region OrderActions

    /// <summary>
    /// Cancels an order by order ID.
    /// </summary>
    /// <param name="requesterId">Requester ID (admin or allowed user).</param>
    /// <param name="orderId">Order ID to cancel.</param>
    void CancelOrder(int requesterId, int orderId);

    /// <summary>
    /// Assigns an order to a courier for handling.
    /// </summary>
    /// <param name="requesterId">Requester ID (admin or that courier).</param>
    /// <param name="courierId">Courier ID.</param>
    /// <param name="orderId">Order ID to assign.</param>
    /// <param name="actualDistance">Optional actual distance override.</param>
    Task AssignOrderToCourier(int requesterId, int courierId, int orderId, double? actualDistance = null);

    /// <summary>
    /// Completes handling of a delivery by a courier.
    /// </summary>
    /// <param name="requesterId">Requester ID (must be the courier).</param>
    /// <param name="courierId">Courier ID.</param>
    /// <param name="deliveryId">Delivery ID to complete.</param>
    /// <param name="finishType">Finish type (e.g., Supplied/Canceled).</param>
    void CompleteOrderHandling(int requesterId, int courierId, int deliveryId, BO.DeliveryFinishType finishType);

    #endregion OrderActions

    //==================== Delivery History Retrieval ===================\\

    #region DeliveryHistory

    /// <summary>
    /// Gets delivery history of an order for a courier.
    /// </summary>
    /// <param name="requesterId">Requester ID (admin or that courier).</param>
    /// <param name="courierId">Courier ID.</param>
    /// <param name="orderId">Order ID.</param>
    /// <returns>Collection of <see cref="BO.DeliveryPerOrderInList"/>.</returns>
    IEnumerable<BO.DeliveryPerOrderInList> GetDeliveryHistoryForCourier(int requesterId, int courierId, int orderId);

    #endregion DeliveryHistory

    //==================== Additional Order Operations ===================\\

    #region AdditionalOperations

    /// <summary>
    /// Updates order items and quantities.
    /// </summary>
    /// <param name="order">Order to update.</param>
    /// <param name="items">Items list (model, quantity).</param>
    void UpdateOrderDetails(BO.Order order, IEnumerable<(string Model, int Quantity)> items);

    /// <summary>
    /// Returns product price by model name.
    /// </summary>
    /// <param name="modelName">Product model name.</param>
    /// <returns>Product price.</returns>
    double GetProductPrice(string modelName);

    #endregion AdditionalOperations

}
