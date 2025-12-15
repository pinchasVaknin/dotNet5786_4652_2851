namespace Dal;
using DalApi;
using System.Diagnostics;

//==================== Main XML DAL Implementation ===================\\

/// <summary>
/// Provides access to data operations for couriers, orders, deliveries, and configuration settings using XML storage.
/// Implements the IDal interface.
/// </summary>
sealed internal class DalXml : IDal
{
    //==================== Singleton Pattern ===================\\

    #region Singleton

    // Private constructor to prevent external instantiation
    private DalXml() { }

    // Lazy initialization + Thread safe
    private static readonly Lazy<DalXml> s_instance =
        new Lazy<DalXml>(() => new DalXml(), true);

    /// <summary>
    /// Gets the singleton instance of the DalXml class.
    /// </summary>
    public static IDal Instance => s_instance.Value;

    #endregion Singleton

    //==================== Entity Accessors ===================\\

    #region Entities

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

    #endregion Entities

    //==================== System & Config ===================\\

    #region SystemAndConfig

    /// <summary>
    /// Provides access to configuration settings and utilities.
    /// </summary>
    public IConfig Config { get; } = new ConfigImplementation();

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

    #endregion SystemAndConfig

}