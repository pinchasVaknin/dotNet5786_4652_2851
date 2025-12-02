namespace Helpers;

using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

internal static class CourierManager
{
    private static IDal s_dal = Factory.Get;

    /// <summary>
    /// Saves the specified courier to the data store.
    /// </summary>
    /// <remarks>If the <paramref name="courier"/> has an ID of 0, a new courier record is created. Otherwise,
    /// the existing courier record is updated.</remarks>
    /// <param name="courier">The courier object to be saved. Must not be null.</param>
    /// <exception cref="Exception">Thrown if the operation fails to save the courier.</exception>
    internal static void SaveCourier(BO.Courier courier) // create or update courier
    {
        try
        {
            // Map the business object courier to a data object courier
            DO.Courier doCourier = ConvertBoToDoCourier(courier);

            // Create or update the courier in the data access layer
            var existing = s_dal.Courier.Read(c => c.CourierId == courier.CourierId);

            if (existing is null)
                s_dal.Courier.Create(doCourier);
            else
                s_dal.Courier.Update(doCourier);
        }
        catch (DalAlreadyExistsException ex)
        {
            // Wrap and rethrow any exceptions that occur during the save operation
            throw new Exception("Failed to save courier", ex);
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            // Wrap and rethrow any exceptions that occur during the save operation
            throw new Exception("Failed to save courier", ex);
        }
        catch (DalDoesNotExistException ex)
        {
            // Wrap and rethrow any exceptions that occur during the save operation
            throw new Exception("Failed to save courier", ex);
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
    internal static IEnumerable<BO.CourierInList> GetCouriers() // read all couriers - query syntax
    {
        try
        {
            // Maximum allowed delivery time range from configuration
            var maxRange = s_dal.Config.MaxDelTimeRnge;

            // Read all couriers and deliveries from the data access layer
            var allCouriers = s_dal.Courier.ReadAll();
            var allDeliveries = s_dal.Delivery.ReadAll();

            // LINQ query syntax to join couriers with their deliveries and calculate statistics
            var query =
                from c in allCouriers
                join d in allDeliveries
                    on c.CourierId equals d.CourierId into deliveriesGroup
                let completed =
                    from d in deliveriesGroup
                    where d.DeliveryFinishType == DO.DeliveryFinishType.Completed
                    select d
                let inTime =
                    completed.Count(d => d.DeliveryFinishDate - d.DeliveryDate <= maxRange)
                let overTime =
                    completed.Count() - inTime
                let orderInHandle =
                    (from d in deliveriesGroup
                     where d.DeliveryFinishType == DO.DeliveryFinishType.None
                     select d.OrderId).FirstOrDefault()
                orderby c.CourierFullName
                // Projecting the results into BO.CourierInList objects
                select new BO.CourierInList
                {
                    CourierId = c.CourierId,
                    CourierFullName = c.CourierFullName,
                    CourierIsActive = c.CourierEnabled,
                    VehicleType = (BO.VehicleType)c.CourierVehicleType,
                    StartWorkDate = c.SeniorityOfCourier,
                    DeliveriesInTime = inTime,
                    DeliveriesOverTime = overTime,
                    OrderIdInHandle = orderInHandle
                };

            return query.ToList();
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new Exception("Failed to load couriers list (query syntax)", ex);
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
        catch (DalDoesNotExistException ex)
        {
            throw new Exception("Failed to delete courier", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to delete courier", ex);
        }
        
    }


    //-------------- Private Convert Methods ----------------\\

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
        BO.OrderInProgress? orderInProgress = null;

        // If there is an active delivery, retrieve the associated order and build the order in progress
        if (activeDelivery is not null)
        {
            var doOrder = s_dal.Order.Read(activeDelivery.OrderId)
                ?? throw new Exception($"Order {activeDelivery.OrderId} for courier {doCourier.CourierId} not found");

            // Build the order in progress using the OrderManager
            orderInProgress = OrderManager.BuildOrderInProgress(doOrder, activeDelivery);
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
}
