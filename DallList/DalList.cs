namespace Dal;
using DalApi;
using DO;

//==================== Main List DAL Implementation ===================\\

/// <summary>
/// Main entry point for the List-based (In-Memory) Data Access Layer.
/// Implements the IDal interface using internal static lists for storage.
/// </summary>
sealed internal class DalList : IDal
{
    //==================== Singleton Pattern ===================\\

    #region Singleton

    // Private constructor to prevent external instantiation
    private DalList() { }

    // Lazy initialization for thread safety
    private static readonly Lazy<DalList> s_instance =
        new Lazy<DalList>(() => new DalList(), true);

    /// <summary>
    /// Gets the singleton instance of the DalList class.
    /// </summary>
    public static IDal Instance => s_instance.Value;

    #endregion Singleton

    //==================== Entity Implementations ===================\\

    #region Entities

    /// <summary>
    /// Gets the courier service implementation used for handling delivery operations.
    /// </summary>
    public ICourier Courier { get; } = new CourierImplementation();

    /// <summary>
    /// Gets the current order service implementation.
    /// </summary>
    public IOrder Order { get; } = new OrderImplementation();

    /// <summary>
    /// Gets the delivery service implementation.
    /// </summary>
    public IDelivery Delivery { get; } = new DeliveryImplementation();

    #endregion Entities

    //==================== System & Config ===================\\

    #region SystemAndConfig

    /// <summary>
    /// Gets the configuration settings for the application.
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