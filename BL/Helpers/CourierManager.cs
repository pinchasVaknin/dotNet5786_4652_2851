namespace Helpers;

using BO;
using DalApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//==================== Courier Business Logic Manager ===================\\

/// <summary>
/// Manages logical operations for Couriers.
/// Handles authentication, data retrieval, CRUD operations, and periodic updates.
/// </summary>
internal static class CourierManager
{

    //==================== Observer Manager (Stage5) ===================\\

    #region ObserverManager

    internal static ObserverManager Observers = new(); //stage5

    #endregion ObserverManager

    //==================== DAL Access ===================\\

    #region DalAccess

    private static IDal s_dal = Factory.Get;

    #endregion DalAccess

    //==================== Authentication & Lists ===================\\

    #region AuthAndLists

    /// <summary>
    /// Authenticates a user and determines their role (Admin or Courier).
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>The <see cref="BO.UserRole"/> if authentication is successful.</returns>
    /// <exception cref="BO.BlInvalidPasswordException">Thrown if the password is incorrect.</exception>
    /// <exception cref="BO.BlUserNotFoundException">Thrown if the user ID is not found.</exception>
    /// <exception cref="BO.BlXMLFileLoadCreateException">Thrown if there is a data access error.</exception>
    internal static BO.UserRole GetUserRole(int userId, string password)
    {
        // Initial setup check
        var config = AdminManager.GetConfig();

        // If AdminId is 0 => system not set up yet
        if (config.AdminId == 0)
        {
            if (userId == 0)
            {
                // Initial setup mode
                if (password == config.AdminPassword)
                    return BO.UserRole.Admin;

                // Incorrect password during initial setup
                throw new BO.BlInvalidPasswordException("Incorrect password for system setup (Reset State).");
            }
        }

        // Validate inputs
        Tools.ValidatePersonId(userId);
        Tools.ValidateNotNull(password);

        try
        {
            //Check Admin
            if (config.AdminId == userId)
            {
                if (config.AdminPassword == password)
                    return BO.UserRole.Admin;
                else
                    throw new BO.BlInvalidPasswordException("Incorrect password for admin user");
            }

            //Check Courier
            var courierUser = s_dal.Courier.Read(userId);
            if (courierUser is not null)
            {
                if (courierUser.CourierPassword == password)
                    return BO.UserRole.Courier;
                else
                    throw new BO.BlInvalidPasswordException("Incorrect password for courier user");
            }

            //User not found
            throw new BO.BlUserNotFoundException("User ID does not exist");
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to get user role", ex);
        }
    }

    /// <summary>
    /// Retrieves a filtered and sorted list of couriers.
    /// </summary>
    /// <param name="requesterId">The ID of the requester (must be an admin).</param>
    /// <param name="filterBy">The filtering criteria.</param>
    /// <param name="filterValue">The value to filter by.</param>
    /// <param name="sortBy">The sorting criteria.</param>
    /// <returns>A list of <see cref="BO.CourierInList"/> objects.</returns>
    /// <exception cref="BO.BlXMLFileLoadCreateException">Thrown if there is a data access error.</exception>
    internal static IEnumerable<BO.CourierInList> GetListOfCouriers(
    BO.CourierInListFilterBy? filterBy = null,
    object? filterValue = null,
    BO.CourierInListSortBy? sortBy = null)
    {
        try
        {
            IEnumerable<BO.CourierInList> boCouriers;
            List<int> courierIds;

            lock (AdminManager.BlMutex)
            {
                courierIds = s_dal.Courier.ReadAll()
                .Select(c => c.CourierId)
                .ToList();
            }

            boCouriers = courierIds.Select(id => GetCourierInList(id));

            // Apply Filtering
            if (filterBy.HasValue && filterValue is not null)
            {
                switch (filterBy.Value)
                {

                    case BO.CourierInListFilterBy.CourierIsActive:
                        if (bool.TryParse(filterValue.ToString(), out bool isActiveVal))
                            boCouriers = boCouriers.Where(x => x.CourierIsActive == isActiveVal);
                        break;

                    case BO.CourierInListFilterBy.VehicleType:
                        if (Tools.TryConvertEnum(filterValue, out BO.VehicleType vehicleVal))
                            boCouriers = boCouriers.Where(x => x.VehicleType == vehicleVal);
                        break;

                    case BO.CourierInListFilterBy.OrderIdInHandle:
                        if (bool.TryParse(filterValue.ToString(), out bool hasOrder))
                        {
                            if (hasOrder)
                                boCouriers = boCouriers.Where(x => x.OrderIdInHandle != null && x.OrderIdInHandle != 0);
                            else
                                boCouriers = boCouriers.Where(x => x.OrderIdInHandle == null || x.OrderIdInHandle == 0);
                        }
                        break;
                }
            }

            // Apply Sorting
            boCouriers = sortBy switch
            {
                BO.CourierInListSortBy.CourierFullName => boCouriers.OrderBy(c => c.CourierFullName),
                BO.CourierInListSortBy.CourierIsActive => boCouriers.OrderBy(c => c.CourierIsActive),
                BO.CourierInListSortBy.VehicleType => boCouriers.OrderBy(c => c.VehicleType),
                BO.CourierInListSortBy.StartWorkDate => boCouriers.OrderBy(c => c.StartWorkDate),
                BO.CourierInListSortBy.DeliveriesInTime => boCouriers.OrderByDescending(c => c.DeliveriesInTime),
                BO.CourierInListSortBy.DeliveriesOverTime => boCouriers.OrderByDescending(c => c.DeliveriesOverTime),
                BO.CourierInListSortBy.OrderIdInHandle => boCouriers.OrderBy(c => c.OrderIdInHandle),
                null or BO.CourierInListSortBy.CourierId or _ => boCouriers.OrderBy(c => c.CourierId),
            };

            return boCouriers.ToList();
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to load couriers list", ex);
        }
    }

