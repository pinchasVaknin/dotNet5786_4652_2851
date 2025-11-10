namespace DalTest;
using DalApi;
using DO;
using System;

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

        string[] courierNames =
        {
            "David Cohen", "Sarah Levi", "Moshe Israeli", "Rachel Mizrachi", "Yossi Peretz", "Tamar Avi",
            "Avi Shapiro", "Noa Ben-David", "Eitan Katz", "Maya Friedman", "Eli Goldberg", "Shira Rosenberg",
            "Dan Biton", "Tal Azoulay", "Ron Malka", "Michal Dahan", "Amir Ohana", "Yael Gabay", "Gil Aharoni",
            "Nir Ben-Ami", "Hadas Levy", "Itai Baruch", "Chen Mizrahi", "Liora Sabag", "Oren Edri", "Ayelet Saban",
            "Uri Shahar", "Roni Kadosh", "Dani Eliyahu", "Shani Ben-Haim"
        };

        string[] streets =
    {
        "Jaffa", "King George", "Ben Yehuda", "Aza", "Herzl",
        "Hillel", "Agripas", "Jabotinsky", "Begin", "Bialik"
    };

        foreach (string name in courierNames)
        {
            //
            int id;
            do
                id = s_rand.Next(MIN_COURIER_ID, MAX_COURIER_ID);
            while (s_dalCourier!.Read(id) != null);

            //
            string email = name.Trim().ToLower().Replace(" ", ".").Replace("’", "") + "@courier.com";
            string phone = "05" + s_rand.Next(0, 9) + "-" + s_rand.Next(1000000, 9999999);
            bool isActive = s_rand.Next(0, 100) < 85;

            //
            string password = "pw" + s_rand.Next(1000, 9999) + "!" + (char)s_rand.Next('A', 'Z' + 1);
            string address = $"{streets[s_rand.Next(streets.Length)]}, St {s_rand.Next(1, 200)}";

            //
            double maxWeight = s_rand.Next(1, 4) * 5.0; // משקל מקסימלי: 5, 10, או 15 ק"ג

            DO.courierVehicleType courierVehicleType = (DO.courierVehicleType)s_rand.Next(0, 3); // סוג רכב

            // תאריך התחלת עבודה אקראי בשלוש האחרונות            
            DateTime startBase = new DateTime(s_dalConfig!.Clock.Year - 3, 1, 1);
            int range = (s_dalConfig.Clock - startBase).Days;
            DateTime employmentStartTime = startBase.AddDays(s_rand.Next(range));

            var ways = Enum.GetValues<courierVehicleType>();
            var way = ways[s_rand.Next(ways.Length)];

            double? maxDist = (s_rand.NextDouble() < 0.6) ? way switch
            {
                courierVehicleType.Car => 10 + s_rand.NextDouble() * (35 - 10),
                courierVehicleType.Motorcycle => 8 + s_rand.NextDouble() * (25 - 8),
                courierVehicleType.Bicycle => 3 + s_rand.NextDouble() * (12 - 3),
                courierVehicleType.Foot => 1 + s_rand.NextDouble() * (4 - 1),
                _ => (double?)null
            } : null;

            DO.ShipmentType[] types = Enum.GetValues<DO.ShipmentType>();
            DO.ShipmentType preferredType = types[s_rand.Next(types.Length)];

            s_dalCourier!.Create(new Courier(
             courierId: id,
             courierFullName: name,
             courierCellPhone: phone,
             courierEmail: email,
             courierPassword: password,
             courierEnabled: isActive,
             maxCourierDistance: maxDist,
             seniorityOfCourier: employmentStartTime,
             courierVehicleType: way
          ));
        }
    }

    private static void createOrders()
    {
    }
   
    private static void createConfig()
    { }















































































































































































































































































































































}
