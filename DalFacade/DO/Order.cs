namespace DO;

public record Order
(
    int orderId, //need to be run number
    string orderStatus,
    string? orderDetail,
    string orderAddress,
    double orderLatitude,
    double orderLongitude,
    string orderCostumerFullName,
    string orderCostumerPhone,
    double orderWeight,
    bool fragile,
    double orderSize,
    DateTime orderDate,
    typeOfOrder typeOfOrder
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
       typeOfOrder.Smartphone   // default
   )
    { }
}
