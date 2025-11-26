namespace Dal;
using DalApi;
using DO;

/// <summary>
/// build and resets
/// </summary>
sealed internal class DalList : IDal

{   //------ Lazy intialization + Thread safe ------\\
    private static readonly Lazy<DalList> s_intance = 
        new Lazy<DalList>(()=> new DalList() , true);

    // publc access to the instance
    public static IDal Instance => s_intance.Value;

    private DalList() { }

    public ICourier Courier { get; } = new CourierImplementation();
    public IOrder Order { get; } = new OrderImplementation();
    public IDelivery Delivery { get; } = new DeliveryImplementation();
    public IConfig Config { get; } = new ConfigImplementation();

    public void ResetDB()
    {
        Courier.DeleteAll();
        Order.DeleteAll();
        Delivery.DeleteAll();
        Config.Reset();
    }
}
