namespace DalTest;
using Dal;
using DalApi;
using DO;

internal class Program
{

    private static ICourier? s_dalStudent = new CourierImplementation(); //stage 1
    private static IDelivery? s_dalCourse = new DeliveryImplementation(); //stage 1
    private static IOrder? s_dalLink = new OrderImplementation(); //stage 1
    private static IConfig? s_dalConfig = new ConfigImplementation(); //stage 1

    static void Main(string[] args)
    {
        try
        {


        }


        catch (Exception msg)
        {
            Console.WriteLine(msg);
        }


    }
}

