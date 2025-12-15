namespace DalTest;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;

//==================== Data Initialization (Seeding) ===================\\

/// <summary>
/// Static class responsible for initializing the database with dummy data.
/// Used for testing purposes to populate Couriers, Orders, Deliveries, and Configuration.
/// </summary>
public static class Initialization
{
    //==================== Fields & Constants ===================\\

    #region Fields

    // Access to the DAL layer via the Factory
    private static IDal? s_dal;

    // Constants for random ID generation
    const int MIN_COURIER_ID = 200000000;
    const int MAX_COURIER_ID = 400000000;

    // Random generator for data creation
    private static readonly Random s_rand = new();

    #endregion Fields

    //==================== Main Entry Point ===================\\

    #region MainBoot

    /// <summary>
    /// Main boot function to reset the database and seed initial data.
    /// </summary>
    public static void Do()
    {
        // Get the DAL instance from the Factory
        s_dal = DalApi.Factory.Get;

        Console.WriteLine("Resetting configuration and lists...");

        // Reset the database (clears XMLs or Lists depending on implementation)
        s_dal.ResetDB();

        Console.WriteLine("Creating initial data...");

        // Create Data
        createConfig();
        createCouriers();
        createOrders();
        createDeliverys();
    }

    #endregion MainBoot

    //==================== Data Creation Methods ===================\\

    #region DataCreation

    /// <summary>
    /// Generates and stores random Courier entities.
    /// </summary>
    private static void createCouriers()
    {
        // List of courier names to generate demo data
        string[] courierNames =
        {
            "David Cohen", "Sarah Levi", "Moshe Israeli", "Rachel Mizrachi", "Yossi Peretz", "Tamar Avi",
            "Avi Shapiro", "Noa Ben-David", "Eitan Katz", "Maya Friedman", "Eli Goldberg", "Shira Rosenberg",
            "Dan Biton", "Tal Azoulay", "Ron Malka", "Michal Dahan", "Amir Ohana", "Yael Gabay", "Gil Aharoni",
            "Nir Ben-Ami", "Hadas Levy", "Itai Baruch", "Chen Mizrahi", "Liora Sabag", "Oren Edri", "Ayelet Saban",
            "Uri Shahar", "Roni Kadosh", "Dani Eliyahu", "Shani Ben-Haim"
        };

        // Example street names for building courier address
        string[] streets =
        {
            "Jaffa", "King George", "Ben Yehuda", "Aza", "Herzl",
            "Hillel", "Agripas", "Jabotinsky", "Begin", "Bialik"
        };

        foreach (string name in courierNames)
        {
            // Generate unique courier ID
            int id;
            do
            {
                id = s_rand.Next(MIN_COURIER_ID, MAX_COURIER_ID); // Random ID in valid range
            }
            while (s_dal!.Courier.Read(id) != null); // Ensure ID is not taken

            // Generate email and phone based on name + random pattern
            string email = name.Trim().ToLower().Replace(" ", ".").Replace("’", "") + "@courier.com";
            string phone = "05" + s_rand.Next(0, 9) + "-" + s_rand.Next(1000000, 9999999);

            // Random courier activation status (85% chance active)
            bool isActive = s_rand.Next(0, 100) < 85;

            // Generate a simple random password
            string password = "pw" + s_rand.Next(1000, 9999) + "!" + (char)s_rand.Next('A', 'Z' + 1);

            // Pick random street + house number
            string address = $"{streets[s_rand.Next(streets.Length)]}, St {s_rand.Next(1, 200)}";

            // Employment start time randomly chosen sometime over the past 3 years
            DateTime startBase = new DateTime(s_dal!.Config.Clock.Year - 3, 1, 1);
            int range = (s_dal.Config.Clock - startBase).Days;
            DateTime employmentStartTime = startBase.AddDays(s_rand.Next(range));

            // Randomly pick a vehicle type from enum values
            var VehicleTypes = Enum.GetValues<DO.CourierVehicleType>();
            var VehicleType = VehicleTypes[s_rand.Next(VehicleTypes.Length)];

            // Random max travel distance depending on vehicle type (or null sometimes)
            double? MaxDistance = (s_rand.NextDouble() < 0.6) ? VehicleType switch
            {
                DO.CourierVehicleType.Car => 10 + s_rand.NextDouble() * (35 - 10),
                DO.CourierVehicleType.Motorcycle => 8 + s_rand.NextDouble() * (25 - 8),
                DO.CourierVehicleType.Bicycle => 3 + s_rand.NextDouble() * (12 - 3),
                DO.CourierVehicleType.Foot => 1 + s_rand.NextDouble() * (4 - 1),
                _ => null
            } : null;

            // Round the MaxDistance
            if (MaxDistance is not null)
            {
                MaxDistance = Math.Round(MaxDistance.Value, 3);
            }

            // Create and save courier in DAL
            s_dal!.Courier.Create(new DO.Courier(
             CourierId: id,
             CourierFullName: name,
             CourierCellPhone: phone,
             CourierEmail: email,
             CourierPassword: password,
             CourierAddress: address, // Address from which the courier starts work
             CourierEnabled: isActive,
             MaxCourierDistance: MaxDistance,
             SeniorityOfCourier: employmentStartTime,
             CourierVehicleType: VehicleType
         ));
        }
    }

