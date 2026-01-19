namespace BlImplementation;

using BlApi;
using Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

//==================== Order Implementation ===================\\

/// <summary>
/// Implementation of the IOrder interface.
/// Acts as a facade/service layer that enforces permissions (Admin/Courier)
/// and delegates business logic to the OrderManager helper.
/// </summary>
internal class OrderImplementation : IOrder
{
    //==================== CRUD Operations ===================\\

    #region CRUD Operations

    /// <summary>
    /// Adds a new order to the system. Requires Admin privileges.
    /// </summary>
    /// <param name="requesterId">The user requesting the action.</param>
    /// <param name="order">The order to add.</param>
    public async Task AddOrder(int requesterId, BO.Order order)
    {
        Tools.EnsureAdmin(requesterId, nameof(AddOrder));
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        await OrderManager.AddOrder(order);
    }

    /// <summary>
    /// Retrieves a specific order by ID. Requires Admin privileges.
    /// </summary>
    public BO.Order GetOrder(int requesterId, int orderId)
    {
        Tools.EnsureAdmin(requesterId, nameof(GetOrder));
        return OrderManager.GetOrder(orderId);
    }

    /// <summary>
    /// Updates an existing order. Requires Admin privileges.
    /// </summary>
    public async Task UpdateOrder(int requesterId, BO.Order order)
    {
        Tools.EnsureAdmin(requesterId, nameof(UpdateOrder));
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        await OrderManager.UpdateOrder(order);
    }

    /// <summary>
    /// Deletes an order from the system. Requires Admin privileges.
    /// </summary>
    public void DeleteOrder(int requesterId, int orderId)
    {
        Tools.EnsureAdmin(requesterId, nameof(DeleteOrder));
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        OrderManager.DeleteOrder(orderId);
    }

    #endregion CRUD Operations

    //==================== List Retrieval ===================\\

    #region List Operations

    /// <summary>
    /// Retrieves a list of orders with optional filters and sorting. Requires Admin privileges.
    /// </summary>
    public IEnumerable<BO.OrderInList> GetOrders(
            int requesterId,
            BO.OrderInListFilterBy? filterField = null,
            object? filterValue = null,
            BO.OrderInListSortBy? sortBy = null)
    {
        Tools.EnsureAdmin(requesterId, nameof(GetOrders));
        return OrderManager.GetOrders(filterField, filterValue, sortBy);
    }

    /// <summary>
    /// Retrieves closed deliveries for a specific courier.
    /// Accessible by the specific courier or an Admin.
    /// </summary>
    public IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesByCourier(
            int requesterId,
            int courierId,
            BO.TypeOfOrder? typeFilter = null,
            BO.ClosedDeliverySortBy? sortBy = null)
    {
        // Permission check: Requester must be the courier owner OR an Admin
        if (requesterId != courierId)
            Tools.EnsureAdmin(requesterId, nameof(GetClosedDeliveriesByCourier));

        return OrderManager.GetClosedDeliveriesByCourier(courierId, typeFilter, sortBy);
    }

    /// <summary>
    /// Retrieves open orders suitable for a specific courier.
    /// Accessible by the specific courier or an Admin.
    /// </summary>
    public async Task<IEnumerable<BO.OpenOrderInList>> GetOpenOrdersForCourier(
        int requesterId,
        int courierId,
        BO.TypeOfOrder? typeFilter = null,
        BO.OpenOrderSortBy? sortBy = null)
    {
        // Permission check: Requester must be the courier owner OR an Admin
        if (requesterId != courierId)
            Tools.EnsureAdmin(requesterId, nameof(GetOpenOrdersForCourier));

        return await OrderManager.GetOpenOrdersForCourierAsync(courierId, typeFilter, sortBy);
    }

    public IEnumerable<BO.DeliveryPerOrderInList> GetDeliveryHistoryForCourier(int requesterId, int courierId, int orderId)
    {
        return OrderManager.GetDeliveryHistoryForCourier(requesterId, courierId, orderId);
    }

    #endregion List Operations

    //==================== Business Actions ===================\\

    #region Order Management

    /// <summary>
    /// Returns a statistical summary of order statuses. Requires Admin privileges.
    /// </summary>
    public int[] GetOrderStatusSummary(int requesterId)
    {
        Tools.EnsureAdmin(requesterId, nameof(GetOrderStatusSummary));
        return OrderManager.GetOrderStatusSummary();
    }

    /// <summary>
    /// Marks a delivery as completed. Can only be performed by the assigned courier.
    /// </summary>
    public void CompleteOrderHandling(int requesterId, int courierId, int deliveryId, BO.DeliveryFinishType finishType)
    {
        // Strict permission check: Only the assigned courier can complete their own delivery
        if (requesterId != courierId)
            throw new BO.BlAdminPermissionException("Only the courier can complete the delivery");

        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        OrderManager.CompleteOrderHandling(courierId, deliveryId, finishType);
    }

    /// <summary>
    /// Cancels an order. Requires Admin privileges.
    /// </summary>
    public void CancelOrder(int requesterId, int orderId)
    {
        Tools.EnsureAdmin(requesterId, nameof(CancelOrder));
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        OrderManager.CancelOrder(orderId);
    }

    /// <summary>
    /// Assigns an order to a courier.
    /// Can be performed by the courier themselves (claiming an order) or by an Admin.
    /// </summary>
    public async Task AssignOrderToCourier(int requesterId, int courierId, int orderId, double? actualDistance = null)
    {
        // Permission check: If requester is NOT the courier, they must be Admin
        if (requesterId != courierId)
            Tools.EnsureAdmin(requesterId, nameof(AssignOrderToCourier));

        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        await OrderManager.AssignOrderToCourier(courierId, orderId, actualDistance);
    }

    #endregion Order Management

    //==================== Additional Functionalities ===================\\

    #region Additional Functionalities

    public void UpdateOrderDetails(BO.Order order, IEnumerable<(string Model, int Quantity)> items)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        OrderManager.UpdateOrderDetails(order, items);
    }

    public double GetProductPrice(string modelName) => OrderManager.GetProductPrice(modelName);

    #endregion Additional Functionalities

    //==================== Observer Implementation ===================\\

    #region Observer Implementation

    public void AddObserver(Action listObserver) =>
    OrderManager.Observers.AddListObserver(listObserver); //stage 5
    public void AddObserver(int id, Action observer) =>
    OrderManager.Observers.AddObserver(id, observer); //stage 5
    public void RemoveObserver(Action listObserver) =>
    OrderManager.Observers.RemoveListObserver(listObserver); //stage 5
    public void RemoveObserver(int id, Action observer) =>
    OrderManager.Observers.RemoveObserver(id, observer); //stage 5

    #endregion Observer Implementation

}