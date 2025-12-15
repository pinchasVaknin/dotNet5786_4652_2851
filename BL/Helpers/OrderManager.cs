namespace Helpers;

using DalApi;
using System;
using System.Collections.Generic;
using System.Linq;

//==================== Order Business Logic Manager ===================\\

/// <summary>
/// Manages logical operations for Orders.
/// Handles CRUD, state transitions (Assignment, Cancellation, Completion),
/// list filtering/sorting, and complex data aggregation.
/// </summary>
internal static class OrderManager
{
    //==================== DAL Access ===================\\

    #region DalAccess

    private static IDal s_dal = Factory.Get;

    #endregion DalAccess

    //==================== CRUD Operations (BO) ===================\\

    #region BoCrudMethods

    /// <summary>
    /// Creates a new order in the system.
    /// Validates data and converts address to coordinates.
    /// </summary>
    internal static void AddOrder(BO.Order order)
    {
        Tools.ValidateOrder(order);

        var coords = Tools.GetLocationFromAddress(order.OrderAddress);
        if (coords == null)
            throw new BO.BlInvalidStringException($"Address '{order.OrderAddress}' is invalid.");

        order.OrderLatitude = coords.Value.Lat ?? 0;
        order.OrderLongitude = coords.Value.Lon ?? 0;

        try
        {
            DO.Order doOrder = ConvertBoToDoOrder(order);
            s_dal.Order.Create(doOrder);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException("Failed to add order", ex);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to add order", ex);
        }
    }

