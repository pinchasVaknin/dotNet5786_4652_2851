namespace DO;

public record Order
(
    int OrderId, //need to be run number
    string OrderStatus,
    string? OrderDetail,
    string OrderAddress,
    double OrderLatitude,
    double OrderLongitude,
    string OrderCostumerFullName,
    string OrderCostumerPhone,
    double OrderWeight,
    bool IsFragile,
    double OrderSize,
    DateTime OrderDate,
    TypeOfOrder TypeOfOrder
)
{
    public Order() : this(
       0,
       "",
       null,
       "",
       0,
       0,
       "",
       "",
       0,
       false,
       0,
       DateTime.MinValue,
       TypeOfOrder.Smartphone   // default
   )
    { }
}
