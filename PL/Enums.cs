namespace PL;
using System.Collections;

//==================== Courier Filters ===================\\

#region CourierFilters

internal class CourierInListFilterByCollection : IEnumerable
{
    // We retrieve all values from the BO.CourierInListFilterBy Enum
    static readonly IEnumerable<BO.CourierInListFilterBy> s_enums =
        (Enum.GetValues(typeof(BO.CourierInListFilterBy)) as IEnumerable<BO.CourierInListFilterBy>)!;
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

internal class VehicleTypeCollection : IEnumerable
{
    // We retrieve all values from the BO.VehicleType Enum
    static readonly IEnumerable<BO.VehicleType> s_enums =
        (Enum.GetValues(typeof(BO.VehicleType)) as IEnumerable<BO.VehicleType>)!;

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

#endregion CourierFilters

//==================== Order Filters ===================\\

#region OrderFilters

internal class OrderInListFilterByCollection : IEnumerable
{
    // We retrieve all values from the BO.OrderInListFilterBy Enum
    static readonly IEnumerable<BO.OrderInListFilterBy> s_enums =
        (Enum.GetValues(typeof(BO.OrderInListFilterBy)) as IEnumerable<BO.OrderInListFilterBy>)!;
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

internal class TypeOfOrderCollection : IEnumerable
{
    // We retrieve all values from the BO.TypeOfOrder Enum
    static readonly IEnumerable<BO.TypeOfOrder> s_enums =
        (Enum.GetValues(typeof(BO.TypeOfOrder)) as IEnumerable<BO.TypeOfOrder>)!;
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

internal class OrderStatusCollection : IEnumerable
{
    // We retrieve all values from the BO.OrderStatus Enum
    static readonly IEnumerable<BO.OrderStatus> s_enums =
        (Enum.GetValues(typeof(BO.OrderStatus)) as IEnumerable<BO.OrderStatus>)!;
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

internal class ScheduleStatusCollection : IEnumerable
{
    // We retrieve all values from the BO.ScheduleStatus Enum
    static readonly IEnumerable<BO.ScheduleStatus> s_enums =
        (Enum.GetValues(typeof(BO.ScheduleStatus)) as IEnumerable<BO.ScheduleStatus>)!;
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

#endregion OrderFilters
