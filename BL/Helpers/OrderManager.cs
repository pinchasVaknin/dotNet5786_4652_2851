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
            var allOrders = s_dal.Order.ReadAll();
            var allDeliveries = s_dal.Delivery.ReadAll();

            var query =
                from o in allOrders
                join d in allDeliveries
                    on o.OrderId equals d.OrderId into delivery

                let AirDistance = DistanceKm(o.OrderLatitude, o.OrderLongitude, 31.7479, 35.188)

                let OrderStatus = o.OrderDate < d.DeliveryDate ?
                                    BO.OrderStatus.Open :
                                    d.DeliveryDate < d.DeliveryFinishDate ?
                                        BO.OrderStatus.InProgress :
                                        d.DeliveryFinishType == DO.DeliveryFinishType.Completed ?
                                            BO.OrderStatus.Supplied :
                                            d.DeliveryFinishType == DO.DeliveryFinishType.Cancelled ?
                                                BO.OrderStatus.Canceled :
                                                BO.OrderStatus.Refused






                // Open,
                //InProgress,
                //Supplied,
                //Refused,
                //Canceled

                let TimeLeftToFinish =


                let TotalHandleTime =

                let TotalDeliveries =








                select new BO.OrderInList
                {
                    DeliveryId = ordersdGroup.FirstOrDefault()?.DeliveryId,
                    OrderId = o.OrderId,
                    TypeOfOrder = (BO.TypeOfOrder)o.TypeOfOrder,
                    AirDistance = o.AirDistance,
                    OrderStatus = (BO.OrderStatus)o.OrderStatus,
                    ScheduleStatus = (BO.ScheduleStatus)o.ScheduleStatus,
                    TimeLeftToFinish = o.EstimatedTimeToFinish.HasValue ? TimeSpan.FromMinutes(o.EstimatedTimeToFinish.Value) : TimeSpan.Zero,
                    TotalHandleTime = TimeSpan.FromMinutes(o.TotalHandleTime),
                    TotalDeliveries = o.TotalDeliveries
                };



        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new Exception("Failed to load orders list (query syntax)", ex);
        }
    }






}

