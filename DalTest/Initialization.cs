namespace DalTest;
using DalApi;
using DO;


public static class Initialization
{
    private static ICourier? s_dalCourier; // stage 1
    private static IOrder? s_dalOrder; // stage 1
    private static IDelivery? s_dalDelivery; // stage 1
    private static IConfig? s_dalConfig; // stage 1

    const int MIN_COURIER_ID = 200000000;
    const int MAX_COURIER_ID = 400000000;

    private static readonly Random s_rand = new();

    private static void createCouriers()
    {
        
        string[] courierNames = { "David Cohen", "Sarah Levi", "Moshe Israeli",
                              "Rachel Mizrachi", "Yossi Peretz", "Tamar Avi",
                              "Avi Shapiro", "Noa Ben-David", "Eitan Katz",
                              "Maya Friedman", "Eli Goldberg", "Shira Rosenberg",
                              "Dan Biton", "Tal Azoulay", "Ron Malka",
                              "Michal Dahan", "Amir Ohana", "Yael Gabay",
                              "Gil Aharoni", "Nir Ben-Ami", "Hadas Levy",
                              "Itai Baruch", "Chen Mizrahi", "Liora Sabag",
                              "Oren Edri", "Ayelet Saban", "Uri Shahar",
                              "Roni Kadosh", "Dani Eliyahu", "Shani Ben-Haim" };

        foreach (var name in courierNames)
        {
            int id;
            do
                id = s_rand.Next(MIN_COURIER_ID, MAX_COURIER_ID);
            while (s_dalCourier!.Read(id) != null);

            string email = name.Replace(" ", ".").ToLower() + "@courier.com";
            string phone = "05" + s_rand.Next(0, 9) + "-" + s_rand.Next(1000000, 9999999);

            bool isActive = s_rand.Next(0, 2) == 1;
            double maxWeight = s_rand.Next(1, 4) * 5.0; // משקל מקסימלי: 5, 10, או 15 ק"ג

            DO.VehicleType vehicleType = (DO.VehicleType)s_rand.Next(0, 3); // רכב, אופניים, או אופנוע

            // תאריך התחלת עבודה אקראי בשנתיים האחרונות
            DateTime startDate = new DateTime(2023, 1, 1);
            DateTime startWork = startDate.AddDays(s_rand.Next((s_dalConfig!.Clock - startDate).Days));

            s_dalCourier!.Create(new(id, name, email, phone, isActive, maxWeight, vehicleType, startWork));
        }
    }
    private static void createOrders()
    {
    }
    private static void createDeliverys() 
    { }
    private static void createConfig() 
    { }


}