    /// <summary>
    /// Generates and stores random Order entities.
    /// </summary>
    private static void createOrders()
    {
        // Sample pool of customer names to randomly pick from
        string[] customers =
        {
            "Noa Levi","Yossi Cohen","Dana Bar","Itai Mor","Rina Azulay","Gal Shachar",
            "Eden Peri","Liad Shalom","Hila Porat","Ido Tal","Maya Dayan","Nir Omer",
            "Rotem Halevi","Eli Abramov","Yael Menachem","Tal Shani","Omer Golan","Roni Sagi",
            "Shir Avital","Yair Katz","Ofir Shalev","Hadar Sinai","Aviv Dagan","Orly Tamir",
            "Shaked Bar-On","Maor David","Shani Eldar","Roy Daniel","Lihi Bram",
            "Tom Elbaz","Yotam Hen","Liron Shalom","Adi Ravid","Erez Shachar",
            "Hila Arbel","Emanuel Sharon","Sapir Menashe","Yuval Avraham","Bar Shahar",
            "Noga Vardi","Naor Levy","Shachar Shemesh","Or Peleg","Yaara Gold",
            "Efrat Shoshani","Omri Shoham","Karen Levi","Ofek Azulay","Dvir Mizrahi",
            "Meitar Sahar"
        };

        // Real Jerusalem addresses with latitude/longitude tuples
        var addresses = new List<(string orderAddress, double lat, double lon)>
        {
            ("Jaffa 97, Jerusalem",                          31.78849, 35.20412),
            ("King George 3, Jerusalem",                     31.78025, 35.21492),
            ("Ben Yehuda 10, Jerusalem",                     31.78144, 35.21603),
            ("Azza 34, Jerusalem",                           31.77652, 35.20998),
            ("Hillel 18, Jerusalem",                         31.78098, 35.22002),
            ("Agripas 80, Mahane Yehuda",                    31.78364, 35.21413),
            ("Jabotinsky 2, Jerusalem",                      31.77343, 35.21262),
            ("Herzl 90, Jerusalem",                          31.80235, 35.19855),
            ("Malha Mall, Jerusalem",                        31.74788, 35.18803),
            ("Givat Shaul 40, Jerusalem",                    31.78886, 35.18847),
            ("Har Hotzvim, Jerusalem",                       31.80619, 35.20750),
            ("Talpiot, HaUman 17, Jerusalem",                31.74880, 35.22330),
            ("Central Bus Station, Jerusalem",               31.79000, 35.20420),
            ("Shlomtzion HaMalka 6, Jerusalem",              31.77744, 35.22543),
            ("Derech Beit Lehem 112, Jerusalem",             31.75683, 35.21988),
            ("Emek Refaim 21, Jerusalem",                    31.76501, 35.21765),
            ("Yehuda 64, Bakaa, Jerusalem",                  31.75919, 35.22274),
            ("HaPalmach 45, Jerusalem",                      31.77315, 35.21297),
            ("Kiryat HaYovel, Stern 33, Jerusalem",          31.76481, 35.18595),
            ("Bayit Vegan, Shaarei Torah 12, Jerusalem",     31.76577, 35.19064),
            ("Mount Herzl, Jerusalem",                       31.77357, 35.18554),
            ("Ein Kerem, Jerusalem",                         31.76452, 35.14696),
            ("Pisgat Ze'ev Center, Jerusalem",               31.83852, 35.24322),
            ("Neve Yaakov Blvd, Jerusalem",                  31.84455, 35.22750),
            ("Ramat Eshkol, Ussishkin 14, Jerusalem",        31.80587, 35.22674),
            ("French Hill, HaGoren 2, Jerusalem",            31.79274, 35.24134),
            ("Gonen (Katamon), Hovevei Zion 5, Jerusalem",   31.76217, 35.20862),
            ("Rehavia, Ramban 15, Jerusalem",                31.77673, 35.21388),
            ("Talbiya, Dubnov 7, Jerusalem",                 31.77498, 35.21911),
            ("Kiryat Moshe, Herzl Blvd 18, Jerusalem",       31.78735, 35.19877),
            ("Romema, Zmora 9, Jerusalem",                   31.79921, 35.20461),
            ("Gilo Center, Jerusalem",                       31.72718, 35.19039),
            ("Armon HaNatziv, Yanovski 12, Jerusalem",       31.75090, 35.22988),
            ("Shaare Zedek Hospital, Jerusalem",             31.76971, 35.19573),
            ("Hadassah Ein Kerem Hospital, Jerusalem",       31.76318, 35.14852),
            ("Ramot, Golda Meir Blvd 254, Jerusalem",        31.82050, 35.18590),
            ("Mishkenot Shaananim, Jerusalem",               31.77098, 35.22651),
            ("Yemin Moshe, Jerusalem",                       31.77135, 35.22620),
            ("Mamilla Mall, Jerusalem",                      31.77705, 35.22162),
            ("Hebrew University, Mt. Scopus",                31.79460, 35.24423),
            ("Hebrew University, Givat Ram",                 31.77360, 35.20146),
            ("Ein Yael, Jerusalem",                          31.75681, 35.17579),
            ("Zoo Biblical, Jerusalem",                      31.74851, 35.17531),
            ("Tzomet Pat, Jerusalem",                        31.75512, 35.20490),
            ("Ramat Sharet, Hartom 12, Jerusalem",           31.76133, 35.20395),
            ("Malha Railway Station, Jerusalem",             31.75033, 35.19230),
            ("Light Rail — Davidka Station, Jerusalem",      31.78553, 35.21205),
            ("Light Rail — Shimon HaTzadik Station",         31.80226, 35.23514),
            ("Rekhes Shmuel, Jerusalem",                     31.79260, 35.21008),
            ("German Colony, Rachel Imenu 15, Jerusalem",    31.76445, 35.21980),
            ("Baka, Pierre Koenig 36, Jerusalem",            31.75142, 35.22291)
        };

        // Quantity requirements: total >= 50
        int totalOrder = 50;
        int openCount = 20;
        int progCount = 10;
        int closedCount = Math.Max(20, totalOrder - openCount - progCount); // ensure >= 20 closed
        int extra = totalOrder - (openCount + progCount + closedCount);
        openCount += Math.Max(0, extra); // allocate any remainder to OPEN

        // Company coordinates (seeded manually in Config). Defaults are fallbacks.
        double companyLat = s_dal!.Config.Latitude ?? 31.7886;
        double companyLon = s_dal.Config.Longitude ?? 35.2034;

        // All order categories (enum values) + the project simulation clock
        var allKinds = Enum.GetValues<DO.TypeOfOrder>();
        var clock = s_dal.Config.Clock;

        // Build a flat sequence of status tags, then iterate once to create all orders
        var statuses = Enumerable.Repeat("OPEN", openCount)
                .Concat(Enumerable.Repeat("IN_PROGRESS", progCount))
                .Concat(Enumerable.Repeat("CLOSED", closedCount));

        foreach (var statusTag in statuses)
        {
            // Random address tuple (address string + lat/lon)
            var addr = addresses[s_rand.Next(addresses.Count)];
            // Random category
            var kind = allKinds[s_rand.Next(allKinds.Length)];

            // Compact electronics catalog: (category enum, array of product names)
            // Assumes DO.Catalog.* definitions exist
            string[] items = kind switch
            {
                DO.TypeOfOrder.Smartphone => Enum.GetNames<DO.Catalog.SmartphoneDetails>(),
                DO.TypeOfOrder.Laptop => Enum.GetNames<DO.Catalog.LaptopDetails>(),
                DO.TypeOfOrder.Tablet => Enum.GetNames<DO.Catalog.TabletDetails>(),
                DO.TypeOfOrder.TV => Enum.GetNames<DO.Catalog.TVDetails>(),
                DO.TypeOfOrder.Camera => Enum.GetNames<DO.Catalog.CameraDetails>(),
                DO.TypeOfOrder.Audio => Enum.GetNames<DO.Catalog.AudioDetails>(),
                DO.TypeOfOrder.SmartHome => Enum.GetNames<DO.Catalog.SmartHomeDetails>(),
                DO.TypeOfOrder.GamingConsole => Enum.GetNames<DO.Catalog.GamingConsoleDetails>(),
                DO.TypeOfOrder.Accessory => Enum.GetNames<DO.Catalog.AccessoryDetails>(),
                _ => Array.Empty<string>()
            };

            // Choosing product from Catalog
            string product = "";
            int count = s_rand.Next(1, 4); // Randomly choose how many products between 1 and 3
            List<string> chosenProducts = new();

            // Loop to randomly pick products
            for (int i = 0; i < count; i++)
            {
                string chosen = items.Length > 0 ? items[s_rand.Next(items.Length)] : "Generic Item";
                if (!chosenProducts.Contains(chosen))
                    chosenProducts.Add(chosen);
                else
                    count--; // if hit the same, try again
            }
            // Combine all chosen products into one string separated by commas
            product = string.Join(", ", chosenProducts);

            // Random customer + phone in Israeli format 05X-XXXXXXX
            string customer = customers[s_rand.Next(customers.Length)];
            string phone = "05" + s_rand.Next(0, 9) + "-" + s_rand.Next(1000000, 9999999);

            // Order date: sometime in the last ~90 days relative to the system clock
            DateTime orderDate = clock.AddDays(-s_rand.Next(0, 90)).AddMinutes(-s_rand.Next(0, 12 * 60));

            // Size/weight profile per category (min/max + fragility probability)
            (double weightMin, double weightMax, double sizeMin, double sizeMax, bool fragP) pack = kind switch
            {
                DO.TypeOfOrder.Smartphone => (0.2, 0.8, 0.1, 0.3, false),
                DO.TypeOfOrder.Tablet => (0.3, 1.0, 0.15, 0.35, true),
                DO.TypeOfOrder.Laptop => (1.0, 3.0, 0.3, 0.8, true),
                DO.TypeOfOrder.TV => (8.0, 25.0, 0.9, 2.0, true),
                DO.TypeOfOrder.Camera => (0.4, 2.0, 0.2, 0.6, true),
                DO.TypeOfOrder.Audio => (0.2, 1.5, 0.15, 0.6, false),
                DO.TypeOfOrder.SmartHome => (0.3, 2.5, 0.2, 0.7, false),
                DO.TypeOfOrder.GamingConsole => (2.0, 6.0, 0.5, 1.0, false),
                DO.TypeOfOrder.Accessory => (0.05, 0.5, 0.05, 0.2, false),
                _ => (0.5, 5.0, 0.2, 1.0, true)
            };

            // Sample weight/size within the category ranges, and sample fragility
            double weight = pack.weightMin + s_rand.NextDouble() * (pack.weightMax - pack.weightMin);
            double size = pack.sizeMin + s_rand.NextDouble() * (pack.sizeMax - pack.sizeMin);
            weight = Math.Round(weight, 1);
            size = Math.Round(size, 1);
            bool fragile = pack.fragP;

            // Compute straight-line (air) distance company <-> order address (km)
            double airKm = Haversine(companyLat, companyLon, addr.lat, addr.lon);

            // order Status Tag
            string orderStatusTag = $"[{statusTag}]";

            // Human-readable detail: status tag + product + category + weight + ~air distance
            string detail = $" {product} => {kind} , {weight}kg , ~{airKm:F1}km";

            // Persist via DAL: orderId = 0 so DAL assigns the next running ID
            s_dal!.Order.Create(new DO.Order(
                OrderId: 0,
                OrderStatus: orderStatusTag,
                OrderDetail: detail,
                OrderAddress: addr.orderAddress,
                OrderLatitude: addr.lat,
                OrderLongitude: addr.lon,
                OrderCustomerFullName: customer,
                OrderCustomerPhone: phone,
                OrderWeight: weight,
                IsFragile: fragile,
                OrderSize: size,
                OrderDate: orderDate,
                TypeOfOrder: kind
            ));
        }
    }

