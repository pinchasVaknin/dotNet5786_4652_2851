namespace Helpers;

using DalApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

internal static class CourierManager
{
    //======== Data Access Layer Instance ========\\

    private static IDal s_dal = Factory.Get;

    //======== Login and List Retrieval ========\\

    #region Login and List Retrieval

    internal static BO.UserRole GetUserRole(int userId, string password)
    {
        try
        {
            var config = AdminManager.GetConfig();

            if (config.AdminId == userId)
            {
                if (config.AdminPassword == password)
                    return BO.UserRole.Admin;

                else throw new Exception("Incorrect password for admin user");
            }

            var courierUser = s_dal.Courier.Read(userId);

            if (courierUser is not null)
            {
                if (courierUser.CourierPassword == password)
                    return BO.UserRole.Courier;

                else throw new Exception("Incorrect password for courier user");
            }

            throw new Exception("User ID does not exist");

        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new Exception("Failed to get user role", ex);
        }
    }

    internal static IEnumerable<BO.CourierInList> GetListOfCouriers(
        int requesterId,
        bool? isActiveFilter = null,
        BO.CourierListSortBy? sortBy = null)
    {
        try
        {

            var boCouriers =
                from c in s_dal.Courier.ReadAll()
                select GetCourierInList(c.CourierId);

            if (isActiveFilter is bool active)
                boCouriers = boCouriers.Where(c => c.CourierIsActive == active);


            boCouriers = sortBy switch
            {
                BO.CourierListSortBy.CourierFullName =>
                    boCouriers.OrderBy(c => c.CourierFullName),

                BO.CourierListSortBy.CourierIsActive =>
                    boCouriers.OrderBy(c => c.CourierIsActive),

                BO.CourierListSortBy.VehicleType =>
                    boCouriers.OrderBy(c => c.VehicleType),

                BO.CourierListSortBy.StartWorkDate =>
                    boCouriers.OrderBy(c => c.StartWorkDate),

                BO.CourierListSortBy.DeliveriesInTime =>
                    boCouriers.OrderByDescending(c => c.DeliveriesInTime),

                BO.CourierListSortBy.DeliveriesOverTime =>
                    boCouriers.OrderByDescending(c => c.DeliveriesOverTime),

                BO.CourierListSortBy.OrderIdInHandle =>
                    boCouriers.OrderBy(c => c.OrderIdInHandle),

                null or BO.CourierListSortBy.CourierId or _ =>
                    boCouriers.OrderBy(c => c.CourierId),
            };

            return boCouriers.ToList();
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new Exception("Failed to load couriers list", ex);
        }
    }

    /// <summary>
    /// Retrieves a collection of couriers with their delivery statistics,
    /// using LINQ query syntax over all couriers and deliveries.
    /// </summary>
    /// <remarks>
    /// For each courier, this method calculates the number of deliveries completed
    /// within the allowed time range, the number of deliveries completed over time,
    /// and the ID of the current order being handled (if any).
    /// </remarks>
    /// <returns>
    /// An enumerable collection of <see cref="BO.CourierInList"/> objects,
    /// each representing a courier with summary delivery information.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if the couriers list cannot be loaded or if a data access error occurs.
    /// </exception>
    internal static BO.CourierInList GetCourierInList(int courierId) // read all couriers - query syntax
    {
        try
        {

            var doCourier = s_dal.Courier.Read(courierId) ??
                throw new Exception($"Courier with ID={courierId} does not exist");

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
            throw new Exception("Failed to load couriers list (query syntax)", ex);
        }
    }

    #endregion

    //======== Courier Operations ========\\

    #region CRUD Methods

    internal static void AddCourier(BO.Courier courier) // create or update courier
    {
        try
        {
            var existing = s_dal.Courier.Read(courier.CourierId);
            if (existing is not null)
                throw new Exception($"Courier with ID={courier.CourierId} already exists.");

            DO.Courier doCourier = ConvertBoToDoCourier(courier);
            s_dal.Courier.Create(doCourier);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new Exception("Failed to add courier", ex);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new Exception("Failed to add courier", ex);
        }
    }

    /// <summary>
    /// Retrieves a courier by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the courier to retrieve. Must be a valid, existing ID.</param>
    /// <returns>A <see cref="BO.Courier"/> object representing the courier with the specified ID.</returns>
    /// <exception cref="Exception">Thrown if the courier with the specified ID does not exist or if an error occurs while loading the courier.</exception>
    internal static BO.Courier GetCourier(int id) // read courier by id
    {
        try
        {
            // Read the courier from the data access layer
            DO.Courier doCourier = s_dal.Courier.Read(id)
                ?? throw new Exception($"Courier with ID={id} does not exist");

            // Build and return the business object representation of the courier
            return ConvertDoToBoCourier(doCourier);
        }
        catch (Exception ex)
        {
            // Wrap and rethrow any exceptions that occur during the process
            throw new Exception("Failed to load courier", ex);
        }
    }

