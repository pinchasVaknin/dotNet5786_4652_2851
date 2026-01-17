namespace Helpers;

using BlApi;
using BO;
using DalApi;
using DO;
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
    private static IDal s_dal = DalApi.Factory.Get;

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
                .Any(d => d.DeliveryFinishType == null);

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

        // Variable to hold active delivery for notification
        int? courierIdToNotify = null;

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
                    ActualDistance = null,
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
                    .ReadAll(d => d.OrderId == orderId && d.DeliveryFinishType == null)
                    .Single();

                // Mark delivery as cancelled
                s_dal.Delivery.Update(
                activeDelivery with
                {
                    DeliveryFinishDate = s_dal.Config.Clock,
                    DeliveryFinishType = DO.DeliveryFinishType.Cancelled
                });

                // Set courier ID for notification
                courierIdToNotify = activeDelivery.CourierId;
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

        if (courierIdToNotify.HasValue)
        {
            CourierManager.Observers.NotifyItemUpdated(courierIdToNotify.Value);
            CourierManager.Observers.NotifyListUpdated();
        }
    }

    /// <summary>
    /// Marks a delivery as completed.
    /// </summary>
    internal static void CompleteOrderHandling(int courierId, int deliveryId, BO.DeliveryFinishType finishType)
    {
        // Validate IDs
        Tools.ValidatePersonId(courierId);
        Tools.ValidateSystemId(deliveryId);

        // Variable to hold order ID for notification
        int orderId;

        try
        {
            // Fetch delivery to verify existence and assignment
            var delivery = s_dal.Delivery.Read(deliveryId)
                ?? throw new BO.BlDoesNotExistException($"Delivery with ID={deliveryId} does not exist.");

            // Ensure the courier is assigned to this delivery
            if (delivery.CourierId != courierId)
                throw new BO.BlCourierNotAssignedToDeliveryException(
                    $"Courier with ID={courierId} is not assigned to delivery ID={deliveryId}.");

            // Get order ID for notification
            orderId = delivery.OrderId;

            // Mark delivery as completed
            s_dal.Delivery.Update(
                delivery with
                {
                    DeliveryFinishDate = s_dal.Config.Clock,
                    DeliveryFinishType = (DO.DeliveryFinishType)finishType
                });
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Failed to complete order handling", ex);
        }

        // Notify observers of the order update
        Observers.NotifyListUpdated();
        Observers.NotifyItemUpdated(orderId);

        CourierManager.Observers.NotifyListUpdated();
        CourierManager.Observers.NotifyItemUpdated(courierId);
    }

    /// <summary>
    /// Assigns an open order to a courier by creating a new active Delivery.
    /// </summary>
    internal static void AssignOrderToCourier(int courierId, int orderId, double? actualDistance)
    {
        // Validate IDs
        Tools.ValidatePersonId(courierId);
        Tools.ValidateSystemId(orderId);

        try
        {
            // Fetch order and courier to verify existence
            DO.Order doOrder = s_dal.Order.Read(o => o.OrderId == orderId)
                ?? throw new DO.DalDoesNotExistException($"Order with ID={orderId} does not exist.");

            // Fetch courier
            DO.Courier doCourier = s_dal.Courier.Read(c => c.CourierId == courierId)
                ?? throw new DO.DalDoesNotExistException($"Courier with ID={courierId} does not exist.");

            // Ensure courier is enabled
            if (!doCourier.CourierEnabled)
                throw new BO.BlCourierDisabledException($"Courier {courierId} is disabled.");

            // Check order status
            BO.OrderStatus orderStatus = GetOrderStatus(doOrder);

            // Ensure order is open for assignment
            if (orderStatus != BO.OrderStatus.Open)
                throw new BO.BlOrderNotOpenForAssignmentException(
                    $"Order {orderId} cannot be assigned. Current status: {orderStatus}");

            // Check for existing active delivery
            bool hasActiveDelivery = s_dal.Delivery.ReadAll(d => d.OrderId == orderId &&
                                                                 d.DeliveryFinishType == null).Any();

            // Prevent duplicate active delivery
            if (hasActiveDelivery)
                throw new BO.BlOrderHasActiveDeliveryException($"Order {orderId} already has an active delivery.");

            // Calculate actual distance if not provided
            if (actualDistance is null || actualDistance == 0)
            {
                // Calculate actual distance using external service
                bool missingCoords = doOrder.OrderLatitude == null || doOrder.OrderLongitude == null ||
                                     s_dal.Config.Latitude == null || s_dal.Config.Longitude == null;

                // If coordinates are missing, set distance to 0
                if (missingCoords)
                    actualDistance = 0;
                else
                {
                    // Attempt to get actual distance
                    try
                    {
                        // Call external service to get actual distance
                        actualDistance = Tools.GetActualDistanceAsync(
                                                s_dal.Config.Latitude,      // From Company
                                                s_dal.Config.Longitude,
                                                doOrder.OrderLatitude,      // To Customer
                                                doOrder.OrderLongitude,
                                                doCourier.CourierVehicleType)
                                                .GetAwaiter().GetResult();
                    }
                    catch
                    {
                        // Fallback: estimate distance as 1.5x air distance
                        double airDist = Tools.DistanceKm(s_dal.Config.Latitude ?? 0, s_dal.Config.Longitude ?? 0,
                                                          doOrder.OrderLatitude, doOrder.OrderLongitude);

                        // Estimate actual distance
                        actualDistance = airDist * 1.5;
                    }
                }
            }
            // Ensure we don't pass null to the database
            if (actualDistance == null) actualDistance = 0;

            // Create new Delivery record
            var newDelivery = new DO.Delivery(
                DeliveryId: 0,
                OrderId: orderId,
                CourierId: courierId,
                ActualDistance: actualDistance,
                DeliveryDate: s_dal.Config.Clock, // Start time based on simulation clock
                DeliveryFinishDate: null,
                ShipmentType: DO.ShipmentType.Standard,
                DeliveryFinishType: null
            );

            // Save to DAL
            s_dal.Delivery.Create(newDelivery);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Order or courier not found.", ex);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException("Delivery already exists.", ex);
        }

        // Notify observers of the order update
        Observers.NotifyListUpdated();
        Observers.NotifyItemUpdated(orderId);
        // Notify observers of the courier update
        CourierManager.Observers.NotifyListUpdated();
        CourierManager.Observers.NotifyItemUpdated(courierId);
    }

    #endregion OrderActions

    //==================== List Retrieval & Filtering ===================\\

    #region ListRetrieval

    /// <summary>
    /// Retrieves a list of orders based on filters and sorting options.
    /// Performs complex calculations (Status, Schedule, Timings) for each order.
    /// </summary>
    internal static IEnumerable<BO.OrderInList> GetOrders(
            BO.OrderInListFilterBy? filterBy = null,
            object? filterValue = null,
            BO.OrderInListSortBy? sortBy = null)
    {
        try
        {
            // Build filter predicate if applicable (Erlier filtering attempt)
            Func<DO.Order, bool>? dalFilter = null;

            // Example for TypeOfOrder filter
            if (filterBy == BO.OrderInListFilterBy.TypeOfOrder && filterValue is not null)
            {
                if (Tools.TryConvertEnum(filterValue, out BO.TypeOfOrder typeVal))
                    dalFilter = o => (BO.TypeOfOrder)o.TypeOfOrder == typeVal;
            }

            // Pre-calculate commonly used values
            var maxRange = s_dal.Config.MaxDelTimeRnge;
            var maxRangeWithoutRisk = maxRange - s_dal.Config.RiskTimeRnge;

            var allOrders = s_dal.Order.ReadAll(dalFilter);
            var allDeliveries = s_dal.Delivery.ReadAll();

            var baseLatitude = s_dal.Config.Latitude ?? 31.7479;
            var baseLongitude = s_dal.Config.Longitude ?? 35.188;

            // Join Orders with Deliveries Group
            var query =
                from o in allOrders
                join d in allDeliveries
                    on o.OrderId equals d.OrderId into deliveriesGroup

                // Get last delivery for status calculations
                let lastDelivery = deliveriesGroup.OrderByDescending(del => del.DeliveryDate).FirstOrDefault()

                // Determine Order Status based on last delivery
                let OrderStatus =
                    lastDelivery is null ? BO.OrderStatus.Open :
                    lastDelivery.DeliveryFinishType == null ? BO.OrderStatus.InProgress :
                    lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.Completed ? BO.OrderStatus.Supplied :
                    lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.Cancelled ? BO.OrderStatus.Cancelled :
                    BO.OrderStatus.Refused

                // Calculate Schedule Status
                let ScheduleStatus = Tools.CalcScheduleStatus(o.OrderDate, lastDelivery?.DeliveryFinishDate)

                // TypeOfOrder filter
                let filterStatusExclude = filterBy == BO.OrderInListFilterBy.OrderStatus && filterValue is not null &&
                                 Tools.TryConvertEnum(filterValue, out BO.OrderStatus statusVal) &&
                                 OrderStatus != statusVal
                where !filterStatusExclude

                // ScheduleStatus filter
                let filterScheduleExclude = filterBy == BO.OrderInListFilterBy.ScheduleStatus && filterValue is not null &&
                                 Tools.TryConvertEnum(filterValue, out BO.ScheduleStatus schedVal) &&
                                 ScheduleStatus != schedVal
                where !filterScheduleExclude

                // Calculate Air Distance from base to order location
                let AirDistance = Tools.DistanceKm(o.OrderLatitude, o.OrderLongitude, baseLatitude, baseLongitude)

                // Calculate max allowed time
                let maxTime = o.OrderDate + maxRange

                // Get current clock
                let clock = s_dal.Config.Clock

                // Calculate Time Left to Finish
                let TimeLeftToFinish =
                (OrderStatus == BO.OrderStatus.Supplied || OrderStatus == BO.OrderStatus.Cancelled || OrderStatus == BO.OrderStatus.Refused) ?
                    TimeSpan.Zero :
                (maxTime <= clock ? TimeSpan.Zero : maxTime - clock)

                // Calculate Total Handle Time from completed deliveries
                let TotalHandleTime =
                    (from del in deliveriesGroup
                     where del.DeliveryFinishType == DO.DeliveryFinishType.Completed
                     select (del.DeliveryFinishDate ?? o.OrderDate) - o.OrderDate)
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
                  .ReadAll(d => d.CourierId == courierId && d.DeliveryFinishType != null)
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

        // Fetch courier details
        DO.Courier courier = s_dal.Courier.Read(c => c.CourierId == courierId)
            ?? throw new DO.DalDoesNotExistException($"Courier {courierId} not found");

        // Get courier constraints
        double? maxDistance = courier.MaxCourierDistance;
        var vehicle = courier.CourierVehicleType;

        // Base location (company headquarters)
        var baseLat = s_dal.Config.Latitude ?? 0;
        var baseLon = s_dal.Config.Longitude ?? 0;

        // Current simulation clock
        DateTime systemClock = s_dal.Config.Clock;

        try
        {
            // Fetch all orders
            var allOrders = s_dal.Order.ReadAll();

            // Get IDs of orders already taken (in progress or completed)
            var unavailableOrderIds =
                    s_dal.Delivery.ReadAll(d => d.DeliveryFinishDate == null ||                             // In progress
                                                d.DeliveryFinishType == null ||                             // In progress safety
                                                d.DeliveryFinishType == DO.DeliveryFinishType.Completed ||  // Completed
                                                d.DeliveryFinishType == DO.DeliveryFinishType.Cancelled ||  // Cancelled
                                                d.DeliveryFinishType == DO.DeliveryFinishType.Returned)     // Returned
                                                .Select(d => d.OrderId)
                                                .ToHashSet();

            // Query for suitable open orders
            var query = from doOrder in allOrders
                        where !unavailableOrderIds.Contains(doOrder.OrderId)
                        where typeFilter == null || (BO.TypeOfOrder)doOrder.TypeOfOrder == typeFilter

                        // Vehicle suitability checks
                        let isFragile = doOrder.IsFragile
                        let isTV = (BO.TypeOfOrder)doOrder.TypeOfOrder == BO.TypeOfOrder.TV
                        where (vehicle == DO.CourierVehicleType.Car) ||
                              (vehicle == DO.CourierVehicleType.Motorcycle && !isTV) ||
                              (vehicle != DO.CourierVehicleType.Car && vehicle != DO.CourierVehicleType.Motorcycle && !isTV && !isFragile)

                        // Distance check
                        let airDist = Tools.DistanceKm(doOrder.OrderLatitude, doOrder.OrderLongitude, baseLat, baseLon)
                        where (!maxDistance.HasValue || airDist <= maxDistance.Value)

                        // Calculate deadline
                        let deadline = doOrder.OrderDate + s_dal.Config.MaxDelTimeRnge

                        let missingCoords = doOrder.OrderLatitude == null || doOrder.OrderLongitude == null ||
                                            s_dal.Config.Latitude == null || s_dal.Config.Longitude == null

                        let actualDistance = Tools.GetActualDistanceAsync(baseLat, baseLon,
                                                                          doOrder.OrderLatitude,
                                                                          doOrder.OrderLongitude,
                                                                          vehicle)
                                                                          .GetAwaiter().GetResult()


                        // Project to BO.OpenOrderInList
                        select new BO.OpenOrderInList
                        {
                            OrderId = doOrder.OrderId,
                            TypeOfOrder = (BO.TypeOfOrder)doOrder.TypeOfOrder,
                            OrderWeight = doOrder.OrderWeight,
                            IsFragile = doOrder.IsFragile,
                            OrderSize = doOrder.OrderSize,
                            CustomerAddress = doOrder.OrderAddress,
                            AirDistance = airDist,
                            ActualDistance = actualDistance,
                            EstimatedActualTime = null,
                            ScheduleStatus = Tools.CalcScheduleStatus(doOrder.OrderDate, null), // No deliveries yet
                            TimeLeftToFinish = (deadline <= systemClock) ? TimeSpan.Zero : (deadline - systemClock), // Time left
                            MaxDeliveryTime = deadline
                        };

            // Materialize query to list
            var list = query.ToList();

            // Apply sorting if specified
            if (sortBy != null)
            {
                // Order the results based on the specified sorter
                list = sortBy switch
                {
                    BO.OpenOrderSortBy.TypeOfOrder => list.OrderBy(x => x.TypeOfOrder).ThenBy(x => x.OrderId).ToList(),
                    BO.OpenOrderSortBy.AirDistance => list.OrderBy(x => x.AirDistance).ThenBy(x => x.OrderId).ToList(),
                    BO.OpenOrderSortBy.ScheduleStatus => list.OrderBy(x => x.ScheduleStatus).ThenBy(x => x.OrderId).ToList(),
                    BO.OpenOrderSortBy.OrderId => list.OrderBy(x => x.OrderId).ToList(),
                    _ => list.OrderBy(x => x.ScheduleStatus).ThenBy(x => x.OrderId).ToList()
                };
            }
            // Return the final list
            return list;
        }
        catch (Exception ex)
        {
            throw new BO.BlDoesNotExistException("Error calculating open orders.", ex);
        }
    }

    /// <summary>
    /// Retrieves the delivery history for a specific courier and order.
    /// </summary>
    /// <param name="requesterId"> ID of the user requesting the history.</param>
    /// <param name="courierId"> ID of the courier whose history is requested.</param>
    /// <param name="orderId"> ID of the order for which history is requested.</param>
    /// <returns> List of delivery history entries.</returns>
    /// <exception cref="BO.BlAdminPermissionException"> Thrown if requester lacks permission.</exception>
    internal static IEnumerable<BO.DeliveryPerOrderInList> GetDeliveryHistoryForCourier(
        int requesterId, int courierId, int orderId)
    {
        // Validate IDs
        Tools.ValidatePersonId(courierId);
        Tools.ValidateSystemId(orderId);

        // Admin check
        var config = AdminManager.GetConfig();
        bool isAdmin = (requesterId == config.AdminId);

        // Only Admin or the same courier can ask
        if (!isAdmin && requesterId != courierId)
            throw new BO.BlAdminPermissionException(
                $"User {requesterId} is not authorized to perform action 'GetDeliveryHistoryForCourier'.");

        // If not admin -> courier must be related to this order
        if (!isAdmin)
        {
            bool related = s_dal.Delivery
                .ReadAll(d => d.OrderId == orderId && d.CourierId == courierId)
                .Any();

            if (!related)
                throw new BO.BlAdminPermissionException("Courier is not allowed to view this order history.");
        }

        var deliveries = s_dal.Delivery
            .ReadAll(d => d.OrderId == orderId)
            .OrderByDescending(d => d.DeliveryDate);

        var query =
            from d in deliveries
            let doCourier = (d.CourierId == 0) ? null : s_dal.Courier.Read(d.CourierId)
            select new BO.DeliveryPerOrderInList
            {
                DeliveryId = d.DeliveryId,
                CourierId = (d.CourierId == 0) ? (int?)null : d.CourierId,
                CourierFullName = doCourier?.CourierFullName ?? "System",
                ShipmentType = (BO.ShipmentType)d.ShipmentType,
                StartDeliveryDate = d.DeliveryDate,

                // Nullable fields
                DeliveryFinishType = (d.DeliveryFinishType == null)
                    ? (BO.DeliveryFinishType?)null
                    : (BO.DeliveryFinishType)d.DeliveryFinishType,

                FinishDeliveryTime = (d.DeliveryFinishType == null)
                    ? (DateTime?)null
                    : d.DeliveryFinishDate
            };

        return query.ToList();
    }

    #endregion ListRetrieval

    //==================== Status Summaries & Helpers ===================\\

    #region StatusSummaries

    /// <summary>
    /// Returns a statistical summary of orders grouped by Status and ScheduleStatus.
    /// </summary>
    internal static int[] GetOrderStatusSummary()
    {
        // Fetch all orders and deliveries
        var orders = s_dal.Order.ReadAll();
        var allDeliveries = s_dal.Delivery.ReadAll();

        // Group deliveries by OrderId for quick lookup
        var deliveriesMap = allDeliveries
            .GroupBy(d => d.OrderId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Prepare summary array
        int orderStatusCount = Enum.GetValues(typeof(BO.OrderStatus)).Length;
        int scheduleStatusCount = Enum.GetValues(typeof(BO.ScheduleStatus)).Length;
        int[] summary = new int[orderStatusCount * scheduleStatusCount];

        // Group orders by calculated statuses
        var grouped =
            from o in orders

            // Get deliveries for this order
            let orderDeliveries = deliveriesMap.ContainsKey(o.OrderId) ? deliveriesMap[o.OrderId] : new List<DO.Delivery>()

            // Calculate statuses
            let orderStatus = GetOrderStatus(o, orderDeliveries)
            let scheduleStatus = GetScheduleStatus(o, orderDeliveries)

            // Group by combined key
            group o by new { orderStatus, scheduleStatus } into g
            select new
            {
                Index = (int)g.Key.orderStatus * scheduleStatusCount + (int)g.Key.scheduleStatus,
                Count = g.Count()
            };

        // Populate summary array
        foreach (var item in grouped)
        {
            if (item.Index >= 0 && item.Index < summary.Length)
                summary[item.Index] = item.Count;
        }
        // Return the summary array
        return summary;
    }

    /// <summary>
    /// Calculates the Order Status.
    /// </summary>
    /// <param name="order"> The order to evaluate.</param>
    /// <param name="deliveries"> Optional pre-fetched deliveries for optimization.</param>
    /// <returns> The calculated Order Status.</returns>
    /// <exception cref="BO.BlInvalidDeliveryStatusException"> Thrown for unknown delivery finish types.</exception>
    internal static BO.OrderStatus GetOrderStatus(DO.Order order, IEnumerable<DO.Delivery>? deliveries = null)
    {
        // If a list was provided use it, otherwise fetch from DAL for this specific order
        var orderDeliveries = deliveries ?? s_dal.Delivery.ReadAll(d => d.OrderId == order.OrderId);

        // If there are no deliveries, the order is Open
        if (!orderDeliveries.Any()) return BO.OrderStatus.Open;

        // Get the most recent delivery
        var lastDelivery = orderDeliveries
            .OrderByDescending(d => d.DeliveryDate)
            .ThenByDescending(d => d.DeliveryId)
            .First();

        // If the delivery has no finish type, it is still in progress
        if (lastDelivery.DeliveryFinishType == null)
            return BO.OrderStatus.InProgress;

        // Map the delivery finish type to the order status
        return lastDelivery.DeliveryFinishType switch
        {
            DO.DeliveryFinishType.Completed => BO.OrderStatus.Supplied,
            DO.DeliveryFinishType.Cancelled => BO.OrderStatus.Cancelled,
            DO.DeliveryFinishType.Returned => BO.OrderStatus.Refused,
            DO.DeliveryFinishType.Failed => BO.OrderStatus.Open, // Failed returns to Open
            _ => throw new BO.BlInvalidDeliveryStatusException($"Unknown delivery finish type")
        };
    }

    /// <summary>
    /// Calculates the Schedule Status.
    /// </summary>
    /// <param name="order"> The order to evaluate.</param>
    /// <param name="deliveries"> Optional pre-fetched deliveries for optimization.</param>
    /// <returns> The calculated Schedule Status.</returns>
    internal static BO.ScheduleStatus GetScheduleStatus(DO.Order order, IEnumerable<DO.Delivery>? deliveries = null)
    {
        // If a list was provided use it, otherwise fetch from DAL for this specific order
        var orderDeliveries = deliveries ?? s_dal.Delivery.ReadAll(d => d.OrderId == order.OrderId);

        // Get the current status using the resolved list
        BO.OrderStatus currentStatus = GetOrderStatus(order, orderDeliveries);

        // If the order is still open, calculate based on order date only
        if (currentStatus == BO.OrderStatus.Open)
        {
            return Tools.CalcScheduleStatus(order.OrderDate, null);
        }

        // Find the last actual finish date
        DateTime? lastFinishDate = orderDeliveries
            .Where(d => d.DeliveryFinishType != null)
            .OrderByDescending(d => d.DeliveryFinishDate)
            .Select(d => (DateTime?)d.DeliveryFinishDate)
            .FirstOrDefault();

        // Calculate schedule status based on the finish date
        return Tools.CalcScheduleStatus(order.OrderDate, lastFinishDate);
    }

    #endregion StatusSummaries

    //==================== Product Catalog (for Price/Weight/Size) ===================\\

    #region ProductCatalog

    /// <summary>
    /// Product information record.
    /// </summary>
    /// <param name="Price">The price of the product.</param>
    /// <param name="Weight">The weight of the product.</param>
    /// <param name="Size">The size of the product.</param>
    /// <param name="IsFragile">Indicates if the product is fragile.</param>
    private record ProductInfo(double Price, double Weight, double Size, bool IsFragile);

    /// <summary>
    /// In-memory product catalog with predefined products.
    /// </summary>
    private static readonly Dictionary<string, ProductInfo> _catalog = new()
    {
        // ============== Smartphones ============== \\
        { "iPhone_14",   new(3500, 0.18, 0.6,  true) },
        { "Galaxy_S23",  new(3200, 0.17, 0.6,  true) },
        { "Pixel_8",     new(2800, 0.19, 0.6,  true) },
        { "Xiaomi_13",   new(2200, 0.19, 0.6,  true) },

        // ================== Laptops ================== \\
        { "Dell_XPS_13",        new(6500, 1.25, 6.5, true) },
        { "MacBook_Air_M2",     new(5800, 1.24, 6.2, true) },
        { "HP_Spectre_x360",    new(6200, 1.35, 6.8, true) },
        { "Lenovo_ThinkPad_X1", new(7000, 1.12, 6.3, true) },

        // ================ Tablets ================ \\
        { "iPad_Air",       new(2600, 0.46, 2.5, true) },
        { "Galaxy_Tab_S9",  new(3000, 0.50, 2.7, true) },
        { "Xiaomi_Pad_6",   new(1900, 0.49, 2.7, true) },

        // ====================== TVs ====================== \\
        { "LG_OLED_C3_55",       new(5500, 18.0, 110.0, true) },
        { "Samsung_QLED_Q80_65", new(6000, 24.0, 160.0, true) },
        { "Sony_Bravia_50",      new(4500, 12.0,  90.0, true) },

        // ================ Cameras ================ \\
        { "Canon_EOS_R10", new(3200, 0.55, 2.0, true) },
        { "Sony_a6400",    new(3400, 0.52, 2.0, true) },
        { "Nikon_Z50",     new(3600, 0.58, 2.1, true) },

        // =================== Audio =================== \\
        { "Sony_WH_1000XM5", new(1400, 0.25, 3.0, false) },
        { "AirPods_Pro_2",   new(950,  0.06, 0.4, false) },
        { "Bose_QC45",       new(1200, 0.24, 3.0, false) },
        { "JBL_Flip_6",      new(550,  0.55, 2.2, false) },

        // ================== SmartHome ================== \\
        { "Google_Nest_Hub",     new(450, 0.48, 2.5, true) },
        { "Amazon_Echo",         new(380, 0.95, 4.5, false) },
        { "Philips_Hue_Starter", new(520, 1.20, 5.5, false) },

        // ================= Gaming Consoles ================= \\
        { "PlayStation_5",        new(2200, 4.50, 28.0, false) },
        { "Xbox_Series_X",        new(2100, 4.45, 25.0, false) },
        { "Nintendo_Switch_OLED", new(1600, 0.42,  3.5, false) },

        // ================= Accessories ================= \\
        { "USB_C_Cable_100W",  new(50,  0.10, 0.3, false) },
        { "GaN_Charger_65W",   new(150, 0.20, 0.6, false) },
        { "NVMe_SSD_1TB",      new(320, 0.03, 0.2, false) },
        { "HDMI_4K_2_1_Cable", new(70,  0.15, 0.5, false) },
    };

    // ==================== Public Logic Methods ==================== \\

    /// <summary>
    /// Gets the product price from the catalog based on model name.
    /// </summary>
    /// <param name="modelName">The model name of the product.</param>
    /// <returns>The price of the product, or 0 if not found.</returns>
    internal static double GetProductPrice(string modelName)
    {
        // Lookup product in catalog
        if (_catalog.TryGetValue(modelName, out var info))
        {
            return info.Price;
        }
        return 0;
    }

    /// <summary>
    /// Updates order details (weight, size, fragility, description) based on the provided items.
    /// </summary>
    /// <param name="order">The order object to update.</param>
    /// <param name="items">A collection of tuples containing product model names and their quantities.</param>
    internal static void UpdateOrderDetails(BO.Order order, IEnumerable<(string Model, int Quantity)> items)
    {
        // Handle null items
        if (items == null)
            items = Enumerable.Empty<(string Model, int Quantity)>();

        // Filter valid items
        var list = items.Where(i => !string.IsNullOrWhiteSpace(i.Model) && i.Quantity > 0).ToList();

        // Handle empty list
        if (list.Count == 0)
        {
            order.OrderDetail = "";
            order.OrderWeight = 0;
            order.OrderSize = 0;
            order.IsFragile = false;
            return;
        }

        // Calculate totals
        double totalWeight = 0;
        double totalSize = 0;
        bool isPackageFragile = false;

        // Build order detail string
        List<string> detailsBuilder = new();

        // Iterate through items
        foreach (var (nameRaw, qty) in list)
        {
            string name = nameRaw.Trim();

            // Lookup product info
            if (!_catalog.TryGetValue(name, out var info))
                info = new ProductInfo(0, 1.0, 1.0, false);

            totalWeight += info.Weight * qty;
            totalSize += info.Size * qty;

            // Check fragility
            if (info.IsFragile)
                isPackageFragile = true;

            detailsBuilder.Add($"{name}{{{qty}}}");
        }

        // Update order properties
        order.OrderWeight = totalWeight;
        order.OrderSize = totalSize;
        order.IsFragile = isPackageFragile;

        // Set order detail string
        order.OrderDetail = $"{order.TypeOfOrder} => {string.Join(", ", detailsBuilder)}";
    }

    #endregion ProductCatalog

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

            // Build and return OrderInProgress object
            return new BO.OrderInProgress
            {
                DeliveryId = activeDelivery.DeliveryId,
                OrderId = doOrder.OrderId,
                TypeOfOrder = (BO.TypeOfOrder)doOrder.TypeOfOrder,
                OrderDetail = thisOrder.OrderDetail,
                CustomerAddress = thisOrder.OrderAddress,
                AirDistance = airDistance,
                ActualDistance = activeDelivery.ActualDistance,
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
        else if (lastDelivery.DeliveryFinishType == null)
            enumOrderStatus = BO.OrderStatus.InProgress;
        else
        {
            // Map finish types to order statuses
            enumOrderStatus = lastDelivery.DeliveryFinishType switch
            {
                DO.DeliveryFinishType.Completed => BO.OrderStatus.Supplied,
                DO.DeliveryFinishType.Cancelled => BO.OrderStatus.Cancelled,
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

    #endregion HelpersAndConverters

    //==================== Periodic Updates ===================\\

    #region PeriodicUpdates

    /// <summary>
    /// Performs periodic updates on active orders to sync their statuses based on delivery history.
    /// </summary>
    /// <param name="oldClock">The current clock time.</param>
    /// <param name="newClock">The previous clock time.</param>
    /// <exception cref="BO.BlXMLFileLoadCreateException">Thrown when there is an error during the update process.</exception>
    internal static void PeriodicOrdersUpdates(DateTime oldClock, DateTime newClock)
    {
        try
        {
            // Fetch all active orders (Open or InProgress)
            var activeOrders = s_dal.Order.ReadAll(o => o.OrderStatus == BO.OrderStatus.Open.ToString() ||
                                                        o.OrderStatus == BO.OrderStatus.InProgress.ToString());

            // Update each active order
            foreach (var order in activeOrders)
            {
                SyncOrderStatus(order, newClock);
            }

            // Notify observers about the updates
            Observers.NotifyListUpdated();
        }
        catch (Exception ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to perform periodic order updates", ex);
        }
    }

    /// <summary>
    /// Synchronizes the status of a single order based on its delivery history.
    /// </summary>
    /// <param name="order">The order to synchronize.</param>
    /// <param name="newClock">The new clock time.</param>
    /// <exception cref="BO.BlInvalidDeliveryStatusException">Thrown when an unknown delivery finish type is encountered.</exception>
    private static void SyncOrderStatus(DO.Order order, DateTime newClock)
    {
        // Fetch all deliveries for the order
        var deliveries = s_dal.Delivery.ReadAll(d => d.OrderId == order.OrderId);

        // Get the last delivery
        var lastDelivery = deliveries.OrderByDescending(d => d.DeliveryDate).FirstOrDefault();

        // Determine new status based on last delivery
        BO.OrderStatus newStatus;

        // Determine status based on last delivery
        if (lastDelivery == null)
            newStatus = BO.OrderStatus.Open;
        else if (lastDelivery.DeliveryFinishType == null)
            newStatus = BO.OrderStatus.InProgress;
        else
        {
            // Map finish types to order statuses
            newStatus = lastDelivery.DeliveryFinishType switch
            {
                DO.DeliveryFinishType.Completed => BO.OrderStatus.Supplied,
                DO.DeliveryFinishType.Cancelled => BO.OrderStatus.Cancelled,
                DO.DeliveryFinishType.Failed or DO.DeliveryFinishType.Returned => BO.OrderStatus.Refused,
                _ => throw new BO.BlInvalidDeliveryStatusException($"Unknown delivery finish type: {lastDelivery.DeliveryFinishType}")
            };
        }

        // Convert new status to DAL string
        var dalNewStatus = newStatus.ToString();

        // Update order if status has changed
        if (order.OrderStatus != dalNewStatus)
        {
            order = order with { OrderStatus = dalNewStatus };
            s_dal.Order.Update(order);

            Observers.NotifyItemUpdated(order.OrderId);
        }
    }

    #endregion PeriodicUpdates

}
