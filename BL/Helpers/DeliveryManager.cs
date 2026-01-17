namespace Helpers;

using DalApi;
using System;
using System.Collections.Generic;
using System.Linq;

//==================== Delivery Business Logic Manager ===================\\

/// <summary>
/// Manages logical operations for Deliveries.
/// Primarily handles the construction of delivery lists and reports for Orders and Admin views.
/// </summary>
internal static class DeliveryManager
{

    //==================== Observer Manager (Stage 5) ===================\\

    #region ObserverManager

    internal static ObserverManager Observers = new(); //stage 5

    #endregion ObserverManager

    //==================== DAL Access ===================\\

    #region DalAccess

    private static IDal s_dal = Factory.Get;

    #endregion DalAccess

    //==================== List Builders ===================\\

    #region ListBuilders

    /// <summary>
    /// Builds a history list of all delivery attempts for a specific order.
    /// </summary>
    /// <param name="doOrder">The Data Object of the order.</param>
    /// <returns>A list of <see cref="BO.DeliveryPerOrderInList"/> representing the delivery history.</returns>
    internal static List<BO.DeliveryPerOrderInList> BuildDeliveryPerOrderInList(DO.Order doOrder)
    {
        // Get all deliveries associated with this order ID
        var deliveries = s_dal.Delivery.ReadAll(d => d.OrderId == doOrder.OrderId);

        var query =
            from delivery in deliveries
                // Try to find the courier details
            let doCourier = s_dal.Courier.Read(delivery.CourierId)

            select new BO.DeliveryPerOrderInList
            {
                DeliveryId = delivery.DeliveryId,
                CourierId = delivery.CourierId,

                // Handle display name: if ID is 0 it's System/Admin, otherwise use Courier Name or "Unknown"
                CourierFullName = (delivery.CourierId == 0)
                    ? "System/Admin"
                    : (doCourier?.CourierFullName ?? "Unknown Courier"),

                ShipmentType = (BO.ShipmentType)delivery.ShipmentType,
                StartDeliveryDate = delivery.DeliveryDate,
                DeliveryFinishType = delivery.DeliveryFinishType.HasValue ?
                                        (BO.DeliveryFinishType)delivery.DeliveryFinishType.Value : null,
                FinishDeliveryTime = delivery.DeliveryFinishDate
            };

        return query.ToList();
    }

    /// <summary>
    /// Builds a report list of all closed deliveries (Completed, Failed, etc.).
    /// Calculates actual distances and handling times.
    /// </summary>
    /// <returns>A list of <see cref="BO.ClosedDeliveryInList"/>.</returns>
    internal static List<BO.ClosedDeliveryInList> BuildClosedDeliveryInList()
    {
        // Filter only deliveries that have a finish type
        var deliveries = s_dal.Delivery.ReadAll(d => d.DeliveryFinishType != null);
        var config = AdminManager.GetConfig();

        var query =
            from delivery in deliveries

                // Retrieve related Order entity
            let order = s_dal.Order.Read(delivery.OrderId)

            // Retrieve related Courier entity
            let thisCourier = (delivery.CourierId == 0) ? null : s_dal.Courier.Read(delivery.CourierId)

            // Ensure both Order and Courier exist
            where order != null

            // Determine vehicle type, defaulting to Car if courier is null
            let vehicle = thisCourier?.CourierVehicleType ?? DO.CourierVehicleType.Car

            // Calculate total time taken
            let totalHandleTime = (delivery.DeliveryFinishDate ?? delivery.DeliveryDate) - delivery.DeliveryDate

            select new BO.ClosedDeliveryInList
            {
                DeliveryId = delivery.DeliveryId,
                OrderId = delivery.OrderId,
                TypeOfOrder = (BO.TypeOfOrder)order.TypeOfOrder,
                OrderAddress = order.OrderAddress,
                ShipmentType = (BO.ShipmentType)delivery.ShipmentType,
                ActualDistance = delivery.ActualDistance,
                TotalHandleTime = totalHandleTime,
                DeliveryFinishType = (BO.DeliveryFinishType)delivery.DeliveryFinishType!.Value
            };

        return query.ToList();
    }

    #endregion ListBuilders

}