using DalApi;
using DO;
using Helpers;

internal static IEnumerable<BO.OrderInList> GetOrders(BO.OrderInListFilterBy? filterField = null, object? filterValue = null, BO.OrderInListSortBy? sortBy = null) // read all Orders - query syntax + filter/sort
{
    try
    {
        // Maximum allowed delivery time range from configuration
        var maxRange = s_dal.Config.MaxDelTimeRnge;
        var maxRangeWithoutRisk = maxRange - s_dal.Config.RiskTimeRnge;

        var allOrders = s_dal.Order.ReadAll();
        var allDeliveries = s_dal.Delivery.ReadAll();

        var query =
            from o in allOrders
            join d in allDeliveries
                on o.OrderId equals d.OrderId into deliveriesGroup

            let lastDelivery =
                deliveriesGroup.OrderByDescending(del => del.DeliveryDate).FirstOrDefault()

            let AirDistance =
                Tools.DistanceKm(o.OrderLatitude, o.OrderLongitude, 31.7479, 35.188)

            let OrderStatus =
                lastDelivery is null ?
                    BO.OrderStatus.Open :
                lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.None ?
                    BO.OrderStatus.InProgress :
                lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.Completed ?
                    BO.OrderStatus.Supplied :
                lastDelivery.DeliveryFinishType == DO.DeliveryFinishType.Cancelled ?
                    BO.OrderStatus.Canceled :
                BO.OrderStatus.Refused

            let ScheduleStatus =
                Tools.CalcScheduleStatus(o.OrderDate, s_dal.Config.Clock, lastDelivery?.DeliveryFinishDate,
                                         maxRangeWithoutRisk, maxRange)

            let TimeLeftToFinish =
                    lastDelivery is null || (lastDelivery.DeliveryDate + maxRange) < s_dal.Config.Clock ?
                        TimeSpan.Zero :
                    (lastDelivery.DeliveryDate + maxRange) - s_dal.Config.Clock

            let TotalHandleTime =
                (from del in deliveriesGroup
                 where del.DeliveryFinishType == DO.DeliveryFinishType.Completed
                 select del.DeliveryFinishDate - o.OrderDate)
                    .Aggregate(TimeSpan.Zero, (acc, span) => acc + span)

            let TotalDeliveries =
                deliveriesGroup.Count()

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

        // materialize
        var list = query.ToList();

        // Filtering:
        // If filterField is provided AND filterValue is not null, apply equality filter.
        if (filterField.HasValue && filterValue is not null)
        {
            switch (filterField.Value)
            {
                case BO.OrderInListFilterBy.OrderId:
                    if (int.TryParse(Convert.ToString(filterValue), out var id))
                        list = list.Where(x => x.OrderId == id).ToList();
                    break;

                case BO.OrderInListFilterBy.TypeOfOrder:
                    {
                        if (TryConvertEnum(filterValue, out BO.TypeOfOrder typeVal))
                            list = list.Where(x => x.TypeOfOrder == typeVal).ToList();
                    }
                    break;

                case BO.OrderInListFilterBy.OrderStatus:
                    {
                        if (TryConvertEnum(filterValue, out BO.OrderStatus statusVal))
                            list = list.Where(x => x.OrderStatus == statusVal).ToList();
                    }
                    break;

                case BO.OrderInListFilterBy.ScheduleStatus:
                    {
                        if (TryConvertEnum(filterValue, out BO.ScheduleStatus schedVal))
                            list = list.Where(x => x.ScheduleStatus == schedVal).ToList();
                    }
                    break;

                default:
                    break;
            }
        }

        // Sorting:
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
    catch (DalXMLFileLoadCreateException ex)
    {
        throw new Exception("Failed to load orders list (query syntax)", ex);
    }
}

internal static void DeleteOrders(int id)
{
    try
    {
        // Check that order exists
        DO.Order? doOrder = s_dal.Order.Read(id)
            ?? throw new Exception($"Order with ID={id} does not exist");

        // Check if order has an active delivery
        bool hasActiveDelivery = s_dal.Delivery
            .ReadAll(d => d.OrderId == id)
            .Any(d => d.DeliveryFinishType == DO.DeliveryFinishType.None);

        if (hasActiveDelivery)
            throw new Exception($"Cannot delete order {id}: courier is on way with delivery.");

        // Perform deletion
        s_dal.Order.Delete(id);
    }
    catch (DalDoesNotExistException ex)
    {
        throw new Exception("Failed to delete order", ex);
    }
    catch (Exception ex)
    {
        throw new Exception("Failed to delete order", ex);
    }

}