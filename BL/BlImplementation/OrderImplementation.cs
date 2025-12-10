namespace BlImplementation;
using BlApi;
using Helpers;
using System.Collections.Generic;

internal class OrderImplementation : IOrder
{

    //======== Order Operations ========\\

    #region CRUD Operations

    public void AddOrder(int requesterId, BO.Order order)
    {
        Tools.EnsureAdmin(requesterId, nameof(AddOrder));
        OrderManager.AddOrder(order);
    }

    public BO.Order GetOrder(int requesterId, int orderId)
    {
        Tools.EnsureAdmin(requesterId, nameof(GetOrder));
        return OrderManager.GetOrder(orderId);
    }

    public void UpdateOrder(int requesterId, BO.Order order)
    {
        Tools.EnsureAdmin(requesterId, nameof(UpdateOrder));
        OrderManager.UpdateOrder(order);
    }

    public void DeleteOrder(int requesterId, int orderId)
    {
        Tools.EnsureAdmin(requesterId, nameof(DeleteOrder));
        OrderManager.DeleteOrder(orderId);
    }

    #endregion CRUD Operations

    // ======== List Operations ========\\

    #region List Operations

    //======== List Retrieval ========\\

    public IEnumerable<BO.OrderInList> GetOrders(
           int requesterId,
           BO.OrderInListFilterBy? filterField = null,
           object? filterValue = null,
           BO.OrderInListSortBy? sortBy = null)
    {
        Tools.EnsureAdmin(requesterId, nameof(GetOrders));
        return OrderManager.GetOrders(filterField, filterValue, sortBy);
    }


    //======== Courier-Specific Lists ========\\
    public IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesByCourier(
            int requesterId,
            int courierId,
            BO.TypeOfOrder? typeFilter = null,
            BO.ClosedDeliverySortBy? sortBy = null)
    {
        if (requesterId != courierId)
            Tools.EnsureAdmin(requesterId, nameof(GetClosedDeliveriesByCourier));

        return OrderManager.GetClosedDeliveriesByCourier(courierId, typeFilter, sortBy);
    }

    public IEnumerable<BO.OpenOrderInList> GetOpenOrdersForCourier(
        int requesterId,
        int courierId,
        BO.TypeOfOrder? typeFilter = null,
        BO.OpenOrderSortBy? sortBy = null)
    {
        if (requesterId != courierId)
            Tools.EnsureAdmin(requesterId, nameof(GetClosedDeliveriesByCourier));

        return OrderManager.GetOpenOrdersForCourier(courierId, typeFilter, sortBy);
    }


    #endregion List Operations

    // ======== Order Management ========\\

    #region Order Management

    public int[] GetOrderStatusSummary(int requesterId)
    {
        Tools.EnsureAdmin(requesterId, nameof(GetOrderStatusSummary));
        return OrderManager.GetOrderStatusSummary();
    }

    public void CompleteOrderHandling(int requesterId, int courierId, int deliveryId)
    {
        if (requesterId != courierId)
            throw new BO.BlAdminPermissionException("Only the courier can complete the delivery");

        OrderManager.CompleteOrderHandling(courierId, deliveryId);
    }

    public void CancelOrder(int requesterId, int orderId)
    {
        Tools.EnsureAdmin(requesterId, nameof(CancelOrder));
        OrderManager.CancelOrder(orderId);
    }

    public void AssignOrderToCourier(int requesterId, int courierId, int orderId)
    {
        if (requesterId != courierId)
            Tools.EnsureAdmin(requesterId, nameof(AssignOrderToCourier));

        //Tools.EnsureAdmin(requesterId, nameof(AssignOrderToCourier));
        OrderManager.AssignOrderToCourier(courierId, orderId);
    }

    #endregion Order Management

}
