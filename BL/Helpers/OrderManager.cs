namespace Helpers;


using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;


internal static class OrderManager
{
    private static IDal s_dal = Factory.Get;

    internal static void SaveOrder(BO.Order Order)
    {
        try
        {
            // Map the business object Order to a data object Order
            DO.Order doOrder = ConvertBoToDoOrder(Order);

            // Create or update the Order in the data access layer
            var existing = s_dal.Order.Read(c => c.OrderId == Order.OrderId);

            if (existing is null)
                s_dal.Order.Create(doOrder);
            else
                s_dal.Order.Update(doOrder);
        }
        catch (DalAlreadyExistsException ex)
        {
            // Wrap and rethrow any exceptions that occur during the save operation
            throw new Exception("Failed to save Order", ex);
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            // Wrap and rethrow any exceptions that occur during the save operation
            throw new Exception("Failed to save Order", ex);
        }
        catch (DalDoesNotExistException ex)
        {
            // Wrap and rethrow any exceptions that occur during the save operation
            throw new Exception("Failed to save Order", ex);
        }
    }

    internal static BO.Order GetOrder(int id) // read Order by id
    {
        try
        {
            // Read the Order from the data access layer
            DO.Order doOrder = s_dal.Order.Read(id)
                ?? throw new Exception($"Order with ID={id} does not exist");

            // Build and return the business object representation of the Order
            return ConvertDoToBoOrder(doOrder);
        }
        catch (Exception ex)
        {
            // Wrap and rethrow any exceptions that occur during the process
            throw new Exception("Failed to load Order", ex);
        }
    }

    internal static IEnumerable<BO.OrderInList> GetOrders() // read all Orders - query syntax
    {
        try
        {
            // Maximum allowed delivery time range from configuration
            var maxRange = s_dal.Config.MaxDelTimeRnge;

            var allOrders = s_dal.Order.ReadAll();
            var allDeliveries = s_dal.Delivery.ReadAll();

            var query =
                from o in allOrders
                join d in allDeliveries
                    on o.OrderId equals d.OrderId into deliveriesGroup

                let lastDelivery =
                    deliveriesGroup.OrderByDescending(del => del.DeliveryDate).FirstOrDefault()

                let AirDistance =
                    Tools.DistanceKm(o.OrderLatitude, o.OrderLongitude, 31.7479, 35.188)

                let OrderStatus =
                    o.OrderDate < lastDelivery.DeliveryDate ? BO.OrderStatus.Open :
                    lastDelivery.DeliveryDate < lastDelivery.DeliveryFinishDate ?
                        BO.OrderStatus.InProgress :
                    lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.Completed ?
                        BO.OrderStatus.Supplied :
                    lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.Cancelled ?
                        BO.OrderStatus.Canceled :
                    BO.OrderStatus.Refused

                let TimeLeftToFinish = 
                        lastDelivery is null || (lastDelivery.DeliveryDate + maxRange) < s_dal.Config.Clock ?
                            TimeSpan.Zero :
                        (lastDelivery.DeliveryDate + maxRange) - s_dal.Config.Clock


                let TotalHandleTime = 
                    (from del in deliveriesGroup
                     where del.DeliveryFinishType == DO.DeliveryFinishType.Completed
                     select (del.DeliveryFinishDate - o.OrderDate).TotalMinutes).Sum()

                let TotalDeliveries =
                    deliveriesGroup.Count()

                select new BO.OrderInList
                {
                    DeliveryId = lastDelivery?.DeliveryId,
                    OrderId = o.OrderId,
                    TypeOfOrder = (BO.TypeOfOrder)o.TypeOfOrder,
                    AirDistance = AirDistance,
                    OrderStatus = OrderStatus,
                    ScheduleStatus = (BO.ScheduleStatus)o.ScheduleStatus,
                    TimeLeftToFinish = TimeLeftToFinish,
                    TotalHandleTime = TotalHandleTime,
                    TotalDeliveries = TotalDeliveries
                };

        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new Exception("Failed to load orders list (query syntax)", ex);
        }
    }






}

