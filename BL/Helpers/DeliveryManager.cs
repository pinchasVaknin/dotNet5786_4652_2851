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
        IEnumerable<DO.Delivery> deliveries;
        IEnumerable<DO.Courier> allCouriers;

        lock (AdminManager.BlMutex) //stage 7
        {
            // Get all deliveries associated with this order ID
            deliveries = s_dal.Delivery.ReadAll(d => d.OrderId == doOrder.OrderId).ToList();

            // Get all couriers as a List
            allCouriers = s_dal.Courier.ReadAll().ToList();
        }

        var query =
            from delivery in deliveries

                // Find courier in the list using LINQ
            let doCourier = allCouriers.FirstOrDefault(c => c.CourierId == delivery.CourierId)

            select new BO.DeliveryPerOrderInList
            {
                DeliveryId = delivery.DeliveryId,
                CourierId = delivery.CourierId,

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
        IEnumerable<DO.Delivery> deliveries;
        IEnumerable<DO.Order> allOrders;
        IEnumerable<DO.Courier> allCouriers;

        lock (AdminManager.BlMutex) //stage 7
        {
            // Filter only deliveries that have a finish type
            deliveries = s_dal.Delivery.ReadAll(d => d.DeliveryFinishType != null).ToList();

            // Fetch all orders and couriers as Lists
            allOrders = s_dal.Order.ReadAll().ToList();
            allCouriers = s_dal.Courier.ReadAll().ToList();
        }

        var query =
            from delivery in deliveries

                // Find related Order in the list
            let order = allOrders.FirstOrDefault(o => o.OrderId == delivery.OrderId)

            // Find related Courier in the list
            let thisCourier = (delivery.CourierId == 0) ? null : allCouriers.FirstOrDefault(c => c.CourierId == delivery.CourierId)

            where order != null

            // Calculate total time taken
            let totalHandleTime = (delivery.DeliveryFinishDate ?? delivery.DeliveryDate) - delivery.DeliveryDate

            select new BO.ClosedDeliveryInList
            {
                DeliveryId = delivery.DeliveryId,
                OrderId = delivery.OrderId,
                TypeOfOrder = (BO.TypeOfOrder)order.TypeOfOrder, // No need for .Value if order is class
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