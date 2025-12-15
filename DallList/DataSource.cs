namespace Dal;

//==================== Data Source (In-Memory) ===================\\

/// <summary>
/// Represents the internal data storage for the DalList layer.
/// Contains static lists acting as tables for Couriers, Orders, and Deliveries.
/// </summary>
internal static class DataSource
{
    //==================== Data Lists ===================\\

    #region DataLists

    // Collection of all Courier entities.
    internal static List<DO.Courier> Couriers { get; } = new();

    // Collection of all Delivery entities.
    internal static List<DO.Delivery> Deliverys { get; } = new();

    // Collection of all Order entities.
    internal static List<DO.Order> Orders { get; } = new();

    #endregion DataLists

}