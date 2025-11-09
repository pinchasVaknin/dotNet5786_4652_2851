namespace DO;

public record Order
(
    int OrderId,//need to be run number
    
    string? orderDetail,
    string orderAdress,
    double orderLatitude,
    double orderLongitude,
    string orderCostumerFullName,
    string orderCostumerPhone,
    double orderWeight,
    bool fragile,
    double orderSize,
    DateTime orderDate,
    typeOfOrder typeOfOrder
);