    /// <summary>
    /// Generates and stores Delivery entities by matching Orders with Couriers.
    /// </summary>
    private static void createDeliverys()
    {
        // Fetch current couriers and orders; if either is empty, there is nothing to do
        var couriers = s_dal!.Courier.ReadAll();
        var orders = s_dal.Order.ReadAll();

        // Check if empty
        if (!couriers.Any() || !orders.Any())
            return;

        // Create deliveries for a subset of orders (e.g., ~60%).
        int total = orders.Count();
        int targetDeliveries = Math.Max(1, (int)(total * 0.6));

        // Get enum values for random sampling
        var shipmentTypes = Enum.GetValues<DO.ShipmentType>();
        var finishTypes = Enum.GetValues<DO.DeliveryFinishType>();

        // Shuffle orders lightly by sampling indexes
        var orderIndexes = new List<int>(total);
        for (int i = 0; i < total; i++) orderIndexes.Add(i);
        for (int i = 0; i < orderIndexes.Count; i++)
        {
            int j = s_rand.Next(i, orderIndexes.Count);
            (orderIndexes[i], orderIndexes[j]) = (orderIndexes[j], orderIndexes[i]);
        }

        // Create deliveries for the first `targetDeliveries` shuffled orders
        for (int k = 0; k < targetDeliveries; k++)
        {
            var order = orders.ElementAt(orderIndexes[k]);

            // Pick a random courier for this delivery
            var courier = couriers.ElementAt(s_rand.Next(couriers.Count()));

            // Base timestamps
            DateTime deliveryDate = order.OrderDate.AddHours(s_rand.Next(0, 24)).AddMinutes(s_rand.Next(0, 60));
            DateTime deliveryFinishDate = deliveryDate.AddMinutes(s_rand.Next(30, 240));

            // Sample randomly from enum
            DO.ShipmentType shipmentType = shipmentTypes[s_rand.Next(shipmentTypes.Length)];

            // Delivery finish type Cancelled/Failed/Returned/Completed
            DO.DeliveryFinishType finishType;
            int p = s_rand.Next(100);
            if (p < 85) finishType = DO.DeliveryFinishType.Completed;        // ~85%
            else if (p < 92) finishType = DO.DeliveryFinishType.Cancelled;   // ~7%
            else if (p < 95) finishType = DO.DeliveryFinishType.Failed;      // ~3%
            else finishType = DO.DeliveryFinishType.Returned;                // ~5%

            // Max distance policy used for later checks
            double? maxDistance = (s_rand.NextDouble() < 0.90) ? s_dal.Config.MaxAirDistance : null;

            // Persist via DAL (DeliveryImplementation will assign the running deliveryId)
            s_dal.Delivery.Create(new DO.Delivery(
                DeliveryId: 0,                    // Next Delivery Id
                OrderId: order.OrderId,           // link order
                CourierId: courier.CourierId,     // link courier
                DeliveryMaxDistance: maxDistance, // per delivery
                DeliveryDate: deliveryDate,
                DeliveryFinishDate: deliveryFinishDate,
                ShipmentType: shipmentType,
                DeliveryFinishType: finishType
            ));
        }
    }