    /// <summary>
    /// Retrieves an order by ID.
    /// </summary>
    internal static BO.Order GetOrder(int id)
    {
        Tools.ValidateSystemId(id);

        try
        {
            DO.Order doOrder = s_dal.Order.Read(id)
                ?? throw new BO.BlDoesNotExistException($"Order with ID={id} does not exist");

            return ConvertDoToBoOrder(doOrder);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Failed to get order", ex);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to get order", ex);
        }
    }

    /// <summary>
    /// Updates an existing order.
    /// Re-calculates coordinates if the address changed.
    /// </summary>
    internal static void UpdateOrder(BO.Order order)
    {
        Tools.ValidateOrder(order);

        var oldOrder = s_dal.Order.Read(order.OrderId); // Assuming read succeeds if we reached here validation wise, but simpler logic implies ID exists check inside Read or Update logic

        if (oldOrder.OrderAddress != order.OrderAddress)
        {
            var coords = Tools.GetLocationFromAddress(order.OrderAddress);
            if (coords == null)
                throw new BO.BlInvalidStringException($"New address '{order.OrderAddress}' is invalid.");

            order.OrderLatitude = coords.Value.Lat ?? 0;
            order.OrderLongitude = coords.Value.Lon ?? 0;
        }
        else
        {
            order.OrderLatitude = oldOrder.OrderLatitude;
            order.OrderLongitude = oldOrder.OrderLongitude;
        }

        try
        {
            DO.Order doOrder = ConvertBoToDoOrder(order);
            s_dal.Order.Update(doOrder);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to update order", ex);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Failed to update order", ex);
        }
    }

    /// <summary>
    /// Deletes an order if it has no active delivery.
    /// </summary>
    internal static void DeleteOrder(int id)
    {
        Tools.ValidateSystemId(id);

        try
        {
            DO.Order? doOrder = s_dal.Order.Read(id)
                ?? throw new BO.BlDoesNotExistException($"Order with ID={id} does not exist");

            // Check for active delivery
            bool hasActiveDelivery = s_dal.Delivery
                .ReadAll(d => d.OrderId == id)
                .Any(d => d.DeliveryFinishType == DO.DeliveryFinishType.None);

            if (hasActiveDelivery)
                throw new BO.BlOrderHasActiveDeliveryException($"Cannot delete order {id}: courier is on way with delivery.");

            s_dal.Order.Delete(id);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Failed to delete order", ex);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to delete order", ex);
        }
    }

    #endregion BoCrudMethods

    //==================== Order Actions (State Change) ===================\\

    #region OrderActions

    /// <summary>
    /// Cancels an order. If open, creates a cancelled delivery record. If in progress, updates the active delivery.
    /// </summary>
    internal static void CancelOrder(int orderId)
    {
        Tools.ValidateSystemId(orderId);

        try
        {
            var doOrder = s_dal.Order.Read(orderId)
                ?? throw new BO.BlDoesNotExistException($"Order with ID={orderId} does not exist.");

            var boOrder = ConvertDoToBoOrder(doOrder);

            if (boOrder.OrderStatus == BO.OrderStatus.Open)
            {
                // Create a "ghost" delivery to mark cancellation
                DO.Delivery delivery = new DO.Delivery
                {
                    DeliveryId = 0,
                    OrderId = orderId,
                    CourierId = 0,
                    DeliveryMaxDistance = null,
                    DeliveryDate = s_dal.Config.Clock,
                    DeliveryFinishDate = s_dal.Config.Clock,
                    ShipmentType = DO.ShipmentType.Standard,
                    DeliveryFinishType = DO.DeliveryFinishType.Cancelled
                };
                s_dal.Delivery.Create(delivery);
            }
            else if (boOrder.OrderStatus == BO.OrderStatus.InProgress)
            {
                // Update the running delivery
                var activeDelivery = s_dal.Delivery
                    .ReadAll(d => d.OrderId == orderId && d.DeliveryFinishType == DO.DeliveryFinishType.None)
                    .Single();

                s_dal.Delivery.Update(
                activeDelivery with
                {
                    DeliveryFinishDate = s_dal.Config.Clock,
                    DeliveryFinishType = DO.DeliveryFinishType.Cancelled
                });
            }
            else
            {
                throw new BO.BlOrderAlreadyCanceledException($"Order {orderId} is already canceled (or finished).");
            }
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Failed to cancel order", ex);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException("Failed to cancel order", ex);
        }
    }

    /// <summary>
    /// Marks a delivery as completed.
    /// </summary>
    internal static void CompleteOrderHandling(int courierId, int deliveryId)
    {
        Tools.ValidatePersonId(courierId);
        Tools.ValidateSystemId(deliveryId);

        try
        {
            var delivery = s_dal.Delivery.Read(deliveryId)
                ?? throw new BO.BlDoesNotExistException($"Delivery with ID={deliveryId} does not exist.");

            if (delivery.CourierId != courierId)
                throw new BO.BlCourierNotAssignedToDeliveryException(
                    $"Courier with ID={courierId} is not assigned to delivery ID={deliveryId}.");

            s_dal.Delivery.Update(
                delivery with
                {
                    DeliveryFinishDate = s_dal.Config.Clock,
                    DeliveryFinishType = DO.DeliveryFinishType.Completed
                });
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Failed to complete order handling", ex);
        }
    }

    /// <summary>
    /// Assigns an open order to a courier by creating a new active Delivery.
    /// </summary>
    internal static void AssignOrderToCourier(int courierId, int orderId)
    {
        Tools.ValidatePersonId(courierId);
        Tools.ValidateSystemId(orderId);

        try
        {
            DO.Order doOrder = s_dal.Order.Read(o => o.OrderId == orderId)
                ?? throw new DO.DalDoesNotExistException($"Order with ID={orderId} does not exist.");

            DO.Courier doCourier = s_dal.Courier.Read(c => c.CourierId == courierId)
                ?? throw new DO.DalDoesNotExistException($"Courier with ID={courierId} does not exist.");

            if (!doCourier.CourierEnabled)
                throw new BO.BlCourierDisabledException($"Courier {courierId} is disabled and cannot take orders.");

            BO.OrderStatus orderStatus = GetOrderStatus(doOrder);
            if (orderStatus != BO.OrderStatus.Open)
                throw new BO.BlOrderNotOpenForAssignmentException(
                    $"Order {orderId} is not open for assignment (current status: {orderStatus}).");

            bool hasActiveDelivery = s_dal.Delivery.ReadAll(d =>
                    d.OrderId == orderId &&
                    d.DeliveryFinishType == DO.DeliveryFinishType.None)
                .Any();

            if (hasActiveDelivery)
                throw new BO.BlOrderHasActiveDeliveryException($"Order {orderId} already has an active delivery.");

            var newDelivery = new DO.Delivery(
                DeliveryId: 0,
                OrderId: orderId,
                CourierId: courierId,
                DeliveryMaxDistance: doCourier.MaxCourierDistance,
                DeliveryDate: s_dal.Config.Clock,
                DeliveryFinishDate: s_dal.Config.Clock, // Start time
                ShipmentType: DO.ShipmentType.Standard,
                DeliveryFinishType: DO.DeliveryFinishType.None // Active
            );

            s_dal.Delivery.Create(newDelivery);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Order or courier not found for assignment.", ex);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException("Delivery already exists for this order.", ex);
        }
    }

    #endregion OrderActions

    //==================== List Retrieval & Filtering ===================\\

    #region ListRetrieval

    /// <summary>
    /// Retrieves a list of orders based on filters and sorting options.
    /// Performs complex calculations (Status, Schedule, Timings) for each order.
    /// </summary>
    internal static IEnumerable<BO.OrderInList> GetOrders(
            BO.OrderInListFilterBy? filterField = null,
            object? filterValue = null,
            BO.OrderInListSortBy? sortBy = null)
    {
        try
        {
            var maxRange = s_dal.Config.MaxDelTimeRnge;
            var maxRangeWithoutRisk = maxRange - s_dal.Config.RiskTimeRnge;

            var allOrders = s_dal.Order.ReadAll();
            var allDeliveries = s_dal.Delivery.ReadAll();

            // Join Orders with Deliveries Group
            var query =
                from o in allOrders
                join d in allDeliveries
                    on o.OrderId equals d.OrderId into deliveriesGroup

                let lastDelivery = deliveriesGroup.OrderByDescending(del => del.DeliveryDate).FirstOrDefault()

                let AirDistance = Tools.DistanceKm(o.OrderLatitude, o.OrderLongitude, 31.7479, 35.188)

                let OrderStatus =
                    lastDelivery is null ? BO.OrderStatus.Open :
                    lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.None ? BO.OrderStatus.InProgress :
                    lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.Completed ? BO.OrderStatus.Supplied :
                    lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.Cancelled ? BO.OrderStatus.Canceled :
                    BO.OrderStatus.Refused

                let ScheduleStatus = Tools.CalcScheduleStatus(o.OrderDate, lastDelivery?.DeliveryFinishDate)

                let TimeLeftToFinish =
                    lastDelivery is null || (lastDelivery.DeliveryDate + maxRange) < s_dal.Config.Clock ?
                        TimeSpan.Zero :
                        (lastDelivery.DeliveryDate + maxRange) - s_dal.Config.Clock

                let TotalHandleTime =
                    (from del in deliveriesGroup
                     where del.DeliveryFinishType == DO.DeliveryFinishType.Completed
                     select del.DeliveryFinishDate - o.OrderDate)
                        .Aggregate(TimeSpan.Zero, (acc, span) => acc + span)

                let TotalDeliveries = deliveriesGroup.Count()

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

            var list = query.ToList();

            // Apply Filters
            if (filterField.HasValue && filterValue is not null)
            {
                switch (filterField.Value)
                {
                    case BO.OrderInListFilterBy.OrderId:
                        if (int.TryParse(Convert.ToString(filterValue), out var id))
                            list = list.Where(x => x.OrderId == id).ToList();
                        break;
                    case BO.OrderInListFilterBy.TypeOfOrder:
                        if (Tools.TryConvertEnum(filterValue, out BO.TypeOfOrder typeVal))
                            list = list.Where(x => x.TypeOfOrder == typeVal).ToList();
                        break;
                    case BO.OrderInListFilterBy.OrderStatus:
                        if (Tools.TryConvertEnum(filterValue, out BO.OrderStatus statusVal))
                            list = list.Where(x => x.OrderStatus == statusVal).ToList();
                        break;
                    case BO.OrderInListFilterBy.ScheduleStatus:
                        if (Tools.TryConvertEnum(filterValue, out BO.ScheduleStatus schedVal))
                            list = list.Where(x => x.ScheduleStatus == schedVal).ToList();
                        break;
                }
            }

            // Apply Sorting
            var sorter = sortBy ?? BO.OrderInListSortBy.OrderStatus;
            list = sorter switch
            {
                BO.OrderInListSortBy.OrderId => list.OrderBy(x => x.OrderId).ToList(),
                BO.OrderInListSortBy.TypeOfOrder => list.OrderBy(x => x.TypeOfOrder).ThenBy(x => x.OrderId).ToList(),
                BO.OrderInListSortBy.AirDistance => list.OrderBy(x => x.AirDistance).ThenBy(x => x.OrderId).ToList(),
                BO.OrderInListSortBy.OrderStatus => list.OrderBy(x => x.OrderStatus).ThenBy(x => x.OrderId).ToList(),
                BO.OrderInListSortBy.ScheduleStatus => list.OrderBy(x => x.ScheduleStatus).ThenBy(x => x.OrderId).ToList(),
                BO.OrderInListSortBy.TimeLeftToFinish => list.OrderBy(x => x.TimeLeftToFinish).ThenBy(x => x.OrderId).ToList(),
                BO.OrderInListSortBy.TotalHandleTime => list.OrderBy(x => x.TotalHandleTime).ThenBy(x => x.OrderId).ToList(),
                BO.OrderInListSortBy.TotalDeliveries => list.OrderBy(x => x.TotalDeliveries).ThenBy(x => x.OrderId).ToList(),
                _ => list.OrderBy(x => x.OrderStatus).ThenBy(x => x.OrderId).ToList()
            };

            return list;
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to load orders list (query syntax)", ex);
        }
    }

    /// <summary>
    /// Retrieves all closed deliveries for a specific courier.
    /// </summary>
    internal static IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesByCourier(
        int courierId,
        BO.TypeOfOrder? typeFilter,
        BO.ClosedDeliverySortBy? sortBy)
    {
        Tools.ValidatePersonId(courierId);

        try
        {
            var allClosed = DeliveryManager.BuildClosedDeliveryInList();

            // Get relevant delivery IDs
            var courierDeliveryIds = s_dal.Delivery
                  .ReadAll(d => d.CourierId == courierId && d.DeliveryFinishType != DO.DeliveryFinishType.None)
                  .Select(d => d.DeliveryId)
                  .ToHashSet();

            var query = from d in allClosed
                        where courierDeliveryIds.Contains(d.DeliveryId)
                        select d;

            if (typeFilter.HasValue)
                query = query.Where(d => d.TypeOfOrder == typeFilter.Value);

            var sorter = sortBy ?? BO.ClosedDeliverySortBy.DeliveryFinishType;

            IOrderedEnumerable<BO.ClosedDeliveryInList> ordered = sorter switch
            {
                BO.ClosedDeliverySortBy.DeliveryFinishType => query.OrderBy(d => d.DeliveryFinishType).ThenBy(d => d.OrderId),
                BO.ClosedDeliverySortBy.TotalHandleTime => query.OrderBy(d => d.TotalHandleTime).ThenBy(d => d.OrderId),
                BO.ClosedDeliverySortBy.TypeOfOrder => query.OrderBy(d => d.TypeOfOrder).ThenBy(d => d.OrderId),
                BO.ClosedDeliverySortBy.OrderId => query.OrderBy(d => d.OrderId),
                BO.ClosedDeliverySortBy.ActualDistance => query.OrderBy(d => d.ActualDistance).ThenBy(d => d.OrderId),
                _ => query.OrderBy(d => d.DeliveryFinishType).ThenBy(d => d.OrderId)
            };

            return ordered.ToList();
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to load closed deliveries list for courier.", ex);
        }
    }

    /// <summary>
    /// Retrieves suitable open orders for a specific courier (based on distance/vehicle).
    /// </summary>
    internal static IEnumerable<BO.OpenOrderInList> GetOpenOrdersForCourier(
        int courierId,
        BO.TypeOfOrder? typeFilter,
        BO.OpenOrderSortBy? sortBy)
    {
        Tools.ValidatePersonId(courierId);

        try
        {
            DO.Courier courier = s_dal.Courier.Read(c => c.CourierId == courierId)
                ?? throw new DO.DalDoesNotExistException($"Courier with ID={courierId} does not exist.");
            double? maxDistance = courier.MaxCourierDistance;

            var orderInList = GetOrders();
            var config = AdminManager.GetConfig();

            var query = from o in orderInList
                        where o.OrderStatus == BO.OrderStatus.Open
                        select o;

            if (maxDistance.HasValue)
                query = query.Where(o => o.AirDistance <= maxDistance.Value);

            if (typeFilter.HasValue)
                query = query.Where(o => o.TypeOfOrder == typeFilter.Value);

            var resultQuery =
                from o in query
                let thisCourier = s_dal.Courier.Read(courierId)
                let fullOrder = GetOrder(o.OrderId)
                let courierVehicleType = thisCourier.CourierVehicleType

                let ActualDistance = Tools.GetActualDistanceAsync(
                                                fullOrder.OrderLatitude,
                                                fullOrder.OrderLongitude,
                                                config.Latitude,
                                                config.Longitude,
                                                courierVehicleType)
                                                .GetAwaiter().GetResult()

                let EstimatedActualTime =
                        ActualDistance is null ? (TimeSpan?)null :
                        courierVehicleType switch
                        {
                            DO.CourierVehicleType.Car => TimeSpan.FromHours(ActualDistance.Value / s_dal.Config.AvgCarSpeed),
                            DO.CourierVehicleType.Motorcycle => TimeSpan.FromHours(ActualDistance.Value / s_dal.Config.AvgMotorcycleSpeed),
                            DO.CourierVehicleType.Bicycle => TimeSpan.FromHours(ActualDistance.Value / s_dal.Config.AvgBicycleSpeed),
                            _ => TimeSpan.FromHours(ActualDistance.Value / s_dal.Config.AvgWalkSpeed)
                        }

                select new BO.OpenOrderInList
                {
                    OrderId = o.OrderId,
                    TypeOfOrder = o.TypeOfOrder,
                    OrderWeight = fullOrder.OrderWeight,
                    IsFragile = fullOrder.IsFragile,
                    OrderSize = fullOrder.OrderSize,
                    CustomerAddress = fullOrder.OrderAddress,
                    AirDistance = o.AirDistance,
                    ActualDistance = ActualDistance,
                    EstimatedActualTime = EstimatedActualTime,
                    ScheduleStatus = o.ScheduleStatus,
                    TimeLeftToFinish = o.TimeLeftToFinish,
                    MaxDeliveryTime = fullOrder.MaxDeliveryTime
                };

            var list = resultQuery.ToList();

            var sorter = sortBy ?? BO.OpenOrderSortBy.ScheduleStatus;
            list = sorter switch
            {
                BO.OpenOrderSortBy.TypeOfOrder => list.OrderBy(x => x.TypeOfOrder).ThenBy(x => x.OrderId).ToList(),
                BO.OpenOrderSortBy.AirDistance => list.OrderBy(x => x.AirDistance).ThenBy(x => x.OrderId).ToList(),
                BO.OpenOrderSortBy.ScheduleStatus => list.OrderBy(x => x.ScheduleStatus).ThenBy(x => x.OrderId).ToList(),
                BO.OpenOrderSortBy.OrderId => list.OrderBy(x => x.OrderId).ToList(),
                _ => list.OrderBy(x => x.ScheduleStatus).ThenBy(x => x.OrderId).ToList()
            };

            return list;
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Courier not found while building open orders list.", ex);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to load open orders list.", ex);
        }
    }

    #endregion ListRetrieval

    //==================== Status Summaries & Helpers ===================\\

    #region StatusSummaries

    /// <summary>
    /// Returns a statistical summary of orders grouped by Status and ScheduleStatus.
    /// </summary>
    internal static int[] GetOrderStatusSummary()
    {
        var orders = s_dal.Order.ReadAll();

        int orderStatusCount = Enum.GetValues(typeof(BO.OrderStatus)).Length;
        int scheduleStatusCount = Enum.GetValues(typeof(BO.ScheduleStatus)).Length;

        int[] summary = new int[orderStatusCount * scheduleStatusCount];

        var grouped =
            from o in orders
            let orderStatus = GetOrderStatus(o)
            let scheduleStatus = GetScheduleStatus(o)
            group o by new { orderStatus, scheduleStatus } into g
            select new
            {
                Index = (int)g.Key.orderStatus * scheduleStatusCount + (int)g.Key.scheduleStatus,
                Count = g.Count()
            };

        foreach (var item in grouped)
        {
            summary[item.Index] = item.Count;
        }

        return summary;
    }

    /// <summary>
    /// Calculates the BO.OrderStatus from the DO.Order deliveries history.
    /// </summary>
    internal static BO.OrderStatus GetOrderStatus(DO.Order order)
    {
        var deliveries = s_dal.Delivery.ReadAll(d => d.OrderId == order.OrderId);

        if (!deliveries.Any()) return BO.OrderStatus.Open;

        var lastDelivery = deliveries
            .OrderByDescending(d => d.DeliveryDate)
            .ThenByDescending(d => d.DeliveryId)
            .First();

        if (lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.None)
            return BO.OrderStatus.InProgress;

        return lastDelivery.DeliveryFinishType switch
        {
            DO.DeliveryFinishType.Completed => BO.OrderStatus.Supplied,
            DO.DeliveryFinishType.Cancelled => BO.OrderStatus.Canceled,
            DO.DeliveryFinishType.Failed or DO.DeliveryFinishType.Returned => BO.OrderStatus.Refused,
            _ => throw new BO.BlInvalidDeliveryStatusException($"Unknown delivery finish type: {lastDelivery.DeliveryFinishType}")
        };
    }

    /// <summary>
    /// Calculates the Schedule Status (OnTime, Late, Risk) for an order.
    /// </summary>
    internal static BO.ScheduleStatus GetScheduleStatus(DO.Order order)
    {
        var deliveries = s_dal.Delivery.ReadAll(d => d.OrderId == order.OrderId);

        DateTime? lastFinishDate = deliveries
            .Where(d => d.DeliveryFinishType != DO.DeliveryFinishType.None)
            .OrderByDescending(d => d.DeliveryFinishDate)
            .Select(d => (DateTime?)d.DeliveryFinishDate)
            .FirstOrDefault();

        return Tools.CalcScheduleStatus(order.OrderDate, lastFinishDate);
    }

    #endregion StatusSummaries

    //==================== Helpers & Converters ===================\\

    #region HelpersAndConverters

    /// <summary>
    /// Builds a detailed BO.OrderInProgress object for tracking.
    /// </summary>
    internal static BO.OrderInProgress? BuildOrderInProgress(DO.Order doOrder, DO.Delivery activeDelivery)
    {
        try
        {
            if (activeDelivery is null) return null;

            var thisOrder = GetOrder(doOrder.OrderId);
            var thisCourier = s_dal.Courier.Read(c => c.CourierId == activeDelivery.CourierId)
                ?? throw new DO.DalDoesNotExistException($"Courier with ID={activeDelivery.CourierId} does not exist.");
            var config = AdminManager.GetConfig();

            var expectedDeliveryTime = thisOrder.ExpectedDeliveryTime ?? thisOrder.MaxDeliveryTime;
            double airDistance = Tools.DistanceKm(doOrder.OrderLatitude, doOrder.OrderLongitude, 31.7479, 35.188);

            var maxRange = s_dal.Config.MaxDelTimeRnge;
            var scheduleStatus = Tools.CalcScheduleStatus(doOrder.OrderDate, null);

            TimeSpan timeLeftToFinish =
                (activeDelivery.DeliveryDate + maxRange) < s_dal.Config.Clock ?
                    TimeSpan.Zero :
                (activeDelivery.DeliveryDate + maxRange) - s_dal.Config.Clock;

            var actualDistance = Tools.GetActualDistanceAsync(
                doOrder.OrderLatitude, doOrder.OrderLongitude,
                config.Latitude, config.Longitude,
                thisCourier.CourierVehicleType).GetAwaiter().GetResult();

            return new BO.OrderInProgress
            {
                DeliveryId = activeDelivery.DeliveryId,
                OrderId = doOrder.OrderId,
                TypeOfOrder = (BO.TypeOfOrder)doOrder.TypeOfOrder,
                OrderDetail = thisOrder.OrderDetail,
                CustomerAddress = thisOrder.OrderAddress,
                AirDistance = airDistance,
                ActualDistance = actualDistance,
                CostumerFullName = thisOrder.CustomerFullName,
                CostumerPhone = thisOrder.CustomerPhone,
                OrderOpenTime = thisOrder.OrderOpenTime,
                DeliveryStartTime = activeDelivery.DeliveryDate,
                ExpectedDeliveryTime = expectedDeliveryTime,
                MaxDeliveryTime = thisOrder.MaxDeliveryTime,
                OrderStatus = BO.OrderStatus.InProgress,
                ScheduleStatus = scheduleStatus,
                TimeLeftToFinish = timeLeftToFinish
            };
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Failed to build orders in progress list", ex);
        }
    }

    /// <summary>
    /// Converts a DO.Order to a BO.Order, including calculations and nested lists.
    /// </summary>
    internal static BO.Order ConvertDoToBoOrder(DO.Order doOrder)
    {
        var maxRange = s_dal.Config.MaxDelTimeRnge;

        // Deliveries for this order
        var deliveriesForOrder = from d in s_dal.Delivery.ReadAll()
                                 where d.OrderId == doOrder.OrderId
                                 select d;

        var lastDelivery = deliveriesForOrder.OrderByDescending(del => del.DeliveryDate).FirstOrDefault();

        var courierOfLastDelivery = lastDelivery == null ? null :
            s_dal.Courier.Read(c => c.CourierId == lastDelivery.CourierId);

        // Determine Status
        BO.OrderStatus enumOrderStatus;
        if (lastDelivery == null)
            enumOrderStatus = BO.OrderStatus.Open;
        else if (lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.None)
            enumOrderStatus = BO.OrderStatus.InProgress;
        else
        {
            enumOrderStatus = lastDelivery.DeliveryFinishType switch
            {
                DO.DeliveryFinishType.Completed => BO.OrderStatus.Supplied,
                DO.DeliveryFinishType.Cancelled => BO.OrderStatus.Canceled,
                DO.DeliveryFinishType.Failed or DO.DeliveryFinishType.Returned => BO.OrderStatus.Refused,
                _ => throw new BO.BlInvalidDeliveryStatusException($"Unknown delivery finish type: {lastDelivery.DeliveryFinishType}")
            };
        }

        var calculateAirDistance = Tools.DistanceKm(doOrder.OrderLatitude, doOrder.OrderLongitude, 31.7479, 35.188);
        var maxDelTime = doOrder.OrderDate + s_dal.Config.MaxDelTimeRnge;
        var ScheduleStatus = Tools.CalcScheduleStatus(doOrder.OrderDate, lastDelivery?.DeliveryFinishDate);

        var TimeLeftToFinish = (doOrder.OrderDate + maxRange) < s_dal.Config.Clock ? TimeSpan.Zero :
                               (doOrder.OrderDate + maxRange) - s_dal.Config.Clock;

        DateTime? expectedDeliveryTime = null;
        if (lastDelivery != null && courierOfLastDelivery != null)
        {
            double speed = courierOfLastDelivery.CourierVehicleType switch
            {
                DO.CourierVehicleType.Car => s_dal.Config.AvgCarSpeed,
                DO.CourierVehicleType.Motorcycle => s_dal.Config.AvgMotorcycleSpeed,
                DO.CourierVehicleType.Bicycle => s_dal.Config.AvgBicycleSpeed,
                _ => s_dal.Config.AvgWalkSpeed
            };
            expectedDeliveryTime = lastDelivery.DeliveryDate + TimeSpan.FromHours(calculateAirDistance / speed);
        }

        var delPerOrderInList = DeliveryManager.BuildDeliveryPerOrderInList(doOrder);

        return new BO.Order
        {
            OrderId = doOrder.OrderId,
            OrderStatus = enumOrderStatus,
            OrderDetail = doOrder.OrderDetail,
            OrderAddress = doOrder.OrderAddress,
            OrderLatitude = doOrder.OrderLatitude,
            OrderLongitude = doOrder.OrderLongitude,
            CustomerFullName = doOrder.OrderCustomerFullName,
            CustomerPhone = doOrder.OrderCustomerPhone,
            OrderWeight = doOrder.OrderWeight,
            IsFragile = doOrder.IsFragile,
            OrderSize = doOrder.OrderSize,
            OrderOpenTime = doOrder.OrderDate,
            TypeOfOrder = (BO.TypeOfOrder)doOrder.TypeOfOrder,
            AirDistance = calculateAirDistance,
            MaxDeliveryTime = maxDelTime,
            ExpectedDeliveryTime = expectedDeliveryTime,
            ScheduleStatus = ScheduleStatus,
            TimeRemaining = TimeLeftToFinish,
            DeliveryPerOrderInList = delPerOrderInList
        };
    }

    /// <summary>
    /// Converts a BO.Order to a DO.Order.
    /// </summary>
    internal static DO.Order ConvertBoToDoOrder(BO.Order boOrder) =>
        new DO.Order(
            OrderId: boOrder.OrderId,
            OrderStatus: boOrder.OrderStatus.ToString(),
            OrderDetail: boOrder.OrderDetail,
            OrderAddress: boOrder.OrderAddress,
            OrderLatitude: boOrder.OrderLatitude,
            OrderLongitude: boOrder.OrderLongitude,
            OrderCustomerFullName: boOrder.CustomerFullName,
            OrderCustomerPhone: boOrder.CustomerPhone,
            OrderWeight: boOrder.OrderWeight,
            IsFragile: boOrder.IsFragile,
            OrderSize: boOrder.OrderSize,
            OrderDate: boOrder.OrderOpenTime,
            TypeOfOrder: (DO.TypeOfOrder)boOrder.TypeOfOrder
        );

    //==================== Not in use ===================\\
    internal static List<BO.OpenOrderInList> BuildOpenOrderInlist()
    {
        try
        {
            var allDeliveries = s_dal.Delivery.ReadAll();
            var orderinlist = GetOrders();
            var config = AdminManager.GetConfig();

            var query =
                from o in orderinlist
                where o.OrderStatus == BO.OrderStatus.InProgress
                join d in allDeliveries
                    on o.OrderId equals d.OrderId into deliveriesGroup

                let lastDelivery = deliveriesGroup.OrderByDescending(del => del.DeliveryDate).FirstOrDefault()
                let thisOrder = GetOrder(o.OrderId)
                let courierOfLastDelivery = lastDelivery == null ? null : s_dal.Courier.Read(c => c.CourierId == lastDelivery.CourierId)

                let EstimatedActualTime =
                lastDelivery is null || courierOfLastDelivery is null ? (TimeSpan?)null :
                    courierOfLastDelivery.CourierVehicleType == DO.CourierVehicleType.Car ? TimeSpan.FromHours(o.AirDistance / s_dal.Config.AvgCarSpeed) :
                    courierOfLastDelivery.CourierVehicleType == DO.CourierVehicleType.Motorcycle ? TimeSpan.FromHours(o.AirDistance / s_dal.Config.AvgMotorcycleSpeed) :
                    courierOfLastDelivery.CourierVehicleType == DO.CourierVehicleType.Bicycle ? TimeSpan.FromHours(o.AirDistance / s_dal.Config.AvgBicycleSpeed) :
                    TimeSpan.FromHours(o.AirDistance / s_dal.Config.AvgWalkSpeed)

                select new BO.OpenOrderInList
                {
                    CourierId = lastDelivery.CourierId,
                    OrderId = o.OrderId,
                    TypeOfOrder = o.TypeOfOrder,
                    OrderWeight = thisOrder.OrderWeight,
                    IsFragile = thisOrder.IsFragile,
                    OrderSize = thisOrder.OrderSize,
                    CustomerAddress = thisOrder.OrderAddress,
                    AirDistance = o.AirDistance,
                    ActualDistance = 1,
                    EstimatedActualTime = EstimatedActualTime,
                    ScheduleStatus = o.ScheduleStatus,
                    TimeLeftToFinish = o.TimeLeftToFinish,
                    MaxDeliveryTime = thisOrder.MaxDeliveryTime
                };
            return query.ToList();
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Failed to build open orders in list", ex);
        }
    }

    #endregion HelpersAndConverters

}