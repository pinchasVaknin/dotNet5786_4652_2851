namespace Helpers;
using DalApi;
using System.Collections.Generic;

internal static class DeliveryManager
{
    private static IDal s_dal = Factory.Get;

    internal static List<BO.DeliveryPerOrderInList> BuildDeliveryPerOrderInList(DO.Order doOrder)
    {

        var deliveries = s_dal.Delivery.ReadAll(d => d.OrderId == doOrder.OrderId);

        var query =
            from delivery in deliveries

            let courier = CourierManager.GetCourier(delivery.CourierId)

            select new BO.DeliveryPerOrderInList
            {
                DeliveryId = delivery.DeliveryId,
                CourierId = delivery.CourierId,
                CourierFullName = courier.CourierFullName,
                ShipmentType = (BO.ShipmentType)delivery.ShipmentType,
                StartDeliveryDate = delivery.DeliveryDate,
                DeliveryFinishType = (BO.DeliveryFinishType)delivery.DeliveryFinishType,
                FinishDeliveryTime = delivery.DeliveryFinishDate
            };
        return query.ToList();
    }

    internal static List<BO.ClosedDeliveryInList> BuildClosedDeliveryInList()
    {
        var deliveries = s_dal.Delivery.ReadAll(d => d.DeliveryFinishType != DO.DeliveryFinishType.None);

        var query =
            from delivery in deliveries

            let Order = OrderManager.GetOrder(delivery.OrderId)

            let thisCurier = CourierManager.GetCourier(delivery.CourierId)

            let config = AdminManager.GetConfig()

            let totalHandleTime = delivery.DeliveryFinishDate - delivery.DeliveryDate

            let actualDistance = Tools.GetActualDistanceAsync(
                Order.OrderLatitude,
                Order.OrderLongitude,
                config.Latitude,
                config.Longitude,
                (DO.CourierVehicleType)thisCurier.VehicleType).GetAwaiter().GetResult()

            select new BO.ClosedDeliveryInList
            {
                DeliveryId = delivery.DeliveryId,
                OrderId = delivery.OrderId,
                TypeOfOrder = (BO.TypeOfOrder)Order.TypeOfOrder,
                OrderAddress = Order.OrderAddress,
                ShipmentType = (BO.ShipmentType)delivery.ShipmentType,
                ActualDistance = actualDistance,
                TotalHandleTime = totalHandleTime,
                DeliveryFinishType = (BO.DeliveryFinishType)delivery.DeliveryFinishType
            };
        return query.ToList();
    }
    
}
