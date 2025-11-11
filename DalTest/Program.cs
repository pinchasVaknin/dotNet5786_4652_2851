namespace DalTest;
using Dal;
using DalApi;
using DO;

internal class Program
{

    private static ICourier? s_dalCourier = new CourierImplementation(); //stage 1
    private static IDelivery? s_dalDelivery = new DeliveryImplementation(); //stage 1
    private static IOrder? s_dalOrder = new OrderImplementation(); //stage 1
    private static IConfig? s_dalConfig = new ConfigImplementation(); //stage 1

    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("DAL Test starter:");
            Console.WriteLine($"Courier DAL:  {s_dalCourier.GetType().FullName}");
            Console.WriteLine($"Order DAL:    {s_dalOrder.GetType().FullName}");
            Console.WriteLine($"Delivery DAL: {s_dalDelivery.GetType().FullName}");
            Console.WriteLine($"Config DAL:   {s_dalConfig.GetType().FullName}");

            // Example quick sanity check (safe: only call if implementations exist)
            // You can uncomment / adapt the following once DO types/constructors are aligned:
            // s_dalConfig.Reset();
            // var couriers = s_dalCourier.ReadAll();
            // var orders = s_dalOrder.ReadAll();
            // var deliveries = s_dalDelivery.ReadAll();
            //
            // Console.WriteLine($"Counts => Couriers: {couriers.Count}, Orders: {orders.Count}, Deliveries: {deliveries.Count}");
        

        }


        catch (Exception msg)
        {
            Console.WriteLine(msg);
        }


    }
}