    /// <summary>
    /// Initializes system configuration variables.
    /// </summary>
    private static void createConfig()
    {
        // System clock
        s_dal!.Config.Clock = DateTime.Now;

        // Admin credentials
        s_dal.Config.AdminId = 333333333;               // admin id
        s_dal.Config.AdminPassword = "ChangeMe!1234";   // password

        // Company address and its geo-coordinates
        s_dal.Config.CompanyAddress = "Malha Mall, Derech Agudat Sport Beitar 1, Jerusalem"; // textual address
        s_dal.Config.Latitude = 31.7479;
        s_dal.Config.Longitude = 35.1880;

        // Global acceptance constraint: maximum straight-line (air) distance for orders (km)
        s_dal.Config.MaxAirDistance = 25.0;

        // Average speeds (km/h)
        s_dal.Config.AvgCarSpeed = 35.0;         // Car average
        s_dal.Config.AvgMotorcycleSpeed = 40.0;  // Motorcycle average
        s_dal.Config.AvgBicycleSpeed = 15.0;     // Bicyle average
        s_dal.Config.AvgWalkSpeed = 5.0;         // Walk average

        // Time policy ranges
        s_dal.Config.MaxDelTimeRnge = TimeSpan.FromDays(2);     // deliveries should usually complete within ~48h
        s_dal.Config.RiskTimeRnge = TimeSpan.FromHours(3);    // if ETA exceeds by 3h → mark as "at risk"
        s_dal.Config.UnactiveTimeRnge = TimeSpan.FromDays(45);   // 45 days of inactivity is considered stale
    }

    #endregion DataCreation

    //==================== Helpers ===================\\

    #region Helpers

    /// <summary>
    /// Calculates the great-circle distance between two points on the Earth's surface using the Haversine formula.
    /// </summary>
    /// <param name="srcLat">Source Latitude</param>
    /// <param name="srcLon">Source Longitude</param>
    /// <param name="dstLat">Destination Latitude</param>
    /// <param name="dstLon">Destination Longitude</param>
    /// <returns>Distance in Kilometers</returns>
    private static double Haversine(double srcLat, double srcLon, double dstLat, double dstLon)
    {
        // Convert degrees to radians
        double ToRad(double deg) => deg * Math.PI / 180.0;

        // Earth radius (mean) in kilometers
        const double R = 6371.0;

        // Haversine formula
        double dLat = ToRad(dstLat - srcLat);
        double dLon = ToRad(dstLon - srcLon);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                 + Math.Cos(ToRad(srcLat)) * Math.Cos(ToRad(dstLat))
                 * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c; // great-circle distance in km
    }

    #endregion Helpers

}