namespace Helpers;

using DalApi;
using System;
using System.Linq;
using System.Collections.Generic;

//==================== Courier Business Logic Manager ===================\\

/// <summary>
/// Manages logical operations for Couriers.
/// Handles authentication, data retrieval, CRUD operations, and periodic updates.
/// </summary>
internal static class CourierManager
{

    //==================== Observer Manager (Stage 5) ===================\\

    #region ObserverManager

    internal static ObserverManager Observers = new(); //stage 5

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
            // Get all couriers converted to BO.CourierInList
            var boCouriers = from c in s_dal.Courier.ReadAll()
                             select GetCourierInList(c.CourierId);

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
            // Read DO courier
            var doCourier = s_dal.Courier.Read(courierId) ??
                throw new BO.BlDoesNotExistException($"Courier with ID={courierId} does not exist");

            // Convert to full BO to reuse logic (calculations)
            var deliveries = s_dal.Delivery.ReadAll(d => d.CourierId == doCourier.CourierId);

            // Stats calculation
            var completedDeliveries = deliveries
                .Where(d => d.DeliveryFinishType == DO.DeliveryFinishType.Completed)
                .ToList();

            // On-time vs Late calculation
            var maxRange = s_dal.Config.MaxDelTimeRnge;
            int onTime = completedDeliveries.Count(d => d.DeliveryFinishDate - d.DeliveryDate <= maxRange);
            int late = completedDeliveries.Count - onTime;

            // Active Order
            var activeDelivery = deliveries.FirstOrDefault(d => d.DeliveryFinishType == DO.DeliveryFinishType.None);

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

        // Validate location
        if (Tools.GetLocationFromAddress(courier.CourierLocation) == null)
            throw new BO.BlInvalidStringException($"Location '{courier.CourierLocation}' is invalid.");

        try
        {
            // Add new courier
            DO.Courier doCourier = ConvertBoToDoCourier(courier);
            s_dal.Courier.Create(doCourier);
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
            DO.Courier doCourier = s_dal.Courier.Read(id)
                ?? throw new BO.BlDoesNotExistException($"Courier with ID={id} does not exist");

            return ConvertDoToBoCourier(doCourier);
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

        // Validate location
        if (Tools.GetLocationFromAddress(courier.CourierLocation) == null)
            throw new BO.BlInvalidStringException($"Location '{courier.CourierLocation}' is invalid.");

        try
        {
            // Update courier
            DO.Courier doCourier = ConvertBoToDoCourier(courier);
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
            // Check if courier exists
            DO.Courier? doCourier = s_dal.Courier.Read(id)
                ?? throw new BO.BlDoesNotExistException($"Courier with ID={id} does not exist");

            // Check if can delete
            if (!CanDelete(id))
                throw new BO.BlCourierHasDeliveriesException($"Cannot delete courier->{id} because they have associated deliveries.");

            // Delete courier
            s_dal.Courier.Delete(id);
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
        // Get deliveries
        var deliveries = s_dal.Delivery.ReadAll(d => d.CourierId == doCourier.CourierId);

        // Stats calculation
        var completedDeliveries = deliveries
            .Where(d => d.DeliveryFinishType == DO.DeliveryFinishType.Completed)
            .ToList();

        // On-time vs Late calculation
        var maxRange = s_dal.Config.MaxDelTimeRnge;
        int onTime = completedDeliveries.Count(d => d.DeliveryFinishDate - d.DeliveryDate <= maxRange);
        int late = completedDeliveries.Count - onTime;

        // Active Order
        var activeDelivery = deliveries.FirstOrDefault(d => d.DeliveryFinishType == DO.DeliveryFinishType.None);

        BO.OrderInProgress? orderInProgress = null;
        if (activeDelivery is not null)
        {
            var ord = s_dal.Order.Read(activeDelivery.OrderId) ??
                 throw new BO.BlDoesNotExistException($"Order {activeDelivery.OrderId} for courier {doCourier.CourierId} not found");

            orderInProgress = OrderManager.BuildOrderInProgress(ord, activeDelivery);
        }

        return new BO.Courier
        {
            CourierId = doCourier.CourierId,
            CourierFullName = doCourier.CourierFullName,
            CourierCellPhone = doCourier.CourierCellPhone,
            CourierEmail = doCourier.CourierEmail,
            CourierPassword = doCourier.CourierPassword,
            CourierIsActive = doCourier.CourierEnabled,
            CourierLocation = doCourier.CourierAddress,
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
            CourierAddress: boCourier.CourierLocation,
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
            // Check if there are ANY deliveries associated with this courier
            bool hasDeliveries = s_dal.Delivery
                .ReadAll(d => d.CourierId == courierId) // Returns IEnumerable
                .Any();                                 //stops at the first match

            // Can delete only if no deliveries
            return !hasDeliveries;
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to validate courier deletion capability", ex);
        }
    }

    #endregion DeletionCheck

    //==================== Periodic Updates ===================\\

    #region PeriodicUpdates

    /// <summary>
    /// Performs periodic updates for couriers (e.g., setting them inactive if idle for too long).
    /// </summary>
    internal static void PeriodicCouriersUpdates(DateTime oldClock, DateTime newClock)
    {
        try
        {
            // Get all active couriers
            var activeCouriers = s_dal.Courier.ReadAll(c => c.CourierEnabled).ToList();
            // Update each courier's activity status
            foreach (var courier in activeCouriers)
            {
                // Update activity based on closed deliveries
                UpdateCourierActivityByClosedDeliveries(courier, newClock);
            }

            // Notify observers of the courier list update
            Observers.NotifyListUpdated();
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to perform periodic courier updates", ex);
        }
    }

    /// <summary>
    /// Checks if a courier should be deactivated based on inactivity time.
    /// </summary>
    private static void UpdateCourierActivityByClosedDeliveries(DO.Courier courier, DateTime currentClock)
    {
        try
        {
            bool hasActiveDelivery = s_dal.Delivery
                .ReadAll(d => d.CourierId == courier.CourierId && d.DeliveryFinishType == DO.DeliveryFinishType.None)
                .Any();

            if (hasActiveDelivery)
            {
                if (!courier.CourierEnabled)
                {
                    var activeCourier = courier with { CourierEnabled = true };
                    s_dal.Courier.Update(activeCourier);
                    Observers.NotifyItemUpdated(courier.CourierId);
                }
                return;
            }

            var lastClosedDelivery = s_dal.Delivery
                .ReadAll(d => d.CourierId == courier.CourierId && d.DeliveryFinishType != DO.DeliveryFinishType.None)
                .OrderByDescending(d => d.DeliveryFinishDate)
                .FirstOrDefault();

            var config = AdminManager.GetConfig();
            bool shouldBeActive;

            if (lastClosedDelivery is null)
            {
                var timeSinceStart = currentClock - courier.SeniorityOfCourier;
                shouldBeActive = timeSinceStart < config.UnactiveTimeRnge;
            }
            else
            {
                var timePassed = currentClock - lastClosedDelivery.DeliveryFinishDate;
                shouldBeActive = timePassed < config.UnactiveTimeRnge;
            }

            if (courier.CourierEnabled != shouldBeActive)
            {
                var updatedCourier = courier with { CourierEnabled = shouldBeActive };
                s_dal.Courier.Update(updatedCourier);

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

    #endregion PeriodicUpdates

}