    /// <summary>
    /// Converts a single courier to its list representation (CourierInList).
    /// </summary>
    /// <param name="courierId">The ID of the courier.</param>
    /// <returns>A <see cref="BO.CourierInList"/> object.</returns>
    internal static BO.CourierInList GetCourierInList(int courierId)
    {
        try
        {

            IEnumerable<DO.Delivery> deliveries;
            DO.Courier doCourier;
            TimeSpan maxRange;

            lock (AdminManager.BlMutex) //stage 7
            {
                // Read DO courier
                doCourier = s_dal.Courier.Read(courierId) ??
                throw new BO.BlDoesNotExistException($"Courier with ID={courierId} does not exist");

                // Convert to full BO to reuse logic (calculations)
                deliveries = s_dal.Delivery.ReadAll(d => d.CourierId == doCourier.CourierId).ToList();

                maxRange = s_dal.Config.MaxDelTimeRnge;
            }

            // Stats calculation
            var completedDeliveries = deliveries
            .Where(d => d.DeliveryFinishType == DO.DeliveryFinishType.Completed)
            .ToList();

            // On-time vs Late calculation
            int onTime = completedDeliveries.Count(d => d.DeliveryFinishDate - d.DeliveryDate <= maxRange);
            int late = completedDeliveries.Count - onTime;

            // Active Order
            var activeDelivery = deliveries.FirstOrDefault(d => d.DeliveryFinishType == null);

            // Get active order ID if any
            int? activeOrderId = activeDelivery?.OrderId;

            // Build and return CourierInList
            return new BO.CourierInList
            {
                CourierId = doCourier.CourierId,
                CourierFullName = doCourier.CourierFullName,
                CourierIsActive = doCourier.CourierEnabled,
                VehicleType = (BO.VehicleType)doCourier.CourierVehicleType,
                StartWorkDate = doCourier.SeniorityOfCourier,
                DeliveriesInTime = onTime,
                DeliveriesOverTime = late,
                OrderIdInHandle = activeOrderId
            };
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to load courier (InList)", ex);
        }
    }

    #endregion AuthAndLists

    //==================== CRUD Operations ===================\\

    #region CrudOperations

    /// <summary>
    /// Adds a new courier to the system.
    /// </summary>
    /// <param name="courier">The courier object to add.</param>
    internal static void AddCourier(BO.Courier courier)
    {
        // Validate input
        Tools.ValidateCourier(courier);

        try
        {
            // Add new courier
            DO.Courier doCourier = ConvertBoToDoCourier(courier);
            lock (AdminManager.BlMutex) //stage 7
            {
                s_dal.Courier.Create(doCourier);
            }
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to add courier", ex);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException("Failed to add courier", ex);
        }

        // Notify observers of the courier update
        Observers.NotifyListUpdated(); //stage 5
    }

