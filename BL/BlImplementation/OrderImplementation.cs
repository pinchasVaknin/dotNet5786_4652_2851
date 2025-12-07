namespace BlImplementation;
using BlApi;
using DO;
using Helpers;
using System.Collections.Generic;

internal class OrderImplementation : IOrder
{

    //======== Order Operations ========\\

    public void AddOrder(int requesterId, BO.Order order)
    {
        Tools.EnsureAdmin(requesterId, nameof(AddOrder));
        OrderManager.AddOrder(order);
    }

    public void UpdateOrder(int requesterId, BO.Order order)
    {
        Tools.EnsureAdmin(requesterId, nameof(UpdateOrder));
        OrderManager.UpdateOrder(order);
    }

    public BO.Order GetOrder(int requesterId, int orderId)
    {
        Tools.EnsureAdmin(requesterId, nameof(GetOrder));
        return OrderManager.GetOrder(orderId);
    }

    public void DeleteOrder(int requesterId, int orderId)
    {
        Tools.EnsureAdmin(requesterId, nameof(DeleteOrder));
        OrderManager.DeleteOrder(orderId);
    }



    public void CompleteOrderHandling(int requesterId, int courierId, int deliveryId)
    {
        Tools.EnsureAdmin(requesterId, nameof(CompleteOrderHandling));
        OrderManager.CompleteOrderHandling(courierId, deliveryId);
    }

    public void CancelOrder(int requesterId, int orderId)
    {
        Tools.EnsureAdmin(requesterId, nameof(CancelOrder));
        OrderManager.CancelOrder(orderId);
    }

    public void AssignOrderToCourier(int requesterId, int courierId, int orderId)
    {
        throw new NotImplementedException();
    }

    

    

    

    public IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesByCourier(int requesterId, int courierId, TypeOfOrder? typeFilter = null, BO.ClosedDeliverySortBy? sortBy = null)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<BO.OpenOrderInList> GetOpenOrdersForCourier(int requesterId, int courierId, TypeOfOrder? typeFilter = null, BO.OpenOrderSortBy? sortBy = null)
    {
        throw new NotImplementedException();
    }

    

    public IEnumerable<BO.OrderInList> GetOrders(int requesterId, BO.OrderInListFilterBy? filterField = null, object? filterValue = null, BO.OrderInListSortBy? sortBy = null)
    {
        throw new NotImplementedException();
    }

    public int[] GetOrderStatusSummary(int requesterId)
    {
        throw new NotImplementedException();
    }

    
}
