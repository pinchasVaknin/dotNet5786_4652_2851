namespace PL;
using System.Collections;


internal class VehicleTypeCollection : IEnumerable
{
    // We retrieve all values from the BO.VehicleType Enum
    static readonly IEnumerable<BO.VehicleType> s_enums =
        (Enum.GetValues(typeof(BO.VehicleType)) as IEnumerable<BO.VehicleType>)!;

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}


//internal class OrderStatusCollection : IEnumerable
//{
//    // We retrieve all values from the BO.OrderInListFilterBy Enum
//    static readonly IEnumerable<BO.OrderInListFilterBy> s_enums =
//        (Enum.GetValues(typeof(BO.OrderInListFilterBy)) as IEnumerable<BO.OrderInListFilterBy>)!;

//    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
//}