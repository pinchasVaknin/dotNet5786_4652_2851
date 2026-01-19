namespace Helpers;

using BlApi;
using BO;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    internal static async Task AddOrder(BO.Order order)
    {
        // Validate input
        Tools.ValidateOrder(order);

        // Network call outside lock
        var coords = await Tools.GetLocationFromAddressAsync(order.OrderAddress);

        // Validate address
        if (coords == null)
            throw new BO.BlInvalidStringException($"Address '{order.OrderAddress}' is invalid.");

        // Extract coordinates
        double lat = coords.Value.Lat;
        double lon = coords.Value.Lon;

        try
        {
            // Build DO object (do not mutate the input BO if possible)
            DO.Order doOrder = ConvertBoToDoOrder(order);

            // Set coordinates on DO (adjust property names to your DO.Order)
            doOrder = doOrder with
            {
                OrderLatitude = lat,
                OrderLongitude = lon
            };

            // DAL write must be under lock
            lock (AdminManager.BlMutex)
            {
                s_dal.Order.Create(doOrder);
            }
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException("Failed to add order", ex);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to add order", ex);
        }

        // Notify observers outside lock
        Observers.NotifyListUpdated();
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
            // Local variable for DO.Order
            DO.Order doOrder;

            lock (AdminManager.BlMutex) //stage 7
            {
                // Fetch DO.Order from DAL
                doOrder = s_dal.Order.Read(id) ??
                    throw new BO.BlDoesNotExistException($"Order with ID={id} does not exist");

                // Convert to BO.Order and return
                return ConvertDoToBoOrder(doOrder);
            }
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
    internal static async Task UpdateOrder(BO.Order order)
    {
        // Validate input
        Tools.ValidateOrder(order);

        // Fetch existing order to compare address
        DO.Order oldOrder;
        lock (AdminManager.BlMutex)
        {
            oldOrder = s_dal.Order.Read(order.OrderId)
                ?? throw new DO.DalDoesNotExistException($"Order {order.OrderId} not found");
        }

        // Prepare new coordinates
        double newLat = oldOrder.OrderLatitude;
        double newLon = oldOrder.OrderLongitude;

        if (oldOrder.OrderAddress != order.OrderAddress)
        {
            var coords = await Tools.GetLocationFromAddressAsync(order.OrderAddress);

            // Validate address
            if (coords == null)
                throw new BO.BlInvalidStringException($"Address '{order.OrderAddress}' is invalid.");

            // Update coordinates
            newLat = coords.Value.Lat;
            newLon = coords.Value.Lon;
        }

        try
        {
            // Build DO object from BO
            DO.Order doOrder = ConvertBoToDoOrder(order);

            // Update coordinates if changed
            doOrder = doOrder with
            {
                OrderLatitude = newLat,
                OrderLongitude = newLon
            };

            lock (AdminManager.BlMutex)
            {
                s_dal.Order.Update(doOrder);
            }
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to update order", ex);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Failed to update order", ex);
        }

        // Notify observers outside lock
        Observers.NotifyListUpdated();
        Observers.NotifyItemUpdated(order.OrderId);
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
            lock (AdminManager.BlMutex)
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
            lock (AdminManager.BlMutex)
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
        Observers.NotifyListUpdated();
        Observers.NotifyItemUpdated(orderId);

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
            lock (AdminManager.BlMutex)
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
    internal static async Task AssignOrderToCourier(int courierId, int orderId, double? actualDistance)
    {
        // Validate IDs
        Tools.ValidatePersonId(courierId);
        Tools.ValidateSystemId(orderId);

        // Local variables
        DO.Order doOrder;
        DO.Courier doCourier;
        DateTime systemClock;
        double? companyLat;
        double? companyLon;
        TimeSpan maxDelTime;
        bool hasActiveDelivery;

        lock (AdminManager.BlMutex)
        {
            // Fetch order and courier
            doOrder = s_dal.Order.Read(o => o.OrderId == orderId)
                ?? throw new DO.DalDoesNotExistException($"Order with ID={orderId} does not exist.");

            doCourier = s_dal.Courier.Read(c => c.CourierId == courierId)
                ?? throw new DO.DalDoesNotExistException($"Courier with ID={courierId} does not exist.");

            systemClock = s_dal.Config.Clock;
            companyLat = s_dal.Config.Latitude;
            companyLon = s_dal.Config.Longitude;
            maxDelTime = s_dal.Config.MaxDelTimeRnge;

            // Active delivery = not finished yet
            hasActiveDelivery = s_dal.Delivery
                .ReadAll(d => d.OrderId == orderId && d.DeliveryFinishDate == null)
                .Any();
        }

        // Validations (outside lock)
        if (!doCourier.CourierEnabled)
            throw new BO.BlCourierDisabledException($"Courier {courierId} is disabled.");

        BO.OrderStatus orderStatus = GetOrderStatus(doOrder);
        if (orderStatus != BO.OrderStatus.Open)
            throw new BO.BlOrderNotOpenForAssignmentException(
                $"Order {orderId} cannot be assigned. Current status: {orderStatus}");

        if (hasActiveDelivery)
            throw new BO.BlOrderHasActiveDeliveryException($"Order {orderId} already has an active delivery.");

        // Check if order is within max delivery time range
        bool missingCoords =
            !companyLat.HasValue || !companyLon.HasValue;

        if (missingCoords)
            throw new BO.BlInvalidStringException(
                "Cannot assign order: missing coordinates (company or order address is not resolved).");


        if (actualDistance == null)
        {
            // Try external OSRM service
            actualDistance = await Tools.GetActualDistanceAsync(
                            companyLat, companyLon,
                            doOrder.OrderLatitude, doOrder.OrderLongitude,
                            doCourier.CourierVehicleType)
                            .ConfigureAwait(false);

            // Fallback if OSRM fails
            if (actualDistance == null)
            {
                // Fallback calculation
                double airDist = Tools.DistanceKm(
                    companyLat.Value, companyLon.Value,
                    doOrder.OrderLatitude, doOrder.OrderLongitude);

                // Estimate actual distance
                actualDistance = airDist * 1.5;
            }
        }

        // At this point actualDistance must have a value (either OSRM or fallback)
        if (actualDistance == null)
            throw new BO.BlDoesNotExistException("Failed to calculate actual distance.");

        try
        {
            // Create new Delivery record
            lock (AdminManager.BlMutex)
            {
                // Re-check availability within lock to prevent race conditions
                bool stillAvailable = !s_dal.Delivery
                    .ReadAll(d => d.OrderId == orderId && d.DeliveryFinishDate == null)
                    .Any();

                if (!stillAvailable)
                    throw new BO.BlOrderHasActiveDeliveryException($"Order {orderId} was taken by another process.");

                var newDelivery = new DO.Delivery(
                    DeliveryId: 0,                // DAL assigns running ID
                    OrderId: orderId,
                    CourierId: courierId,
                    ActualDistance: actualDistance,
                    DeliveryDate: s_dal.Config.Clock,      // Use CURRENT clock
                    DeliveryFinishDate: null,
                    ShipmentType: DO.ShipmentType.Standard,
                    DeliveryFinishType: null
                );

                s_dal.Delivery.Create(newDelivery);
            }
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
            Func<DO.Order, bool>? dalFilter = null;

            if (filterBy == BO.OrderInListFilterBy.TypeOfOrder && filterValue is not null)
            {
                if (Tools.TryConvertEnum(filterValue, out BO.TypeOfOrder typeVal))
                    dalFilter = o => (BO.TypeOfOrder)o.TypeOfOrder == typeVal;
            }

            IEnumerable<DO.Order> allOrders;
            IEnumerable<DO.Delivery> allDeliveries;
            DateTime clock;
            double baseLatitude, baseLongitude;
            TimeSpan maxRange;

            lock (AdminManager.BlMutex)
            {
                var config = s_dal.Config; // Preload config
                allOrders = s_dal.Order.ReadAll(dalFilter).ToList();
                allDeliveries = s_dal.Delivery.ReadAll().ToList();
                clock = config.Clock;
                maxRange = config.MaxDelTimeRnge;
                baseLatitude = config.Latitude ?? 31.7479;
                baseLongitude = config.Longitude ?? 35.188;
            }

            var query =
                from o in allOrders
                join d in allDeliveries
                    on o.OrderId equals d.OrderId into deliveriesGroup

                let lastDelivery = deliveriesGroup.OrderByDescending(del => del.DeliveryDate).FirstOrDefault()

                let OrderStatus =
                    lastDelivery is null ? BO.OrderStatus.Open :
                    lastDelivery.DeliveryFinishType == null ? BO.OrderStatus.InProgress :
                    lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.Completed ? BO.OrderStatus.Supplied :
                    lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.Cancelled ? BO.OrderStatus.Cancelled :
                    BO.OrderStatus.Refused

                let ScheduleStatus = Tools.CalcScheduleStatus(o.OrderDate, lastDelivery?.DeliveryFinishDate)

                let filterStatusExclude = filterBy == BO.OrderInListFilterBy.OrderStatus && filterValue is not null &&
                                 Tools.TryConvertEnum(filterValue, out BO.OrderStatus statusVal) &&
                                 OrderStatus != statusVal
                where !filterStatusExclude

                let filterScheduleExclude = filterBy == BO.OrderInListFilterBy.ScheduleStatus && filterValue is not null &&
                                 Tools.TryConvertEnum(filterValue, out BO.ScheduleStatus schedVal) &&
                                 ScheduleStatus != schedVal
                where !filterScheduleExclude

                let AirDistance = Tools.DistanceKm(o.OrderLatitude, o.OrderLongitude, baseLatitude, baseLongitude)

                let maxTime = o.OrderDate + maxRange

                let TimeLeftToFinish =
                (OrderStatus == BO.OrderStatus.Supplied || OrderStatus == BO.OrderStatus.Cancelled || OrderStatus == BO.OrderStatus.Refused) ?
                    TimeSpan.Zero :
                (maxTime <= clock ? TimeSpan.Zero : maxTime - clock)

                let TotalHandleTime =
                    (from del in deliveriesGroup
                     where del.DeliveryFinishType == DO.DeliveryFinishType.Completed
                     select (del.DeliveryFinishDate ?? o.OrderDate) - o.OrderDate)
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

            IEnumerable<int> courierDeliveryIds;

            lock (AdminManager.BlMutex)
            {
                courierDeliveryIds = s_dal.Delivery
                      .ReadAll(d => d.CourierId == courierId && d.DeliveryFinishType != null)
                      .Select(d => d.DeliveryId)
                      .ToHashSet();
            }

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
    internal static async Task<IEnumerable<BO.OpenOrderInList>> GetOpenOrdersForCourierAsync(
    int courierId,
    BO.TypeOfOrder? typeFilter,
    BO.OpenOrderSortBy? sortBy)
    {
        Tools.ValidatePersonId(courierId);

        DO.Courier courier;
        double baseLat, baseLon;
        DateTime systemClock;
        TimeSpan maxDelTimeRnge;
        IEnumerable<DO.Order> allOrders;
        HashSet<int> unavailableOrderIds;

        lock (AdminManager.BlMutex)
        {
            courier = s_dal.Courier.Read(c => c.CourierId == courierId)
                ?? throw new DO.DalDoesNotExistException($"Courier {courierId} not found");

            var config = s_dal.Config;

            baseLat = config.Latitude ?? 0;
            baseLon = config.Longitude ?? 0;
            systemClock = config.Clock;
            maxDelTimeRnge = config.MaxDelTimeRnge;

            allOrders = s_dal.Order.ReadAll().ToList();

            unavailableOrderIds = s_dal.Delivery.ReadAll(d =>
                d.DeliveryFinishDate == null ||
                d.DeliveryFinishType == null ||
                d.DeliveryFinishType == DO.DeliveryFinishType.Completed ||
                d.DeliveryFinishType == DO.DeliveryFinishType.Cancelled ||
                d.DeliveryFinishType == DO.DeliveryFinishType.Returned)
                .Select(d => d.OrderId)
                .ToHashSet();
        }

        double? maxDistance = courier.MaxCourierDistance;
        var vehicle = courier.CourierVehicleType;

        try
        {
            var tempQuery = from doOrder in allOrders
                            where !unavailableOrderIds.Contains(doOrder.OrderId)
                            where typeFilter == null || (BO.TypeOfOrder)doOrder.TypeOfOrder == typeFilter

                            let isFragile = doOrder.IsFragile
                            let isTV = (BO.TypeOfOrder)doOrder.TypeOfOrder == BO.TypeOfOrder.TV
                            where (vehicle == DO.CourierVehicleType.Car) ||
                                  (vehicle == DO.CourierVehicleType.Motorcycle && !isTV) ||
                                  (vehicle != DO.CourierVehicleType.Car && vehicle != DO.CourierVehicleType.Motorcycle && !isTV && !isFragile)

                            let airDist = Tools.DistanceKm(doOrder.OrderLatitude, doOrder.OrderLongitude, baseLat, baseLon)
                            where (!maxDistance.HasValue || airDist <= maxDistance.Value)

                            let deadline = doOrder.OrderDate + maxDelTimeRnge

                            select new
                            {
                                Order = doOrder,
                                AirDistance = airDist,
                                Deadline = deadline
                            };

            var tempOrders = tempQuery.ToList();

            var resultList = new List<BO.OpenOrderInList>();

            foreach (var item in tempOrders)
            {
                var actualDistance = await Tools.GetActualDistanceAsync(
                    baseLat, baseLon,
                    item.Order.OrderLatitude,
                    item.Order.OrderLongitude,
                    vehicle);

                resultList.Add(new BO.OpenOrderInList
                {
                    OrderId = item.Order.OrderId,
                    TypeOfOrder = (BO.TypeOfOrder)item.Order.TypeOfOrder,
                    OrderWeight = item.Order.OrderWeight,
                    IsFragile = item.Order.IsFragile,
                    OrderSize = item.Order.OrderSize,
                    CustomerAddress = item.Order.OrderAddress,
                    AirDistance = item.AirDistance,
                    ActualDistance = actualDistance,
                    EstimatedActualTime = null,
                    ScheduleStatus = Tools.CalcScheduleStatus(item.Order.OrderDate, null),
                    TimeLeftToFinish = (item.Deadline <= systemClock) ? TimeSpan.Zero : (item.Deadline - systemClock),
                    MaxDeliveryTime = item.Deadline
                });
            }

            if (sortBy != null)
            {
                resultList = sortBy switch
                {
                    BO.OpenOrderSortBy.TypeOfOrder => resultList.OrderBy(x => x.TypeOfOrder).ThenBy(x => x.OrderId).ToList(),
                    BO.OpenOrderSortBy.AirDistance => resultList.OrderBy(x => x.AirDistance).ThenBy(x => x.OrderId).ToList(),
                    BO.OpenOrderSortBy.ScheduleStatus => resultList.OrderBy(x => x.ScheduleStatus).ThenBy(x => x.OrderId).ToList(),
                    BO.OpenOrderSortBy.OrderId => resultList.OrderBy(x => x.OrderId).ToList(),
                    _ => resultList.OrderBy(x => x.ScheduleStatus).ThenBy(x => x.OrderId).ToList()
                };
            }

            return resultList;
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
        Tools.ValidatePersonId(courierId);
        Tools.ValidateSystemId(orderId);

        bool isAdmin;
        List<DO.Delivery> deliveries;
        List<DO.Courier> couriersSnapshot;

        lock (AdminManager.BlMutex)
        {
            int adminId = s_dal.Config.AdminId;
            isAdmin = (requesterId == adminId);

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

            // Snapshot deliveries + couriers (avoid DAL reads outside lock)
            deliveries = s_dal.Delivery
                .ReadAll(d => d.OrderId == orderId)
                .OrderByDescending(d => d.DeliveryDate)
                .ToList();

            couriersSnapshot = s_dal.Courier.ReadAll().ToList();
        }

        // Build BO list outside lock
        var result =
            from d in deliveries
            let courierName =
                (d.CourierId == 0)
                    ? "System"
                    : (couriersSnapshot.FirstOrDefault(c => c.CourierId == d.CourierId)?.CourierFullName ?? "System")
            select new BO.DeliveryPerOrderInList
            {
                DeliveryId = d.DeliveryId,
                CourierId = (d.CourierId == 0) ? (int?)null : d.CourierId,
                CourierFullName = courierName,
                ShipmentType = (BO.ShipmentType)d.ShipmentType,
                StartDeliveryDate = d.DeliveryDate,

                DeliveryFinishType = (d.DeliveryFinishType == null)
                    ? (BO.DeliveryFinishType?)null
                    : (BO.DeliveryFinishType)d.DeliveryFinishType,

                FinishDeliveryTime = (d.DeliveryFinishType == null)
                    ? (DateTime?)null
                    : d.DeliveryFinishDate
            };

        return result.ToList();
    }


    #endregion ListRetrieval

    //==================== Status Summaries & Helpers ===================\\

    #region StatusSummaries

    /// <summary>
    /// Returns a statistical summary of orders grouped by Status and ScheduleStatus.
    /// </summary>
    internal static int[] GetOrderStatusSummary()
    {
        List<DO.Order> orders;
        List<DO.Delivery> allDeliveries;

        lock (AdminManager.BlMutex)
        {
            orders = s_dal.Order.ReadAll().ToList();
            allDeliveries = s_dal.Delivery.ReadAll().ToList();
        }

        var deliveriesMap = allDeliveries
            .GroupBy(d => d.OrderId)
            .ToDictionary(g => g.Key, g => g.ToList());

        int orderStatusCount = Enum.GetValues(typeof(BO.OrderStatus)).Length;
        int scheduleStatusCount = Enum.GetValues(typeof(BO.ScheduleStatus)).Length;
        int[] summary = new int[orderStatusCount * scheduleStatusCount];

        var grouped =
            from o in orders
            let orderDeliveries = deliveriesMap.ContainsKey(o.OrderId) ? deliveriesMap[o.OrderId] : new List<DO.Delivery>()
            let orderStatus = GetOrderStatus(o, orderDeliveries)
            let scheduleStatus = GetScheduleStatus(o, orderDeliveries)
            group o by new { orderStatus, scheduleStatus } into g
            select new
            {
                Index = (int)g.Key.orderStatus * scheduleStatusCount + (int)g.Key.scheduleStatus,
                Count = g.Count()
            };

        foreach (var item in grouped)
        {
            if (item.Index >= 0 && item.Index < summary.Length)
                summary[item.Index] = item.Count;
        }
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
        IEnumerable<DO.Delivery> orderDeliveries;

        if (deliveries != null)
        {
            orderDeliveries = deliveries;
        }
        else
        {
            lock (AdminManager.BlMutex)
            {
                orderDeliveries = s_dal.Delivery.ReadAll(d => d.OrderId == order.OrderId).ToList();
            }
        }

        if (!orderDeliveries.Any()) return BO.OrderStatus.Open;

        var lastDelivery = orderDeliveries
            .OrderByDescending(d => d.DeliveryDate)
            .ThenByDescending(d => d.DeliveryId)
            .First();

        if (lastDelivery.DeliveryFinishType == null)
            return BO.OrderStatus.InProgress;

        return lastDelivery.DeliveryFinishType switch
        {
            DO.DeliveryFinishType.Completed => BO.OrderStatus.Supplied,
            DO.DeliveryFinishType.Cancelled => BO.OrderStatus.Cancelled,
            DO.DeliveryFinishType.Returned => BO.OrderStatus.Refused,
            DO.DeliveryFinishType.Failed => BO.OrderStatus.Open,
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
        IEnumerable<DO.Delivery> orderDeliveries;

        if (deliveries != null)
        {
            orderDeliveries = deliveries;
        }
        else
        {
            lock (AdminManager.BlMutex)
            {
                orderDeliveries = s_dal.Delivery.ReadAll(d => d.OrderId == order.OrderId).ToList();
            }
        }

        BO.OrderStatus currentStatus = GetOrderStatus(order, orderDeliveries);

        if (currentStatus == BO.OrderStatus.Open)
        {
            return Tools.CalcScheduleStatus(order.OrderDate, null);
        }

        DateTime? lastFinishDate = orderDeliveries
            .Where(d => d.DeliveryFinishType != null)
            .OrderByDescending(d => d.DeliveryFinishDate)
            .Select(d => (DateTime?)d.DeliveryFinishDate)
            .FirstOrDefault();

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
            if (activeDelivery is null) return null;

            var thisOrder = GetOrder(doOrder.OrderId);

            DateTime clock;
            TimeSpan maxDelTime;

            lock (AdminManager.BlMutex)
            {
                _ = s_dal.Courier.Read(c => c.CourierId == activeDelivery.CourierId)
                   ?? throw new DO.DalDoesNotExistException($"Courier with ID={activeDelivery.CourierId} does not exist.");
                clock = s_dal.Config.Clock;
                maxDelTime = s_dal.Config.MaxDelTimeRnge;
            }

            var expectedDeliveryTime = thisOrder.ExpectedDeliveryTime ?? thisOrder.MaxDeliveryTime;
            double airDistance = Tools.DistanceKm(doOrder.OrderLatitude, doOrder.OrderLongitude, 31.7479, 35.188);

            var scheduleStatus = Tools.CalcScheduleStatus(doOrder.OrderDate, null);

            TimeSpan timeLeftToFinish =
                (doOrder.OrderDate + maxDelTime) < clock ?
                    TimeSpan.Zero :
                (doOrder.OrderDate + maxDelTime) - clock;

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
        TimeSpan maxRange;
        DateTime clock;
        List<DO.Delivery> deliveriesForOrder;
        var config = AdminManager.GetConfig();

        lock (AdminManager.BlMutex)
        {
            maxRange = config.MaxDelTimeRnge;
            clock = config.Clock;
            deliveriesForOrder = s_dal.Delivery.ReadAll(d => d.OrderId == doOrder.OrderId).ToList();
        }

        var lastDelivery = deliveriesForOrder.OrderByDescending(del => del.DeliveryDate).FirstOrDefault();

        DO.Courier? courierOfLastDelivery = null;
        if (lastDelivery != null)
        {
            lock (AdminManager.BlMutex)
            {
                courierOfLastDelivery = s_dal.Courier.Read(c => c.CourierId == lastDelivery.CourierId);
            }
        }

        BO.OrderStatus enumOrderStatus;
        if (lastDelivery == null)
            enumOrderStatus = BO.OrderStatus.Open;
        else if (lastDelivery.DeliveryFinishType == null)
            enumOrderStatus = BO.OrderStatus.InProgress;
        else
        {
            enumOrderStatus = lastDelivery.DeliveryFinishType switch
            {
                DO.DeliveryFinishType.Completed => BO.OrderStatus.Supplied,
                DO.DeliveryFinishType.Cancelled => BO.OrderStatus.Cancelled,
                DO.DeliveryFinishType.Returned => BO.OrderStatus.Refused,
                DO.DeliveryFinishType.Failed => BO.OrderStatus.Open,
                _ => throw new BO.BlInvalidDeliveryStatusException($"Unknown delivery finish type: {lastDelivery.DeliveryFinishType}")
            };
        }

        var calculateAirDistance = Tools.DistanceKm(doOrder.OrderLatitude, doOrder.OrderLongitude, config.Latitude ?? 31.7479, config.Longitude ?? 35.188);
        var maxDelTime = doOrder.OrderDate + maxRange;
        var ScheduleStatus = Tools.CalcScheduleStatus(doOrder.OrderDate, lastDelivery?.DeliveryFinishDate);

        var TimeLeftToFinish = (doOrder.OrderDate + maxRange) < clock ? TimeSpan.Zero :
                               (doOrder.OrderDate + maxRange) - clock;

        DateTime? expectedDeliveryTime = null;
        if (lastDelivery != null && courierOfLastDelivery != null)
        {
            double speed = courierOfLastDelivery.CourierVehicleType switch
            {
                DO.CourierVehicleType.Car => config.AvgCarSpeed,
                DO.CourierVehicleType.Motorcycle => config.AvgMotorcycleSpeed,
                DO.CourierVehicleType.Bicycle => config.AvgBicycleSpeed,
                _ => config.AvgWalkSpeed
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