    internal static void UpdateCourier(BO.Courier courier) // update courier
    {
        try
        {
            // Map the business object courier to a data object courier
            DO.Courier doCourier = ConvertBoToDoCourier(courier);

            s_dal.Courier.Update(doCourier);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            // Wrap and rethrow any exceptions that occur during the save operation
            throw new Exception("Failed to update courier", ex);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            // Wrap and rethrow any exceptions that occur during the save operation
            throw new Exception("Failed to update courier", ex);
        }
    }

    /// <summary>
    /// Deletes a courier by its ID.
    /// </summary>
    /// <param name="id">The ID of the courier to delete.</param>
    /// <exception cref="Exception">
    /// Thrown if the courier does not exist, has an active delivery,
    /// or if a data-access error occurs.
    /// </exception>
    internal static void DeleteCourier(int id)
    {
        try
        {
            // Check that courier exists
            DO.Courier? doCourier = s_dal.Courier.Read(id)
                ?? throw new Exception($"Courier with ID={id} does not exist");

            // Check if courier has an active delivery
            bool hasActiveDelivery = s_dal.Delivery
                .ReadAll(d => d.CourierId == id)
                .Any(d => d.DeliveryFinishType == DO.DeliveryFinishType.None);

            if (hasActiveDelivery)
                throw new Exception($"Cannot delete courier {id}: courier has an active delivery.");

            // Perform deletion
            s_dal.Courier.Delete(id);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new Exception("Failed to delete courier", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to delete courier", ex);
        }

    }

    #endregion

    //======== Private Conversion Methods ========\\

    #region Private Conversion Methods

    /// <summary>
    /// Constructs a business object representation of a courier from the data object representation.
    /// </summary>
    /// <remarks>The method calculates the total number of on-time and late deliveries based on the courier's
    /// completed deliveries. It also determines if there is an active delivery and constructs an order in progress if
    /// applicable.</remarks>
    /// <param name="doCourier">The data object representing the courier, containing details such as ID, name, and contact information.</param>
    /// <returns>A business object <see cref="BO.Courier"/> that includes the courier's details, delivery statistics, and any
    /// active order in progress.</returns>
    /// <exception cref="Exception">Thrown if an active order associated with the courier cannot be found.</exception>
    private static BO.Courier ConvertDoToBoCourier(DO.Courier doCourier) // build BO.Courier from DO.Courier
    {
        // Retrieve all deliveries associated with the courier
        var deliveries = s_dal.Delivery.ReadAll(d => d.CourierId == doCourier.CourierId);

        // Filter completed deliveries
        var completedDeliveries = deliveries
            .Where(d => d.DeliveryFinishType == DO.DeliveryFinishType.Completed)
            .ToList();

        // Maximum allowed delivery time range from configuration
        var maxRange = s_dal.Config.MaxDelTimeRnge;

        // Calculate on-time and late deliveries
        int onTime = completedDeliveries.Count(d => d.DeliveryFinishDate - d.DeliveryDate <= maxRange);

        // Calculate late deliveries
        int late = completedDeliveries.Count - onTime;

        // Find the active delivery (if any)
        var activeDelivery = deliveries.FirstOrDefault(d => d.DeliveryFinishType == DO.DeliveryFinishType.None);

        // Build the order in progress if there is an active delivery
        var orderInProgress = activeDelivery is null ?
                                null :
                                OrderManager.BuildOrderInProgress(s_dal.Order.Read(activeDelivery.OrderId) ??
                                throw new Exception($"Order {activeDelivery.OrderId} for courier {doCourier.CourierId} not found"),
                                activeDelivery);

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
    /// Maps a business object courier to a data object courier.
    /// </summary>
    /// <param name="boCourier">The business object courier.</param>
    /// <returns>The corresponding data object courier.</returns>
    private static DO.Courier ConvertBoToDoCourier(BO.Courier boCourier) =>
        new DO.Courier(
            CourierId: boCourier.CourierId, // 0 for new courier
            CourierFullName: boCourier.CourierFullName,
            CourierCellPhone: boCourier.CourierCellPhone,
            CourierEmail: boCourier.CourierEmail,
            CourierPassword: boCourier.CourierPassword,
            CourierAddress: boCourier.CourierLocation,
            CourierEnabled: boCourier.CourierIsActive,  // bool
            MaxCourierDistance: boCourier.MaxCourierDistance,
            SeniorityOfCourier: boCourier.StartWorkDate,
            CourierVehicleType: (DO.CourierVehicleType)boCourier.VehicleType // enum
        );

    #endregion
}