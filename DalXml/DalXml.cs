namespace Dal;
using DalApi;

/// <summary>
/// Provides access to data operations for couriers, orders, deliveries, and configuration settings using XML storage.
/// </summary>
sealed public class DalXml : IDal
{
    /// <summary>
    /// Provides CRUD operations for couriers backed by the XML store.
    /// </summary>
    public ICourier Courier { get; } = new CourierImplementation();

    /// <summary>
    /// Provides CRUD operations for orders backed by the XML store.
    /// </summary>
    public IOrder Order { get; } = new OrderImplementation();

    /// <summary>
    /// Provides CRUD operations for deliveries backed by the XML store.
    /// </summary>
    public IDelivery Delivery { get; } = new DeliveryImplementation();

    /// <summary>
    /// Provides access to configuration settings and utilities.
    /// </summary>
    public IConfig Config { get; } = new ConfigImplementation();

    //--------------- Reset function => Courier/Order/Delivery/Config ---------------\\
    /// <summary>
    /// Removes all stored couriers, orders and deliveries, and resets configuration values to defaults.
    /// </summary>
    public void ResetDB()
    {
        Courier.DeleteAll();
        Order.DeleteAll();
        Delivery.DeleteAll();
        Config.Reset();
    }
}