    /// <summary>
    /// Retrieves detailed information about a specific courier.
    /// </summary>
    /// <param name="id">The courier's ID.</param>
    /// <returns>The <see cref="BO.Courier"/> object.</returns>
    internal static BO.Courier GetCourier(int id)
    {
        Tools.ValidatePersonId(id);

        try
        {
            lock (AdminManager.BlMutex) //stage 7
            {
                DO.Courier doCourier = s_dal.Courier.Read(id)
                ?? throw new BO.BlDoesNotExistException($"Courier with ID={id} does not exist");

                return ConvertDoToBoCourier(doCourier);
            }
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to load courier", ex);
        }
    }

    /// <summary>
    /// Updates an existing courier's details.
    /// </summary>
    /// <param name="courier">The courier object with updated details.</param>
    internal static void UpdateCourier(BO.Courier courier)
    {
        // Validate input
        Tools.ValidateCourier(courier);

        try
        {
            // Update courier
            DO.Courier doCourier = ConvertBoToDoCourier(courier);
            lock (AdminManager.BlMutex) //stage 7
                s_dal.Courier.Update(doCourier);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to update courier", ex);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Failed to update courier", ex);
        }

        // Notify observers of the courier update
        Observers.NotifyListUpdated(); //stage 5
        Observers.NotifyItemUpdated(courier.CourierId); //stage 5
    }

    /// <summary>
    /// Deletes the courier with the specified ID.
    /// </summary>
    /// <remarks>This method removes the courier from the system if they exist and have no associated
    /// deliveries. If the courier does not exist or cannot be deleted due to associated deliveries, an exception is
    /// thrown.</remarks>
    /// <param name="id">The unique identifier of the courier to delete. Must be a valid courier ID.</param>
    /// <exception cref="BO.BlDoesNotExistException">Thrown if the courier with the specified ID does not exist.</exception>
    /// <exception cref="BO.BlCourierHasDeliveriesException">Thrown if the courier cannot be deleted because they have associated deliveries.</exception>
    /// <exception cref="BO.BlXMLFileLoadCreateException">Thrown if there is an error loading or creating the underlying XML file during the delete operation.</exception>
    internal static void DeleteCourier(int id)
    {
        // Validate input
        Tools.ValidatePersonId(id);

        try
        {
            lock (AdminManager.BlMutex) //stage 7
            {
                // Check if courier exists
                DO.Courier? doCourier = s_dal.Courier.Read(id)
                ?? throw new BO.BlDoesNotExistException($"Courier with ID={id} does not exist");

                // Check if can delete
                if (!CanDelete(id))
                    throw new BO.BlCourierHasDeliveriesException($"Cannot delete courier->{id} because they have associated deliveries.");

                // Delete courier
                s_dal.Courier.Delete(id);
            }
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Failed to delete courier {id}", ex);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("System file error during courier deletion", ex);
        }

        // Notify observers of the courier update
        Observers.NotifyListUpdated(); //stage 5
        Observers.NotifyItemUpdated(id); //stage 5
    }

    #endregion CrudOperations

    //==================== Conversions (DO <-> BO) ===================\\

    #region Conversions

    /// <summary>
    /// Converts a DO.Courier to a BO.Courier, calculating statistics and active orders.
    /// </summary>
    private static BO.Courier ConvertDoToBoCourier(DO.Courier doCourier)
    {
        IEnumerable<DO.Delivery> deliveries;
        TimeSpan maxRange;
        DO.Order? activeOrder = null;

        lock (AdminManager.BlMutex)
        {
            deliveries = s_dal.Delivery.ReadAll(d => d.CourierId == doCourier.CourierId).ToList();
            maxRange = s_dal.Config.MaxDelTimeRnge;

            var activeDeliveryCheck = deliveries.FirstOrDefault(d => d.DeliveryFinishType == null);
            if (activeDeliveryCheck != null)
            {
                activeOrder = s_dal.Order.Read(activeDeliveryCheck.OrderId);
            }
        }

        var completedDeliveries = deliveries
        .Where(d => d.DeliveryFinishType == DO.DeliveryFinishType.Completed)
        .ToList();

        int onTime = completedDeliveries.Count(d => d.DeliveryFinishDate - d.DeliveryDate <= maxRange);
        int late = completedDeliveries.Count - onTime;

        BO.OrderInProgress? orderInProgress = null;
        var activeDelivery = deliveries.FirstOrDefault(d => d.DeliveryFinishType == null);

        if (activeDelivery is not null && activeOrder is not null)
        {
            orderInProgress = OrderManager.BuildOrderInProgress(activeOrder, activeDelivery);
        }

        return new BO.Courier
        {
            CourierId = doCourier.CourierId,
            CourierFullName = doCourier.CourierFullName,
            CourierCellPhone = doCourier.CourierCellPhone,
            CourierEmail = doCourier.CourierEmail,
            CourierPassword = doCourier.CourierPassword,
            CourierIsActive = doCourier.CourierEnabled,
            MaxCourierDistance = doCourier.MaxCourierDistance,
            VehicleType = (BO.VehicleType)doCourier.CourierVehicleType,
            StartWorkDate = doCourier.SeniorityOfCourier,
            TotalOnTimeDeliveries = onTime,
            TotalLateDeliveries = late,
            OrderInProgress = orderInProgress
        };
    }

    /// <summary>
    /// Converts a BO.Courier to a DO.Courier.
    /// </summary>
    private static DO.Courier ConvertBoToDoCourier(BO.Courier boCourier) =>
    new DO.Courier(
    CourierId: boCourier.CourierId,
    CourierFullName: boCourier.CourierFullName,
    CourierCellPhone: boCourier.CourierCellPhone,
    CourierEmail: boCourier.CourierEmail,
    CourierPassword: boCourier.CourierPassword,
    CourierEnabled: boCourier.CourierIsActive,
    MaxCourierDistance: boCourier.MaxCourierDistance,
    SeniorityOfCourier: boCourier.StartWorkDate,
    CourierVehicleType: (DO.CourierVehicleType)boCourier.VehicleType
    );

    #endregion Conversions

    //==================== Deletion Check ===================\\

    #region DeletionCheck

    /// <summary>
    /// Determines whether a courier can be deleted based on their delivery assignments.
    /// </summary>
    /// <param name="courierId">The unique identifier of the courier to check.</param>
    /// <returns><see langword="true"/> if the courier can be deleted (i.e., they have no associated deliveries); otherwise, <see
    /// langword="false"/>.</returns>
    /// <exception cref="BO.BlXMLFileLoadCreateException">Thrown if an error occurs while attempting to load or access the underlying data store.</exception>
    public static bool CanDelete(int courierId)
    {
        try
        {
            lock (AdminManager.BlMutex) //stage 7
            {
                // Check if there are ANY deliveries associated with this courier
                bool hasDeliveries = s_dal.Delivery
                .ReadAll(d => d.CourierId == courierId) // Returns IEnumerable
                .Any();                                 //stops at the first match

                // Can delete only if no deliveries
                return !hasDeliveries;
            }
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to validate courier deletion capability", ex);
        }
    }

    #endregion DeletionCheck

    //==================== Periodic Updates ===================\\

    #region PeriodicUpdates

    private static readonly AsyncMutex s_periodicMutex = new(); //stage7
    private static readonly AsyncMutex s_simulationMutex = new(); // stage 7 - prevent overlap
    private static readonly Random s_rand = new();

    /// <summary>
    /// Performs periodic updates for couriers (e.g., setting them inactive if idle for too long).
    /// </summary>
    internal static void PeriodicCouriersUpdates(DateTime oldClock, DateTime newClock)
    {
        // If the previous periodic update is still in progress, skip this invocation
        if (s_periodicMutex.CheckAndSetInProgress())
            return;

        try
        {
            IEnumerable<DO.Courier> activeCouriers;

            lock (AdminManager.BlMutex) //stage7
            {
                // Get all active couriers (materialize to avoid deferred execution outside lock)
                activeCouriers = s_dal.Courier.ReadAll(c => c.CourierEnabled).ToList();
            }

            // Update each courier's activity status
            foreach (var courier in activeCouriers)
            {
                // Update activity based on closed deliveries
                UpdateCourierActivityByClosedDeliveries(courier, newClock);
            }

            // Notify observers of the courier list update (outside of any DAL lock)
            Observers.NotifyListUpdated();
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to perform periodic courier updates", ex);
        }
        finally
        {
            s_periodicMutex.UnsetInProgress();
        }
    }

    /// <summary>
    /// Checks if a courier should be deactivated based on inactivity time.
    /// </summary>
    private static void UpdateCourierActivityByClosedDeliveries(DO.Courier courier, DateTime currentClock)
    {
        try
        {
            // Local variables
            bool hasActiveDelivery;
            DO.Delivery? lastClosedDelivery;
            TimeSpan unactiveTimeRnge;

            // Snapshot under
            lock (AdminManager.BlMutex) //stage7
            {
                // Check for active delivery
                hasActiveDelivery = s_dal.Delivery
                    .ReadAll(d => d.CourierId == courier.CourierId && d.DeliveryFinishType == null)
                    .Any();

                // Get last closed delivery
                lastClosedDelivery = s_dal.Delivery
                    .ReadAll(d => d.CourierId == courier.CourierId && d.DeliveryFinishType != null)
                    .OrderByDescending(d => d.DeliveryFinishDate)
                    .FirstOrDefault();

                // Get unactive time range from config
                unactiveTimeRnge = s_dal.Config.UnactiveTimeRnge;
            }

            // If courier has an active delivery, ensure they are marked active
            if (hasActiveDelivery)
            {
                // Ensure courier is active
                if (!courier.CourierEnabled)
                {
                    // Activate courier
                    var activeCourier = courier with { CourierEnabled = true };
                    lock (AdminManager.BlMutex) //stage7
                    {
                        s_dal.Courier.Update(activeCourier);
                    }
                    Observers.NotifyItemUpdated(courier.CourierId);
                }
                return;
            }

            // Determine if courier should be active
            bool shouldBeActive;

            // Determine activity based on last closed delivery or seniority
            if (lastClosedDelivery is null)
            {
                // No closed deliveries; use seniority date
                var timeSinceStart = currentClock - courier.SeniorityOfCourier;
                shouldBeActive = timeSinceStart < unactiveTimeRnge;
            }
            else
            {
                // Calculate time passed since last closed delivery
                var timePassed = currentClock - lastClosedDelivery.DeliveryFinishDate;
                shouldBeActive = timePassed < unactiveTimeRnge;
            }

            // Update courier activity if changed
            if (courier.CourierEnabled != shouldBeActive)
            {
                var updatedCourier = courier with { CourierEnabled = shouldBeActive };

                lock (AdminManager.BlMutex) //stage7
                {
                    s_dal.Courier.Update(updatedCourier);
                }

                Observers.NotifyItemUpdated(courier.CourierId);
            }
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to update courier activity", ex);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Failed to update courier activity", ex);
        }
    }

    /// <summary>
    /// Simulates courier activity by randomly assigning orders and completing deliveries.
    /// </summary>
    /// <returns> A task representing the asynchronous operation.</returns>
    internal static async Task SimulateCourierActivityAsync()
    {
        // If the previous simulation is still in progress, skip this invocation
        if (s_simulationMutex.CheckAndSetInProgress())
            return;

        try
        {
            List<DO.Courier> activeCouriers;
            List<DO.Order> allOrders;

            // Snapshot under lock (no awaits inside lock)
            lock (AdminManager.BlMutex)
            {
                activeCouriers = s_dal.Courier.ReadAll(c => c.CourierEnabled).ToList();
                allOrders = s_dal.Order.ReadAll().ToList();
            }

            if (activeCouriers.Count == 0 || allOrders.Count == 0)
                return;

            // Pick ONE courier per tick (keeps network calls under control)
            DO.Courier courier = activeCouriers[s_rand.Next(activeCouriers.Count)];

            // Does this courier currently have an open delivery?
            DO.Delivery? activeDelivery;
            lock (AdminManager.BlMutex)
            {
                activeDelivery = s_dal.Delivery
                    .ReadAll(d => d.CourierId == courier.CourierId && d.DeliveryFinishDate == null)
                    .OrderByDescending(d => d.DeliveryDate)
                    .FirstOrDefault();
            }

            // If courier is busy -> maybe complete the delivery
            if (activeDelivery is not null)
            {
                if (ShouldCompleteNow(courier, activeDelivery))
                {
                    // Mostly completed; sometimes returned (you can tune probabilities)
                    BO.DeliveryFinishType finishType =
                        (s_rand.NextDouble() < 0.90)
                            ? BO.DeliveryFinishType.Completed
                            : BO.DeliveryFinishType.Returned;

                    // Use OrderManager helper so it updates DAL + does correct notifications
                    OrderManager.CompleteOrderHandling(courier.CourierId, activeDelivery.DeliveryId, finishType);
                }

                return;
            }

            // Courier is idle -> maybe pick an order to handle
            // Reduce network pressure: only sometimes attempt assignment
            if (s_rand.NextDouble() > 0.35)
                return;

            // Try a few random candidates until one works
            for (int attempt = 0; attempt < 5 && allOrders.Count > 0; attempt++)
            {
                DO.Order candidate = allOrders[s_rand.Next(allOrders.Count)];

                try
                {
                    // Use OrderManager helper so it:
                    // - validates order is open
                    // - validates courier has no active delivery
                    // - optionally computes actual distance (async)
                    // - creates delivery
                    // - does correct notifications (orderId + courierId)
                    await OrderManager.AssignOrderToCourier(courier.CourierId, candidate.OrderId, actualDistance: null)
                                      .ConfigureAwait(false);

                    break; // success
                }
                catch (Exception)
                {
                    // Not assignable (not open / bad address / constraints / etc.) -> try another
                    allOrders.Remove(candidate);
                }
            }
        }
        finally
        {
            s_simulationMutex.UnsetInProgress();
        }
    }

    /// <summary>
    /// Determines if a delivery should be marked as complete based on estimated time and randomness.
    /// </summary>
    /// <param name="courier"> The courier handling the delivery.</param>
    /// <param name="delivery"> The courier handling the delivery.</param>
    /// <returns> <see langword="true"/> if the delivery should be completed now; otherwise, <see langword="false"/>.</returns>
    private static bool ShouldCompleteNow(DO.Courier courier, DO.Delivery delivery)
    {
        // Snapshot needed data under lock
        DateTime now;
        double avgCarSpeed, avgMotorSpeed, avgBicycleSpeed, avgWalkSpeed;
        double? companyLat, companyLon;
        DO.Order? order;

        lock (AdminManager.BlMutex)
        {
            now = s_dal.Config.Clock;

            avgCarSpeed = s_dal.Config.AvgCarSpeed;
            avgMotorSpeed = s_dal.Config.AvgMotorcycleSpeed;
            avgBicycleSpeed = s_dal.Config.AvgBicycleSpeed;
            avgWalkSpeed = s_dal.Config.AvgWalkSpeed;

            companyLat = s_dal.Config.Latitude;
            companyLon = s_dal.Config.Longitude;

            order = s_dal.Order.Read(o => o.OrderId == delivery.OrderId);
        }

        if (order is null)
            return false;

        // Prefer actual distance if exists; else fallback to air distance if company coords exist
        double distanceKm;
        if (delivery.ActualDistance.HasValue)
        {
            distanceKm = delivery.ActualDistance.Value;
        }
        else if (companyLat.HasValue && companyLon.HasValue)
        {
            distanceKm = Tools.DistanceKm(companyLat.Value, companyLon.Value, order.OrderLatitude, order.OrderLongitude);
        }
        else
        {
            distanceKm = 1.0; // safe fallback
        }

        double speed = courier.CourierVehicleType switch
        {
            DO.CourierVehicleType.Car => avgCarSpeed,
            DO.CourierVehicleType.Motorcycle => avgMotorSpeed,
            DO.CourierVehicleType.Bicycle => avgBicycleSpeed,
            _ => avgWalkSpeed
        };

        if (speed <= 0)
            speed = 10; // fallback

        TimeSpan estimated = TimeSpan.FromHours(distanceKm / speed);

        // If enough time passed -> complete
        if (now - delivery.DeliveryDate >= estimated)
            return true;

        // Otherwise, small chance to complete early (simulating “reported done”)
        return s_rand.NextDouble() < 0.05;
    }

    #endregion PeriodicUpdates

}
