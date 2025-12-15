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
        Tools.ValidatePersonId(userId);
        Tools.ValidateNotNull(password);

        try
        {
            // 1. Check Admin
            var config = AdminManager.GetConfig();
            if (config.AdminId == userId)
            {
                if (config.AdminPassword == password)
                    return BO.UserRole.Admin;
                else
                    throw new BO.BlInvalidPasswordException("Incorrect password for admin user");
            }

            // 2. Check Courier
            var courierUser = s_dal.Courier.Read(userId);
            if (courierUser is not null)
            {
                if (courierUser.CourierPassword == password)
                    return BO.UserRole.Courier;
                else
                    throw new BO.BlInvalidPasswordException("Incorrect password for courier user");
            }

            // 3. User not found
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
    /// <param name="requesterId">The ID of the user requesting the list (for future permissions).</param>
    /// <param name="isActiveFilter">Optional filter by active status.</param>
    /// <param name="sortBy">Optional sorting criteria.</param>
    /// <returns>A list of <see cref="BO.CourierInList"/>.</returns>
    internal static IEnumerable<BO.CourierInList> GetListOfCouriers(
        int requesterId,
        bool? isActiveFilter = null,
        BO.CourierListSortBy? sortBy = null)
    {
        try
        {
            // Get all couriers converted to BO.CourierInList
            var boCouriers = from c in s_dal.Courier.ReadAll()
                             select GetCourierInList(c.CourierId);

            // Apply Active filter
            if (isActiveFilter is bool active)
                boCouriers = boCouriers.Where(c => c.CourierIsActive == active);

            // Apply Sorting
            boCouriers = sortBy switch
            {
                BO.CourierListSortBy.CourierFullName => boCouriers.OrderBy(c => c.CourierFullName),
                BO.CourierListSortBy.CourierIsActive => boCouriers.OrderBy(c => c.CourierIsActive),
                BO.CourierListSortBy.VehicleType => boCouriers.OrderBy(c => c.VehicleType),
                BO.CourierListSortBy.StartWorkDate => boCouriers.OrderBy(c => c.StartWorkDate),
                BO.CourierListSortBy.DeliveriesInTime => boCouriers.OrderByDescending(c => c.DeliveriesInTime),
                BO.CourierListSortBy.DeliveriesOverTime => boCouriers.OrderByDescending(c => c.DeliveriesOverTime),
                BO.CourierListSortBy.OrderIdInHandle => boCouriers.OrderBy(c => c.OrderIdInHandle),
                null or BO.CourierListSortBy.CourierId or _ => boCouriers.OrderBy(c => c.CourierId),
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
            var doCourier = s_dal.Courier.Read(courierId) ??
                throw new BO.BlDoesNotExistException($"Courier with ID={courierId} does not exist");

            // Convert to full BO to reuse logic (calculations)
            var boCourier = ConvertDoToBoCourier(doCourier);

            return new BO.CourierInList
            {
                CourierId = boCourier.CourierId,
                CourierFullName = boCourier.CourierFullName,
                CourierIsActive = boCourier.CourierIsActive,
                VehicleType = boCourier.VehicleType,
                StartWorkDate = boCourier.StartWorkDate,
                DeliveriesInTime = boCourier.TotalOnTimeDeliveries,
                DeliveriesOverTime = boCourier.TotalLateDeliveries,
                OrderIdInHandle = boCourier.OrderInProgress?.OrderId
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
        Tools.ValidateCourier(courier);

        if (Tools.GetLocationFromAddress(courier.CourierLocation) == null)
            throw new BO.BlInvalidStringException($"Location '{courier.CourierLocation}' is invalid.");

        try
        {
            var existing = s_dal.Courier.Read(courier.CourierId);
            if (existing is not null)
                throw new BO.BlAlreadyExistsException($"Courier with ID={courier.CourierId} already exists.");

            DO.Courier doCourier = ConvertBoToDoCourier(courier);
            s_dal.Courier.Create(doCourier);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException("Failed to add courier", ex);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to add courier", ex);
        }
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
        Tools.ValidateCourier(courier);

        if (Tools.GetLocationFromAddress(courier.CourierLocation) == null)
            throw new BO.BlInvalidStringException($"Location '{courier.CourierLocation}' is invalid.");

        try
        {
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
    }

    /// <summary>
    /// Deletes a courier from the system.
    /// </summary>
    /// <param name="id">The ID of the courier to delete.</param>
    /// <exception cref="BO.BlCourierHasActiveDeliveryException">Thrown if the courier has an active delivery.</exception>
    internal static void DeleteCourier(int id)
    {
        Tools.ValidatePersonId(id);

        try
        {
            DO.Courier? doCourier = s_dal.Courier.Read(id)
                ?? throw new BO.BlDoesNotExistException($"Courier with ID={id} does not exist");

            // Check if courier has an active delivery (FinishType == None)
            bool hasActiveDelivery = s_dal.Delivery
                .ReadAll(d => d.CourierId == id)
                .Any(d => d.DeliveryFinishType == DO.DeliveryFinishType.None);

            if (hasActiveDelivery)
                throw new BO.BlCourierHasActiveDeliveryException($"Cannot delete courier {id}: courier has an active delivery.");

            s_dal.Courier.Delete(id);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Failed to delete courier", ex);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlXMLFileLoadCreateException("Failed to delete courier", ex);
        }
    }

    #endregion CrudOperations

    //==================== Conversions (DO <-> BO) ===================\\

    #region Conversions

    /// <summary>
    /// Converts a DO.Courier to a BO.Courier, calculating statistics and active orders.
    /// </summary>
    private static BO.Courier ConvertDoToBoCourier(DO.Courier doCourier)
    {
        // 1. Get deliveries
        var deliveries = s_dal.Delivery.ReadAll(d => d.CourierId == doCourier.CourierId);

        // 2. Stats calculation
        var completedDeliveries = deliveries
            .Where(d => d.DeliveryFinishType == DO.DeliveryFinishType.Completed)
            .ToList();

        var maxRange = s_dal.Config.MaxDelTimeRnge;
        int onTime = completedDeliveries.Count(d => d.DeliveryFinishDate - d.DeliveryDate <= maxRange);
        int late = completedDeliveries.Count - onTime;

        // 3. Active Order
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

    //==================== Periodic Updates ===================\\

    #region PeriodicUpdates

    /// <summary>
    /// Performs periodic updates for couriers (e.g., setting them inactive if idle for too long).
    /// </summary>
    internal static void PeriodicCouriersUpdates(DateTime oldClock, DateTime newClock)
    {
        try
        {
            var activeCouriers = s_dal.Courier.ReadAll(c => c.CourierEnabled).ToList();
            foreach (var courier in activeCouriers)
            {
                UpdateCourierActivityByClosedDeliveries(courier, newClock);
            }
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
            // Find last closed delivery
            var lastClosedDelivery = s_dal.Delivery
                .ReadAll(d => d.CourierId == courier.CourierId && d.DeliveryFinishType != DO.DeliveryFinishType.None)
                .OrderByDescending(d => d.DeliveryFinishDate)
                .FirstOrDefault();

            if (lastClosedDelivery is null) return;

            // Check time passed
            var config = AdminManager.GetConfig();
            var timePassed = currentClock - lastClosedDelivery.DeliveryFinishDate;
            bool stillActive = timePassed < config.UnactiveTimeRnge;

            // Update if status changed
            if (courier.CourierEnabled != stillActive)
            {
                var updatedCourier = courier with { CourierEnabled = stillActive };
                s_dal.Courier.Update(updatedCourier);
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