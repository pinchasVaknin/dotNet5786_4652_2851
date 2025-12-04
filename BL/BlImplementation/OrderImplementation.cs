namespace BlImplementation;
using BlApi;
using BO;
using System.Collections.Generic;

internal class OrderImplementation : IOrder
{
    public void AddOrder(int requesterId, Order order)
    {
        throw new NotImplementedException();
    }

    public void AssignOrderToCourier(int requesterId, int courierId, int orderId)
    {
        throw new NotImplementedException();
    }

    public void CancelOrder(int requesterId, int orderId)
    {
        throw new NotImplementedException();
    }

    public void CompleteOrderHandling(int requesterId, int courierId, int deliveryId)
    {
        throw new NotImplementedException();
    }

    public void DeleteOrder(int requesterId, int orderId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ClosedDeliveryInList> GetClosedDeliveriesByCourier(int requesterId, int courierId, TypeOfOrder? typeFilter = null, ClosedDeliverySortBy? sortBy = null)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<OpenOrderInList> GetOpenOrdersForCourier(int requesterId, int courierId, TypeOfOrder? typeFilter = null, OpenOrderSortBy? sortBy = null)
    {
        throw new NotImplementedException();
    }

    public Order GetOrder(int requesterId, int orderId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<OrderInList> GetOrders(int requesterId, OrderInListFilterBy? filterField = null, object? filterValue = null, OrderInListSortBy? sortBy = null)
    {
        throw new NotImplementedException();
    }

    public int[] GetOrderStatusSummary(int requesterId)
    {
        throw new NotImplementedException();
    }

    public void UpdateOrder(int requesterId, Order order)
    {
        throw new NotImplementedException();
    }
}
