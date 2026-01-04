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

    //==================== Observer Manager (Stage 5) ===================\\

    #region ObserverManager

    // Observer manager for order-related updates
    internal static ObserverManager Observers = new(); //stage 5

    #endregion ObserverManager

    //==================== DAL Access ===================\\

    #region DalAccess

    // Access to DAL for data operations
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
        // Validate order data
        Tools.ValidateOrder(order);

        // Convert address to coordinates
        var coords = Tools.GetLocationFromAddress(order.OrderAddress);
        if (coords == null)
            throw new BO.BlInvalidStringException($"Address '{order.OrderAddress}' is invalid.");

        // Set coordinates
        order.OrderLatitude = coords.Value.Lat ?? 0;
        order.OrderLongitude = coords.Value.Lon ?? 0;

        try
        {
            // Create DO.Order and add to DAL
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

        // Notify observers of the new order addition
        Observers.NotifyListUpdated(); //stage 5

    }

    /// <summary>
    /// Retrieves an order by ID.
    /// </summary>
    internal static BO.Order GetOrder(int id)
    {

        // Validate ID
        Tools.ValidateSystemId(id);

        try
        {
            // Fetch DO.Order from DAL
            DO.Order doOrder = s_dal.Order.Read(id)
                ?? throw new BO.BlDoesNotExistException($"Order with ID={id} does not exist");

            // Convert to BO.Order and return
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
        // Validate order data
        Tools.ValidateOrder(order);

        // Fetch existing order to compare address
        var oldOrder = s_dal.Order.Read(order.OrderId);

        // Recalculate coordinates if address changed
        if (oldOrder.OrderAddress != order.OrderAddress)
        {
            // Convert new address to coordinates
            var coords = Tools.GetLocationFromAddress(order.OrderAddress);
            if (coords == null)
                throw new BO.BlInvalidStringException($"New address '{order.OrderAddress}' is invalid.");

            // Set new coordinates
            order.OrderLatitude = coords.Value.Lat ?? 0;
            order.OrderLongitude = coords.Value.Lon ?? 0;
        }
        else
        {
            // Keep existing coordinates
            order.OrderLatitude = oldOrder.OrderLatitude;
            order.OrderLongitude = oldOrder.OrderLongitude;
        }

        try
        {
            // Convert to DO.Order and update in DAL
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

        // Notify observers of the order update
        Observers.NotifyListUpdated(); //stage 5
        Observers.NotifyItemUpdated(order.OrderId); //stage 5
    }

    /// <summary>
    /// Deletes an order if it has no active delivery.
    /// </summary>
    internal static void DeleteOrder(int id)
    {
        // Validate ID
        Tools.ValidateSystemId(id);

        try
        {
            // Fetch order to ensure it exists
            DO.Order? doOrder = s_dal.Order.Read(id)
                ?? throw new BO.BlDoesNotExistException($"Order with ID={id} does not exist");

            // Check for active delivery
            bool hasActiveDelivery = s_dal.Delivery
                .ReadAll(d => d.OrderId == id)
                .Any(d => d.DeliveryFinishType == DO.DeliveryFinishType.None);

            // Prevent deletion if active delivery exists
            if (hasActiveDelivery)
                throw new BO.BlOrderHasActiveDeliveryException($"Cannot delete order {id}: courier is on way with delivery.");

            // Proceed to delete order
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

        // Notify observers of the order update
        Observers.NotifyListUpdated(); //stage 5
        Observers.NotifyItemUpdated(id); //stage 5
    }

    #endregion BoCrudMethods

    //==================== Order Actions (State Change) ===================\\

    #region OrderActions

    /// <summary>
    /// Cancels an order. If open, creates a cancelled delivery record. If in progress, updates the active delivery.
    /// </summary>
    internal static void CancelOrder(int orderId)
    {
        // Validate ID
        Tools.ValidateSystemId(orderId);

        try
        {
            // Fetch order to determine current status
            var doOrder = s_dal.Order.Read(orderId)
                ?? throw new BO.BlDoesNotExistException($"Order with ID={orderId} does not exist.");

            // Convert to BO.Order for status check
            var boOrder = ConvertDoToBoOrder(doOrder);

            // Handle cancellation based on current status
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

                // Mark delivery as cancelled
                s_dal.Delivery.Update(
                activeDelivery with
                {
                    DeliveryFinishDate = s_dal.Config.Clock,
                    DeliveryFinishType = DO.DeliveryFinishType.Cancelled
                });
            }
            else
            {
                // Cannot cancel if already canceled or finished
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

        // Notify observers of the order update
        Observers.NotifyListUpdated(); //stage 5
        Observers.NotifyItemUpdated(orderId); //stage 5
    }

    /// <summary>
    /// Marks a delivery as completed.
    /// </summary>
    internal static void CompleteOrderHandling(int courierId, int deliveryId)
    {
        // Validate IDs
        Tools.ValidatePersonId(courierId);
        Tools.ValidateSystemId(deliveryId);

        try
        {
            // Fetch delivery to verify existence and assignment
            var delivery = s_dal.Delivery.Read(deliveryId)
                ?? throw new BO.BlDoesNotExistException($"Delivery with ID={deliveryId} does not exist.");

            // Ensure the courier is assigned to this delivery
            if (delivery.CourierId != courierId)
                throw new BO.BlCourierNotAssignedToDeliveryException(
                    $"Courier with ID={courierId} is not assigned to delivery ID={deliveryId}.");

            // Mark delivery as completed
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

        // Notify observers of the order update
        Observers.NotifyListUpdated(); //stage 5
        Observers.NotifyItemUpdated(deliveryId); //stage 5
    }

    /// <summary>
    /// Assigns an open order to a courier by creating a new active Delivery.
    /// </summary>
    internal static void AssignOrderToCourier(int courierId, int orderId)
    {
        // Validate IDs
        Tools.ValidatePersonId(courierId);
        Tools.ValidateSystemId(orderId);

        try
        {
            // Fetch order and courier
            DO.Order doOrder = s_dal.Order.Read(o => o.OrderId == orderId)
                ?? throw new DO.DalDoesNotExistException($"Order with ID={orderId} does not exist.");

            // Fetch courier
            DO.Courier doCourier = s_dal.Courier.Read(c => c.CourierId == courierId)
                ?? throw new DO.DalDoesNotExistException($"Courier with ID={courierId} does not exist.");

            // Ensure courier is enabled
            if (!doCourier.CourierEnabled)
                throw new BO.BlCourierDisabledException($"Courier {courierId} is disabled and cannot take orders.");

            // Ensure order is open
            BO.OrderStatus orderStatus = GetOrderStatus(doOrder);
            if (orderStatus != BO.OrderStatus.Open)
                throw new BO.BlOrderNotOpenForAssignmentException(
                    $"Order {orderId} is not open for assignment (current status: {orderStatus}).");

            // Ensure no active delivery already exists for this order
            bool hasActiveDelivery = s_dal.Delivery.ReadAll(d =>
                    d.OrderId == orderId &&
                    d.DeliveryFinishType == DO.DeliveryFinishType.None)
                .Any();

            // Prevent duplicate active delivery
            if (hasActiveDelivery)
                throw new BO.BlOrderHasActiveDeliveryException($"Order {orderId} already has an active delivery.");

            // Create new active delivery
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

            // Add delivery to DAL
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

        // Notify observers of the order update
        Observers.NotifyListUpdated(); //stage 5
        Observers.NotifyItemUpdated(orderId); //stage 5
        
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
            // Pre-calculate commonly used values
            var maxRange = s_dal.Config.MaxDelTimeRnge;
            var maxRangeWithoutRisk = maxRange - s_dal.Config.RiskTimeRnge;

            var allOrders = s_dal.Order.ReadAll();
            var allDeliveries = s_dal.Delivery.ReadAll();

            // Join Orders with Deliveries Group
            var query =
                from o in allOrders
                join d in allDeliveries
                    on o.OrderId equals d.OrderId into deliveriesGroup

                // Get last delivery for status calculations
                let lastDelivery = deliveriesGroup.OrderByDescending(del => del.DeliveryDate).FirstOrDefault()

                // Calculate Air Distance from central point (31.7479, 35.188)
                let AirDistance = Tools.DistanceKm(o.OrderLatitude, o.OrderLongitude, 31.7479, 35.188)

                // Determine Order Status based on last delivery
                let OrderStatus =
                    lastDelivery is null ? BO.OrderStatus.Open :
                    lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.None ? BO.OrderStatus.InProgress :
                    lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.Completed ? BO.OrderStatus.Supplied :
                    lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.Cancelled ? BO.OrderStatus.Canceled :
                    BO.OrderStatus.Refused

                // Calculate Schedule Status
                let ScheduleStatus = Tools.CalcScheduleStatus(o.OrderDate, lastDelivery?.DeliveryFinishDate)

                // Calculate Time Left to Finish
                let TimeLeftToFinish =
                    ((o.OrderDate + maxRange) < s_dal.Config.Clock) ?
                        TimeSpan.Zero :
                    (o.OrderDate + maxRange) - s_dal.Config.Clock

                // Calculate Total Handle Time from completed deliveries
                let TotalHandleTime =
                    (from del in deliveriesGroup
                     where del.DeliveryFinishType == DO.DeliveryFinishType.Completed
                     select del.DeliveryFinishDate - o.OrderDate)
                        .Aggregate(TimeSpan.Zero, (acc, span) => acc + span)

                // Calculate Total Deliveries count
                let TotalDeliveries = deliveriesGroup.Count()

                // Project to BO.OrderInList
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

            // Materialize query to list for further processing
            var list = query.ToList();

            // Apply Filters
            if (filterField.HasValue && filterValue is not null)
            {
                switch (filterField.Value)
                {
                    
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
        // Validate courier ID
        Tools.ValidatePersonId(courierId);

        try
        {
            // Get all closed deliveries
            var allClosed = DeliveryManager.BuildClosedDeliveryInList();

            // Get relevant delivery IDs
            var courierDeliveryIds = s_dal.Delivery
                  .ReadAll(d => d.CourierId == courierId && d.DeliveryFinishType != DO.DeliveryFinishType.None)
                  .Select(d => d.DeliveryId)
                  .ToHashSet();

            // Filter deliveries for the specific courier
            var query = from d in allClosed
                        where courierDeliveryIds.Contains(d.DeliveryId)
                        select d;

            // Apply type filter if provided
            if (typeFilter.HasValue)
                query = query.Where(d => d.TypeOfOrder == typeFilter.Value);

            // Apply sorting
            var sorter = sortBy ?? BO.ClosedDeliverySortBy.DeliveryFinishType;

            // Order the results based on the specified sorter
            IOrderedEnumerable<BO.ClosedDeliveryInList> ordered = sorter switch
            {
                BO.ClosedDeliverySortBy.DeliveryFinishType => query.OrderBy(d => d.DeliveryFinishType).ThenBy(d => d.OrderId),
                BO.ClosedDeliverySortBy.TotalHandleTime => query.OrderBy(d => d.TotalHandleTime).ThenBy(d => d.OrderId),
                BO.ClosedDeliverySortBy.TypeOfOrder => query.OrderBy(d => d.TypeOfOrder).ThenBy(d => d.OrderId),
                BO.ClosedDeliverySortBy.OrderId => query.OrderBy(d => d.OrderId),
                BO.ClosedDeliverySortBy.ActualDistance => query.OrderBy(d => d.ActualDistance).ThenBy(d => d.OrderId),
                _ => query.OrderBy(d => d.DeliveryFinishType).ThenBy(d => d.OrderId)
            };

            // Return the ordered list
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
        // Validate courier ID
        Tools.ValidatePersonId(courierId);

        try
        {
            // Fetch courier to get max distance
            DO.Courier courier = s_dal.Courier.Read(c => c.CourierId == courierId)
                ?? throw new DO.DalDoesNotExistException($"Courier with ID={courierId} does not exist.");

            // Get courier's max distance
            double? maxDistance = courier.MaxCourierDistance;

            // Get all open orders
            var orderInList = GetOrders();
            var config = AdminManager.GetConfig();

            // Filter open orders
            var query = from o in orderInList
                        where o.OrderStatus == BO.OrderStatus.Open
                        select o;

            // Apply max distance filter if applicable
            if (maxDistance.HasValue)
                query = query.Where(o => o.AirDistance <= maxDistance.Value);

            // Apply type filter if provided
            if (typeFilter.HasValue)
                query = query.Where(o => o.TypeOfOrder == typeFilter.Value);

            // Project to OpenOrderInList with additional calculations
            var resultQuery =
                from o in query
                let thisCourier = s_dal.Courier.Read(courierId)
                let fullOrder = GetOrder(o.OrderId)
                let courierVehicleType = thisCourier.CourierVehicleType

                // Calculate Actual Distance
                let ActualDistance = Tools.GetActualDistanceAsync(
                                                fullOrder.OrderLatitude,
                                                fullOrder.OrderLongitude,
                                                config.Latitude,
                                                config.Longitude,
                                                courierVehicleType)
                                                .GetAwaiter().GetResult()

                // Estimate Actual Time based on vehicle type
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

            // Apply sorting
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
        // Fetch all orders
        var orders = s_dal.Order.ReadAll();

        // Prepare summary array
        int orderStatusCount = Enum.GetValues(typeof(BO.OrderStatus)).Length;
        int scheduleStatusCount = Enum.GetValues(typeof(BO.ScheduleStatus)).Length;

        // Initialize summary array
        int[] summary = new int[orderStatusCount * scheduleStatusCount];

        // Group orders by status combinations and count
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

        // Populate summary array
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
        // Fetch all deliveries for the order
        var deliveries = s_dal.Delivery.ReadAll(d => d.OrderId == order.OrderId);

        // Determine status based on last delivery
        if (!deliveries.Any()) return BO.OrderStatus.Open;

        // Get the last delivery
        var lastDelivery = deliveries
            .OrderByDescending(d => d.DeliveryDate)
            .ThenByDescending(d => d.DeliveryId)
            .First();

        // Determine status from last delivery finish type
        if (lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.None)
            return BO.OrderStatus.InProgress;

        // Map finish types to order statuses
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
        // Fetch all deliveries for the order
        var deliveries = s_dal.Delivery.ReadAll(d => d.OrderId == order.OrderId);

        // Get the last finish date from deliveries
        DateTime? lastFinishDate = deliveries
            .Where(d => d.DeliveryFinishType != DO.DeliveryFinishType.None)
            .OrderByDescending(d => d.DeliveryFinishDate)
            .Select(d => (DateTime?)d.DeliveryFinishDate)
            .FirstOrDefault();

        // Calculate and return schedule status
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
            // Ensure active delivery exists
            if (activeDelivery is null) return null;

            // Fetch related order and courier
            var thisOrder = GetOrder(doOrder.OrderId);
            var thisCourier = s_dal.Courier.Read(c => c.CourierId == activeDelivery.CourierId)
                ?? throw new DO.DalDoesNotExistException($"Courier with ID={activeDelivery.CourierId} does not exist.");
            var config = AdminManager.GetConfig();

            // Calculate expected delivery time
            var expectedDeliveryTime = thisOrder.ExpectedDeliveryTime ?? thisOrder.MaxDeliveryTime;
            double airDistance = Tools.DistanceKm(doOrder.OrderLatitude, doOrder.OrderLongitude, 31.7479, 35.188);

            // Calculate max range and schedule status
            var maxRange = s_dal.Config.MaxDelTimeRnge;
            var scheduleStatus = Tools.CalcScheduleStatus(doOrder.OrderDate, null);

            // Calculate time left to finish
            TimeSpan timeLeftToFinish =
                (activeDelivery.DeliveryDate + maxRange) < s_dal.Config.Clock ?
                    TimeSpan.Zero :
                (activeDelivery.DeliveryDate + maxRange) - s_dal.Config.Clock;

            // Calculate actual distance
            var actualDistance = Tools.GetActualDistanceAsync(
                doOrder.OrderLatitude, doOrder.OrderLongitude,
                config.Latitude, config.Longitude,
                thisCourier.CourierVehicleType).GetAwaiter().GetResult();

            // Build and return OrderInProgress object
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
        // Pre-calculate commonly used values
        var maxRange = s_dal.Config.MaxDelTimeRnge;

        // Deliveries for this order
        var deliveriesForOrder = from d in s_dal.Delivery.ReadAll()
                                 where d.OrderId == doOrder.OrderId
                                 select d;

        // Get last delivery
        var lastDelivery = deliveriesForOrder.OrderByDescending(del => del.DeliveryDate).FirstOrDefault();

        // Get courier of last delivery
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
            // Map finish types to order statuses
            enumOrderStatus = lastDelivery.DeliveryFinishType switch
            {
                DO.DeliveryFinishType.Completed => BO.OrderStatus.Supplied,
                DO.DeliveryFinishType.Cancelled => BO.OrderStatus.Canceled,
                DO.DeliveryFinishType.Failed or DO.DeliveryFinishType.Returned => BO.OrderStatus.Refused,
                _ => throw new BO.BlInvalidDeliveryStatusException($"Unknown delivery finish type: {lastDelivery.DeliveryFinishType}")
            };
        }

        // Calculate Air Distance from central point (31.7479, 35.188)
        var calculateAirDistance = Tools.DistanceKm(doOrder.OrderLatitude, doOrder.OrderLongitude, 31.7479, 35.188);
        var maxDelTime = doOrder.OrderDate + s_dal.Config.MaxDelTimeRnge;
        var ScheduleStatus = Tools.CalcScheduleStatus(doOrder.OrderDate, lastDelivery?.DeliveryFinishDate);

        // Calculate Time Left to Finish
        var TimeLeftToFinish = (doOrder.OrderDate + maxRange) < s_dal.Config.Clock ? TimeSpan.Zero :
                               (doOrder.OrderDate + maxRange) - s_dal.Config.Clock;

        // Calculate Expected Delivery Time
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

        // Build DeliveryPerOrderInList
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