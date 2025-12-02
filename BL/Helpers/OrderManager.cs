namespace Helpers;


using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;


internal static class OrderManager
{
    private static IDal s_dal = Factory.Get;

    internal static void SaveOrder(BO.Order Order) // create or update Order
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
            var maxRangeWithoutRisk = maxRange - s_dal.Config.RiskTimeRnge;

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
                    lastDelivery is null ? BO.OrderStatus.Open :
                    lastDelivery.DeliveryDate < lastDelivery.DeliveryFinishDate ?
                        BO.OrderStatus.InProgress :
                    lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.Completed ?
                        BO.OrderStatus.Supplied :
                    lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.Cancelled ?
                        BO.OrderStatus.Canceled :
                    BO.OrderStatus.Refused
                    
                let handleTime =
                    (lastDelivery is null) ?
                    s_dal.Config.Clock - o.OrderDate :
                    lastDelivery.DeliveryFinishDate - o.OrderDate

                let ScheduleStatus =
                    handleTime <= maxRangeWithoutRisk ?
                        BO.ScheduleStatus.OnTime :
                    handleTime <= maxRange ?
                        BO.ScheduleStatus.InRisk :
                    BO.ScheduleStatus.Late

                let TimeLeftToFinish = 
                        lastDelivery is null || (lastDelivery.DeliveryDate + maxRange) < s_dal.Config.Clock ?
                            TimeSpan.Zero :
                        (lastDelivery.DeliveryDate + maxRange) - s_dal.Config.Clock

                let TotalHandleTime =
                    (from del in deliveriesGroup
                     where del.DeliveryFinishType == DO.DeliveryFinishType.Completed
                     select del.DeliveryFinishDate - o.OrderDate)
                        .Aggregate(TimeSpan.Zero, (acc, span) => acc + span)

                let TotalDeliveries =
                    deliveriesGroup.Count()

                select new BO.OrderInList
                {
                    DeliveryId = lastDelivery?.DeliveryId,
                    OrderId = o.OrderId,
                    TypeOfOrder = (BO.TypeOfOrder)o.TypeOfOrder,
                    AirDistance = AirDistance,
                    OrderStatus = OrderStatus,
                    ScheduleStatus = ScheduleStatus,
                    TimeLeftToFinish = TimeLeftToFinish,
                    TotalHandleTime = TotalHandleTime,
                    TotalDeliveries = TotalDeliveries
                };
            return query.ToList();
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new Exception("Failed to load orders list (query syntax)", ex);
        }
    }

    internal static void DeleteOrders(int id)
    {
        try
        {
            // Check that order exists
            DO.Order? doOrder = s_dal.Order.Read(id)
                ?? throw new Exception($"Order with ID={id} does not exist");

            // Check if order has an active delivery
            bool hasActiveDelivery = s_dal.Delivery
                .ReadAll(d => d.OrderId == id)
                .Any(d => d.DeliveryFinishType == DO.DeliveryFinishType.None);

            if (hasActiveDelivery)
                throw new Exception($"Cannot delete order {id}: courier is on way with delivery.");

            // Perform deletion
            s_dal.Order.Delete(id);
        }
        catch (DalDoesNotExistException ex)
        {
            throw new Exception("Failed to delete order", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to delete order", ex);
        }

    }

    //-------------- Private Convert Methods ----------------\\

    private static BO.Order ConvertDoToBoOrder(DO.Order doOrder) // build BO.Order from DO.Order
    {
        

        return new BO.Order
        {
            OrderId = doOrder.OrderId,
            OrderFullName = doOrder.OrderFullName,
            OrderCellPhone = doOrder.OrderCellPhone,
            OrderEmail = doOrder.OrderEmail,
            OrderPassword = doOrder.OrderPassword,

            OrderIsActive = doOrder.OrderEnabled,
            MaxOrderDistance = doOrder.MaxOrderDistance,
            VehicleType = (BO.VehicleType)doOrder.OrderVehicleType,

            StartWorkDate = doOrder.SeniorityOfOrder,

            TotalOnTimeDeliveries = onTime,
            TotalLateDeliveries = late,

            OrderInProgress = orderInProgress
        };
    }

    private static DO.Order ConvertBoToDoOrder(BO.Order boOrder) =>
        new DO.Order(
            OrderId: boOrder.OrderId // 0 for new Order
        );


}

