namespace Dal;
using DalApi;
using DO;

/// <summary>
/// build and resets
/// </summary>
sealed internal class DalList : IDal 
{
    // private constructor to prevent external instantiation
    private DalList() { }

    //------ Lazy intialization + Thread safe ------\\
    private static readonly Lazy<DalList> s_intance = 
        new Lazy<DalList>(()=> new DalList() , true);

    // publc access to the instance
    public static IDal Instance  => s_intance.Value;

    /// <summary>
    /// Gets the courier service implementation used for handling delivery operations.
    /// </summary>
    public ICourier Courier { get; } = new CourierImplementation();

    /// <summary>
    /// Gets the current order instance.
    /// </summary>
    public IOrder Order { get; } = new OrderImplementation();

    /// <summary>
    /// Gets the delivery service implementation.
    /// </summary>
    public IDelivery Delivery { get; } = new DeliveryImplementation();

    /// <summary>
    /// Gets the configuration settings for the application.
    /// </summary>
    public IConfig Config { get; } = new ConfigImplementation();

    /// <summary>
    /// Resets the database by deleting all records and restoring default configurations.
    /// </summary>
    /// <remarks>
    /// This method deletes all entries from the Courier, Order, and Delivery tables, and resets the
    /// configuration settings to their default values.  Use with caution as this operation is irreversible and will
    /// result in the loss of all current data.
    /// </remarks>
    public void ResetDB()
    {
        Courier.DeleteAll();
        Order.DeleteAll();
        Delivery.DeleteAll();
        Config.Reset();
    }
}
