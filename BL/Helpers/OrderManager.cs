namespace Helpers;

using DalApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

internal static class OrderManager
{
    //======== Data Access Layer Instance ========\\

    private static IDal s_dal = Factory.Get;

    // ============ BO CRUD Methods ======== \\

    #region BO CRUD Methods

    internal static void AddOrder(BO.Order order) // create order
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

    internal static BO.Order GetOrder(int id) // read Order by id
    {

        Tools.ValidateSystemId(id);

        try
        {
            // Read the Order from the data access layer
            DO.Order doOrder = s_dal.Order.Read(id)
                ?? throw new BO.BlDoesNotExistException($"Order with ID={id} does not exist");

            // Build and return the business object representation of the Order
            return ConvertDoToBoOrder(doOrder);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            // Wrap and rethrow any exceptions that occur during the read operation
            throw new BO.BlDoesNotExistException("Failed to get order", ex);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            // Wrap and rethrow any exceptions that occur during the read operation
            throw new BO.BlXMLFileLoadCreateException("Failed to get order", ex);
        }
    }

    internal static void UpdateOrder(BO.Order order) // update order
    {

        Tools.ValidateOrder(order);

        var oldOrder = s_dal.Order.Read(order.OrderId);

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
            // Map the business object order to a data object order
            DO.Order doOrder = ConvertBoToDoOrder(order);

            s_dal.Order.Update(doOrder);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            // Wrap and rethrow any exceptions that occur during the save operation
            throw new BO.BlXMLFileLoadCreateException("Failed to update order", ex);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            // Wrap and rethrow any exceptions that occur during the save operation
            throw new BO.BlDoesNotExistException("Failed to update order", ex);
        }
    }

    internal static void DeleteOrder(int id)
    {

        Tools.ValidateSystemId(id);

        try
        {
            // Check that order exists
            DO.Order? doOrder = s_dal.Order.Read(id)
                ?? throw new BO.BlDoesNotExistException($"Order with ID={id} does not exist");

            // Check if order has an active delivery
            bool hasActiveDelivery = s_dal.Delivery
                .ReadAll(d => d.OrderId == id)
                .Any(d => d.DeliveryFinishType == DO.DeliveryFinishType.None);

            if (hasActiveDelivery)
                throw new BO.BlOrderHasActiveDeliveryException($"Cannot delete order {id}: courier is on way with delivery.");

            // Perform deletion
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

    #endregion BO CRUD Methods

    // ============ Order Action Methods ======== \\

    #region Order Action Methods

    // =========== Order State Change Methods ======== \\
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
                var activeDeliveries = s_dal.Delivery
                    .ReadAll(d => d.OrderId == orderId && d.DeliveryFinishType == DO.DeliveryFinishType.None).Single();

                s_dal.Delivery.Update(
                activeDeliveries with
                {
                    DeliveryFinishDate = s_dal.Config.Clock,
                    DeliveryFinishType = DO.DeliveryFinishType.Cancelled
                });
            }

            else throw new BO.BlOrderAlreadyCanceledException($"Order {orderId} is already canceled.");
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
    /// Assigns an open order to a courier by creating a new DO.Delivery.
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

            bool hasActiveDelivery =
                s_dal.Delivery.ReadAll(d =>
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
                DeliveryFinishDate: s_dal.Config.Clock,
                ShipmentType: DO.ShipmentType.Standard,
                DeliveryFinishType: DO.DeliveryFinishType.None
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

    // ============ List Retrieval Methods ======== \\

    #region List Retrieval Methods

    internal static IEnumerable<BO.OrderInList> GetOrders(
            BO.OrderInListFilterBy? filterField = null,
            object? filterValue = null,
            BO.OrderInListSortBy? sortBy = null) // read all Orders - query syntax + filter/sort
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
                    lastDelivery is null ?
                        BO.OrderStatus.Open :
                    lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.None ?
                        BO.OrderStatus.InProgress :
                    lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.Completed ?
                        BO.OrderStatus.Supplied :
                    lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.Cancelled ?
                        BO.OrderStatus.Canceled :
                    BO.OrderStatus.Refused

                let ScheduleStatus =
                    Tools.CalcScheduleStatus(o.OrderDate, lastDelivery?.DeliveryFinishDate)

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

            // materialize
            var list = query.ToList();

            // Filtering:
            // If filterField is provided AND filterValue is not null, apply equality filter.
            if (filterField.HasValue && filterValue is not null)
            {
                switch (filterField.Value)
                {
                    case BO.OrderInListFilterBy.OrderId:
                        if (int.TryParse(Convert.ToString(filterValue), out var id))
                            list = list.Where(x => x.OrderId == id).ToList();
                        break;

                    case BO.OrderInListFilterBy.TypeOfOrder:
                        {
                            if (Tools.TryConvertEnum(filterValue, out BO.TypeOfOrder typeVal))
                                list = list.Where(x => x.TypeOfOrder == typeVal).ToList();
                        }
                        break;

                    case BO.OrderInListFilterBy.OrderStatus:
                        {
                            if (Tools.TryConvertEnum(filterValue, out BO.OrderStatus statusVal))
                                list = list.Where(x => x.OrderStatus == statusVal).ToList();
                        }
                        break;

                    case BO.OrderInListFilterBy.ScheduleStatus:
                        {
                            if (Tools.TryConvertEnum(filterValue, out BO.ScheduleStatus schedVal))
                                list = list.Where(x => x.ScheduleStatus == schedVal).ToList();
                        }
                        break;

                    default:
                        break;
                }
            }

            // Sorting:
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
    /// Returns all closed deliveries of a given courier, optionally filtered by TypeOfOrder
    /// and sorted according to ClosedDeliverySortBy.
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

            var courierDeliveryIds =
                s_dal.Delivery
                     .ReadAll(d =>
                         d.CourierId == courierId &&
                         d.DeliveryFinishType != DO.DeliveryFinishType.None)
                     .Select(d => d.DeliveryId)
                     .ToHashSet();

            var query =
                from d in allClosed
                where courierDeliveryIds.Contains(d.DeliveryId)
                select d;

            if (typeFilter.HasValue)
                query = query.Where(d => d.TypeOfOrder == typeFilter.Value);

            var sorter = sortBy ?? BO.ClosedDeliverySortBy.DeliveryFinishType;

            IOrderedEnumerable<BO.ClosedDeliveryInList> ordered = sorter switch
            {
                BO.ClosedDeliverySortBy.DeliveryFinishType =>
                    query.OrderBy(d => d.DeliveryFinishType).ThenBy(d => d.OrderId),

                BO.ClosedDeliverySortBy.TotalHandleTime =>
                    query.OrderBy(d => d.TotalHandleTime).ThenBy(d => d.OrderId),

                BO.ClosedDeliverySortBy.TypeOfOrder =>
                    query.OrderBy(d => d.TypeOfOrder).ThenBy(d => d.OrderId),

                BO.ClosedDeliverySortBy.OrderId =>
                    query.OrderBy(d => d.OrderId),

                BO.ClosedDeliverySortBy.ActualDistance =>
                    query.OrderBy(d => d.ActualDistance).ThenBy(d => d.OrderId),

                _ =>
                    query.OrderBy(d => d.DeliveryFinishType).ThenBy(d => d.OrderId)
            };

            return ordered.ToList();
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to load closed deliveries list for courier.", ex);
        }
    }

    /// <summary>
    /// Returns all open orders that match the courier (by max air distance),
    /// optionally filtered by TypeOfOrder and sorted according to OpenOrderSortBy.
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

            var query =
                from o in orderInList
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
                        ActualDistance is null ?
                            (TimeSpan?)null :
                        courierVehicleType switch
                        {
                            DO.CourierVehicleType.Car =>
                                TimeSpan.FromHours(ActualDistance.Value / s_dal.Config.AvgCarSpeed),
                            DO.CourierVehicleType.Motorcycle =>
                                TimeSpan.FromHours(ActualDistance.Value / s_dal.Config.AvgMotorcycleSpeed),
                            DO.CourierVehicleType.Bicycle =>
                                TimeSpan.FromHours(ActualDistance.Value / s_dal.Config.AvgBicycleSpeed),
                            _ =>
                                TimeSpan.FromHours(ActualDistance.Value / s_dal.Config.AvgWalkSpeed)
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
                BO.OpenOrderSortBy.TypeOfOrder =>
                    list.OrderBy(x => x.TypeOfOrder).ThenBy(x => x.OrderId).ToList(),

                BO.OpenOrderSortBy.AirDistance =>
                    list.OrderBy(x => x.AirDistance).ThenBy(x => x.OrderId).ToList(),

                BO.OpenOrderSortBy.ScheduleStatus =>
                    list.OrderBy(x => x.ScheduleStatus).ThenBy(x => x.OrderId).ToList(),

                BO.OpenOrderSortBy.OrderId =>
                    list.OrderBy(x => x.OrderId).ToList(),

                _ =>
                    list.OrderBy(x => x.ScheduleStatus).ThenBy(x => x.OrderId).ToList()
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

    #endregion List Retrieval Methods

    // ============ Status Summary Methods ============ \\

    #region Status Summary Methods

    /// <summary>
    /// Returns an array of counts per combined (OrderStatus, ScheduleStatus).
    /// In index i sits the number of orders whose combined status equals
    /// the i-th status according to the numeric encoding.
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
                Index =
                    (int)g.Key.orderStatus * scheduleStatusCount +
                    (int)g.Key.scheduleStatus,
                Count = g.Count()
            };

        foreach (var item in grouped)
        {
            summary[item.Index] = item.Count;
        }

        return summary;
    }

    /// <summary>
    /// Calculates the logical OrderStatus for a DO.Order
    /// based on its deliveries.
    /// </summary>
    internal static BO.OrderStatus GetOrderStatus(DO.Order order)
    {

        var deliveries = s_dal.Delivery.ReadAll(d => d.OrderId == order.OrderId);

        if (!deliveries.Any())
            return BO.OrderStatus.Open;

        var lastDelivery = deliveries
            .OrderByDescending(d => d.DeliveryDate)
            .ThenByDescending(d => d.DeliveryId)
            .First();

        if (lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.None)
            return BO.OrderStatus.InProgress;

        switch (lastDelivery.DeliveryFinishType)
        {
            case DO.DeliveryFinishType.Completed:
                return BO.OrderStatus.Supplied;

            case DO.DeliveryFinishType.Cancelled:
                return BO.OrderStatus.Canceled;

            case DO.DeliveryFinishType.Failed:
            case DO.DeliveryFinishType.Returned:
            default:
                return BO.OrderStatus.Refused;
        }
    }

    /// <summary>
    /// Calculates ScheduleStatus (OnTime / InRisk / Late)
    /// for an order according to elapsed time.
    /// </summary>
    internal static BO.ScheduleStatus GetScheduleStatus(DO.Order order)
    {
        var deliveries = s_dal.Delivery.ReadAll(d => d.OrderId == order.OrderId);

        DateTime? lastFinishDate =
            deliveries
                .Where(d => d.DeliveryFinishType != DO.DeliveryFinishType.None)
                .OrderByDescending(d => d.DeliveryFinishDate)
                .Select(d => (DateTime?)d.DeliveryFinishDate)
                .FirstOrDefault();

        return Tools.CalcScheduleStatus(
            order.OrderDate,
            lastFinishDate);
    }

    #endregion Status Summary Methods

    #endregion Order Action Methods

    //  ============ Helper Out Methods ======== \\

    #region Helper Out Methods

    internal static BO.OrderInProgress? BuildOrderInProgress(DO.Order doOrder, DO.Delivery activeDelivery)
    {
        try
        {
            if (activeDelivery is null) return null;

            var thisOrder = GetOrder(doOrder.OrderId);

            var thisCurier = s_dal.Courier.Read(c => c.CourierId == activeDelivery.CourierId)
                ?? throw new DO.DalDoesNotExistException($"Courier with ID={activeDelivery.CourierId} does not exist.");
            var config = AdminManager.GetConfig();

            var expectedDeliveryTime = thisOrder.ExpectedDeliveryTime ?? thisOrder.MaxDeliveryTime;

            double airDistance = Tools.DistanceKm(doOrder.OrderLatitude, doOrder.OrderLongitude, 31.7479, 35.188);

            var maxRange = s_dal.Config.MaxDelTimeRnge;
            var maxRangeWithoutRisk = maxRange - s_dal.Config.RiskTimeRnge;

            var scheduleStatus = Tools.CalcScheduleStatus(doOrder.OrderDate, null);

            TimeSpan timeLeftToFinish =
                (activeDelivery.DeliveryDate + maxRange) < s_dal.Config.Clock ?
                    TimeSpan.Zero :
                (activeDelivery.DeliveryDate + maxRange) - s_dal.Config.Clock;


            var actualDistance = Tools.GetActualDistanceAsync(
                doOrder.OrderLatitude,
                doOrder.OrderLongitude,
                config.Latitude,
                config.Longitude,
                thisCurier.CourierVehicleType).GetAwaiter().GetResult();


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

    #endregion Helper Out Methods

    //======== Conversion Methods ========\\

    #region Private Conversion Methods

    internal static BO.Order ConvertDoToBoOrder(DO.Order doOrder) // build BO.Order from DO.Order
    {
        // Maximum allowed delivery time range from configuration
        var maxRange = s_dal.Config.MaxDelTimeRnge;
        var maxRangeWithoutRisk = maxRange - s_dal.Config.RiskTimeRnge;

        // All deliveries for THIS order only 
        var deliveriesForOrder =
            from d in s_dal.Delivery.ReadAll()
            where d.OrderId == doOrder.OrderId
            select d;

        // Last delivery by DeliveryDate 
        var lastDelivery = deliveriesForOrder
            .OrderByDescending(del => del.DeliveryDate)
            .FirstOrDefault();

        // Courier of last delivery 
        var courierOfLastDelivery =
            lastDelivery == null ?
                null :
            s_dal.Courier.Read(c => c.CourierId == lastDelivery.CourierId);

        // Determine OrderStatus based on last delivery
        BO.OrderStatus enumOrderStatus;

        // No deliveries yet
        if (lastDelivery == null)
            enumOrderStatus = BO.OrderStatus.Open;

        // Last delivery is still in progress
        else if (lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.None)
            enumOrderStatus = BO.OrderStatus.InProgress;

        // Last delivery is finished
        else
        {
            switch (lastDelivery.DeliveryFinishType)
            {
                case DO.DeliveryFinishType.Completed:
                    enumOrderStatus = BO.OrderStatus.Supplied;
                    break;
                case DO.DeliveryFinishType.Cancelled:
                    enumOrderStatus = BO.OrderStatus.Canceled;
                    break;
                case DO.DeliveryFinishType.Failed:
                case DO.DeliveryFinishType.Returned:
                    enumOrderStatus = BO.OrderStatus.Refused;
                    break;
                default:
                    throw new BO.BlInvalidDeliveryStatusException($"Unknown delivery finish type: " +
                        $"{lastDelivery.DeliveryFinishType} for Delivery ID: {lastDelivery.DeliveryId}");
            }
        }

        var calculateAirDistance = Tools.DistanceKm(doOrder.OrderLatitude, doOrder.OrderLongitude, 31.7479, 35.188);

        var maxDelTime = doOrder.OrderDate + s_dal.Config.MaxDelTimeRnge;

        var ScheduleStatus = Tools.CalcScheduleStatus(doOrder.OrderDate, lastDelivery?.DeliveryFinishDate);

        var TimeLeftToFinish =
                lastDelivery is null || (lastDelivery.DeliveryDate + maxRange) < s_dal.Config.Clock ?
                    TimeSpan.Zero :
                (lastDelivery.DeliveryDate + maxRange) - s_dal.Config.Clock;

        DateTime? expectedDeliveryTime = null;

        if (lastDelivery != null && courierOfLastDelivery != null)
        {
            double speed = courierOfLastDelivery.CourierVehicleType switch
            {
                DO.CourierVehicleType.Car => s_dal.Config.AvgCarSpeed,
                DO.CourierVehicleType.Motorcycle => s_dal.Config.AvgMotorcycleSpeed,
                DO.CourierVehicleType.Bicycle => s_dal.Config.AvgBicycleSpeed,
                DO.CourierVehicleType.Foot => s_dal.Config.AvgWalkSpeed,
                _ => s_dal.Config.AvgWalkSpeed
            };

            expectedDeliveryTime =
                lastDelivery.DeliveryDate + TimeSpan.FromHours(calculateAirDistance / speed);
        }

        // Build the order in progress using the DeliveryManager
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

    #endregion Private Conversion Methods

    //=========== Not in use ===========\\
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

                let ExpectedDeliveryTime = thisOrder.ExpectedDeliveryTime ?? thisOrder.MaxDeliveryTime

                let courierOfLastDelivery =
                    lastDelivery == null ?
                        null :
                    s_dal.Courier.Read(c => c.CourierId == lastDelivery.CourierId)

                let EstimatedActualTime =
                lastDelivery is null || courierOfLastDelivery is null ?
                        (TimeSpan?)null :
                    courierOfLastDelivery.CourierVehicleType == DO.CourierVehicleType.Car ?
                        TimeSpan.FromHours(o.AirDistance / s_dal.Config.AvgCarSpeed) :
                    courierOfLastDelivery.CourierVehicleType == DO.CourierVehicleType.Motorcycle ?
                        TimeSpan.FromHours(o.AirDistance / s_dal.Config.AvgMotorcycleSpeed) :
                    courierOfLastDelivery.CourierVehicleType == DO.CourierVehicleType.Bicycle ?
                        TimeSpan.FromHours(o.AirDistance / s_dal.Config.AvgBicycleSpeed) :
                    TimeSpan.FromHours(o.AirDistance / s_dal.Config.AvgWalkSpeed)

                // let ActualDistance = Tools.GetActualDistanceAsync(
                //     thisOrder.OrderLatitude,
                //     thisOrder.OrderLongitude,
                //     config.Latitude,
                //     config.Longitude,
                //(DO.CourierVehicleType)thisCurier.VehicleType).GetAwaiter().GetResult()

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

}
