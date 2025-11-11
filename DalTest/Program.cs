namespace DalTest;
using Dal;
using DalApi;
using DO;
using System.Data;

internal class Program
{

    //private static ICourier? s_dalCourier = new CourierImplementation(); //stage 1
    //private static IDelivery? s_dalDelivery = new DeliveryImplementation(); //stage 1
    //private static IOrder? s_dalOrder = new OrderImplementation(); //stage 1
    //private static IConfig? s_dalConfig = new ConfigImplementation(); //stage 1

    static readonly IDal s_dal = new DalList(); //stage 2


    // -------------------- Main -------------------- \\
    static void Main(string[] args)
    {

        // Always start with a clean console and show the clock.
        Console.Clear();
        Console.WriteLine($"Clock: {s_dal.Config.Clock:yyyy-MM-dd HH:mm:ss}");
        // show the root menu until user chooses Exit
        while (true)
        {
            try
            {
                switch (RootMenu())
                {
                    case 0: Console.WriteLine("Bye!"); return;
                    case 1: DoInitialization(); break;
                    case 2: PrintCounts(); break;
                    case 3: CourierMenu(); break;
                    case 4: OrderMenu(); break;
                    case 5: DeliveryMenu(); break;
                    case 6: ResetAllData(); break;
                    default: Console.WriteLine("Unknown option."); break;
                }
            }
            catch (Exception msg)
            {
                Console.WriteLine(msg);
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.ReadLine();
            Console.Clear();
        }
    }



    // -------------------- Menus -------------------- \\
    private static int RootMenu()
    {
        Console.WriteLine(@"// ==== MAIN MENU ==== \\");
        Console.WriteLine("1) Initialization.Do ");
        Console.WriteLine("2) Print entities count summary ");
        Console.WriteLine("3) Couriers ");
        Console.WriteLine("4) Orders ");
        Console.WriteLine("5) Deliveries ");
        Console.WriteLine("6) Reset ALL data ");
        Console.WriteLine("0) Exit ");
        Console.Write("Choose: ");
        return ReadIntOfMenu();
    }
    private static void CourierMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("---- COURIERS MENU ----");
            var number = shopMenu();

            switch (number)
            {
                case 0: return;
                case 1: CreateCourier(); break;
                case 2: ReadCourier(); break;
                case 3: ReadAllCouriers(); break;
                case 4: UpdateCourier(); break;
                case 5: DeleteCourier(); break;
                case 6: s_dal.Courier.DeleteAll(); Console.WriteLine("All couriers deleted."); break;
                default: Console.WriteLine("Unknown option."); break;
            }
            Console.WriteLine("\nPress ENTER...");
            Console.ReadLine();
        }
    }
    private static void OrderMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("---- ORDERS MENU ----");
            var number = shopMenu();

            switch (number)
            {
                case 0: return;
                case 1: CreateOrder(); break;
                case 2: ReadOrder(); break;
                case 3: ReadAllOrders(); break;
                case 4: UpdateOrder(); break;
                case 5: DeleteOrder(); break;
                case 6: s_dal.Order.DeleteAll(); Console.WriteLine("All orders deleted."); break;
                default: Console.WriteLine("Unknown option."); break;
            }

            Console.WriteLine("\nPress ENTER...");
            Console.ReadLine();
        }
    }
    private static void DeliveryMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("---- DELIVERIES MENU ----");
            var number = shopMenu();

            switch (number)
            {
                case 0: return;
                case 1: CreateDelivery(); break;
                case 2: ReadDelivery(); break;
                case 3: ReadAllDeliveries(); break;
                case 4: UpdateDelivery(); break;
                case 5: DeleteDelivery(); break;
                case 6: s_dal.Delivery.DeleteAll(); Console.WriteLine("All deliveries deleted."); break;
                default: Console.WriteLine("Unknown option."); break;
            }

            Console.WriteLine("\nPress ENTER...");
            Console.ReadLine();
        }
    }
    private static int shopMenu()
    {
        Console.WriteLine("0) Back");
        Console.WriteLine("1) Create");
        Console.WriteLine("2) Read by Id");
        Console.WriteLine("3) ReadAll");
        Console.WriteLine("4) Update");
        Console.WriteLine("5) Delete by Id");
        Console.WriteLine("6) DeleteAll");
        Console.Write("Choose: ");
        return ReadIntOfMenu();
    }

    // -------------------- Stage ops (init/print/reset) -------------------- \\
    private static void DoInitialization()
    {
        // The Do method is assumed to seed base data using the DAL instances.
        Initialization.Do(s_dal); //stage 2
        Console.WriteLine("Initialization.Do completed.");
    }
    private static void PrintCounts()
    {
        Console.WriteLine(@"// ---- COUNTS ---- \\");
        Console.WriteLine($"Couriers  : {s_dal.Courier.ReadAll().Count()}");
        Console.WriteLine($"Orders    : {s_dal.Order.ReadAll().Count()}");
        Console.WriteLine($"Deliveries: {s_dal.Delivery.ReadAll().Count()}");
        Console.WriteLine($"Clock     : {s_dal.Config.Clock:yyyy-MM-dd HH:mm:ss}");
    }
    private static void ResetAllData()
    {
        // Fully reset system data (as per slides: DeleteAll + ResetConfig)
        s_dal.Courier.DeleteAll();
        s_dal.Order.DeleteAll();
        s_dal.Delivery.DeleteAll();
        s_dal.Config.Reset();
        Console.WriteLine("All lists cleared and Config reset.");
    }


    // -------------------- Courier -------------------- \\
    private static void CreateCourier()
    {
        Console.WriteLine("Creating a Courier...");
        int id = ReadInt("Id: ");
        string fullName = ReadRequired("Full name: ");
        string companyAddress = ReadRequired("Courier start address: ");
        string phone = ReadRequired("PhoneNumber: ");
        string email = ReadRequired("Email address: ");
        string password = ReadRequired("Password: ");
        bool isActive = ReadBool("Is the courier active? ");

        Console.Write("Max delivery distance or Enter to skip: ");
        string? distInput = Console.ReadLine();
        double? distance = null;
        if (double.TryParse(distInput, out double distVal))
            distance = distVal;

        Console.Write("Employment start date dd/MM/yyyy or Enter to skip: ");
        string? dateInput = Console.ReadLine();
        DateTime? experience = null;
        if (DateTime.TryParse(dateInput, out DateTime dateVal))
            experience = dateVal;

        // Vehicle type selection from enum
        courierVehicleType VehicleType = ReadEnum<courierVehicleType>("Vehicle type");

        // Adapt fields to your DO.Courier signature.
        var newCourier = new Courier(
            courierId: id,
            courierFullName: fullName,
            courierAddress: companyAddress,
            courierCellPhone: phone,
            courierEmail: email,
            courierPassword: password,
            courierEnabled: isActive,
            maxCourierDistance: distance,
            seniorityOfCourier: experience,
            courierVehicleType: VehicleType
        );

        s_dal.Courier.Create(newCourier);
        Console.WriteLine("Courier created.");
    }
    private static void ReadCourier()
    {
        int id = ReadInt("Courier Id: ");
        var c = s_dal.Courier.Read(id);
        if (c == null) throw new DalDoesNotExistException($"Courier with ID={id} does not exist");
        Console.WriteLine(c);
    }
    private static void ReadAllCouriers()
    {
        foreach (var c in s_dal.Courier.ReadAll())
            Console.WriteLine($"{c} \n");
    }
    private static void UpdateCourier()
    {
        int id = ReadInt("Courier Id to update: ");
        var c = s_dal.Courier.Read(id);
        if (c == null) throw new DalDoesNotExistException($"Courier with ID={id} does not exist");

        Console.WriteLine($"Current: {c}");
        string fullName = ReadOptional($"Full name [{c.courierFullName}]: ", c.courierFullName);
        string address = ReadOptional($"Courier address [{c.courierAddress}]: ", c.courierAddress);
        string phone = ReadOptional($"Phone [{c.courierCellPhone}]: ", c.courierCellPhone);
        string email = ReadOptional($"Email [{c.courierEmail}]: ", c.courierEmail);
        string password = ReadOptional($"Password [{c.courierPassword}]: ", c.courierPassword);

        bool isActive = c.courierEnabled;
        Console.Write($"Active? (y/n, Enter=keep [{(c.courierEnabled ? "y" : "n")}]): ");
        var activeIn = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
        if (activeIn is "y" or "yes" or "true") isActive = true;
        else if (activeIn is "n" or "no" or "false") isActive = false;


        double? distance = c.maxCourierDistance;
        Console.Write($"Max distance km [current={(c.maxCourierDistance?.ToString() ?? "null")}]. Enter=keep, '-'=null, or number: ");
        var distIn = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(distIn))
        {
            if (distIn.Trim() == "-") distance = null;
            else if (double.TryParse(distIn, out var d)) distance = d;
            else throw new Exception("Invalid number for distance.");
        }

        DateTime? experience = c.seniorityOfCourier;
        Console.Write($"Employment start date dd/MM/yyyy [current={(c.seniorityOfCourier?.ToString("dd/MM/yyyy") ?? "null")}]. Enter=keep, '-'=null: ");
        var dateIn = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(dateIn))
        {
            if (dateIn.Trim() == "-") experience = null;
            else if (DateTime.TryParse(dateIn, out var dt)) experience = dt;
            else throw new Exception("Invalid date/time.");
        }

        var vehicleType = c.courierVehicleType;
        Console.WriteLine($"Vehicle type (Enter=keep [{c.courierVehicleType}]): {string.Join(", ", Enum.GetNames<courierVehicleType>())}");
        Console.Write("> ");
        var enumIn = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(enumIn))
        {
            if (!Enum.TryParse<courierVehicleType>(enumIn, ignoreCase: true, out vehicleType))
                throw new Exception("Invalid vehicle type.");
        }

        var updated = c with
        {
            courierFullName = fullName,
            courierAddress = address,
            courierCellPhone = phone,
            courierEmail = email,
            courierPassword = password,
            courierEnabled = isActive,
            maxCourierDistance = distance,
            seniorityOfCourier = experience,
            courierVehicleType = vehicleType
        };

        s_dal.Courier.Update(updated);
        Console.WriteLine("Courier updated.");
    }
    private static void DeleteCourier()
    {
        int id = ReadInt("Courier Id to delete: ");
        s_dal.Courier.Delete(id);
        Console.WriteLine("Courier deleted.");
    }


    // -------------------- Order -------------------- \\
    private static void CreateOrder()
    {
        Console.WriteLine("Creating an Order...");
        string detail = ReadRequired("Detail/Description: ");
        string address = ReadRequired("Address: ");
        double lat = ReadDouble("Latitude: ");
        double lon = ReadDouble("Longitude: ");
        string customer = ReadRequired("Customer full name: ");
        string phone = ReadRequired("Customer phone: ");
        double weight = ReadDouble("Weight (kg): ");
        bool fragile = ReadBool("Fragile (y/n): ");
        double size = ReadDouble("Size (arbitrary units): ");
        DateTime date = ReadDateTime("Order date (dd/MM/yy HH:mm:ss): ");
        var kind = ReadEnum<typeOfOrder>("Order kind (enum): ");

        var o = new Order(
            orderId: 0, // DAL will allocate running Id
            orderDetail: detail,
            orderAddress: address,
            orderLatitude: lat,
            orderLongitude: lon,
            orderCostumerFullName: customer,
            orderCostumerPhone: phone,
            orderWeight: weight,
            fragile: fragile,
            orderSize: size,
            orderDate: date,
            typeOfOrder: kind
        );

        s_dal.Order.Create(o);
        Console.WriteLine("Order created.");
    }
    private static void ReadOrder()
    {
        int id = ReadInt("Order Id: ");
        var o = s_dal.Order.Read(id);
        if (o == null) throw new DalDoesNotExistException($"Order with ID={id} does not exist");
        Console.WriteLine(o);
    }
    private static void ReadAllOrders()
    {
        foreach (var o in s_dal.Order.ReadAll())
            Console.WriteLine($"{o} \n");
    }
    private static void UpdateOrder()
    {
        int id = ReadInt("Order Id to update: ");
        var o = s_dal.Order.Read(id);
        Console.WriteLine($"Current: {o}");
        if (o == null) throw new DalDoesNotExistException($"Order with ID={id} does not exist");

        string detail = ReadOptional($"Detail [{o.orderDetail}]: ", o.orderDetail);
        string address = ReadOptional($"Address [{o.orderAddress}]: ", o.orderAddress);
        double lat = ReadDoubleOptional($"Latitude [{o.orderLatitude}]: ", o.orderLatitude);
        double lon = ReadDoubleOptional($"Longitude [{o.orderLongitude}]: ", o.orderLongitude);
        string phone = ReadOptional($"Phone [{o.orderCostumerPhone}]: ", o.orderCostumerPhone);

        var updated = o with
        {
            orderDetail = detail,
            orderAddress = address,
            orderLatitude = lat,
            orderLongitude = lon,
            orderCostumerPhone = phone
        };

        s_dal.Order.Update(updated);
        Console.WriteLine("Order updated.");
    }
    private static void DeleteOrder()
    {
        int id = ReadInt("Order Id to delete: ");
        s_dal.Order.Delete(id);
        Console.WriteLine("Order deleted.");
    }


    // -------------------- Delivery -------------------- \\
    private static void CreateDelivery()
    {
        Console.WriteLine("Creating a Delivery...");

        int orderId = ReadInt("Order Id to deliver: ");
        int courierId = ReadInt("Courier Id: ");


        Console.Write("Max air distance or Enter to skip: ");
        string? distInput = Console.ReadLine();
        double? deliveryMaxDistance = null;
        if (double.TryParse(distInput, out double dVal))
            deliveryMaxDistance = dVal;

        // Timestamps
        DateTime deliveryDate = ReadDateTime("Pickup/Start date (dd/MM/yy HH:mm:ss): ");
        DateTime deliveryFinishDate = ReadDateTime("Finish date (dd/MM/yy HH:mm:ss): ");

        // Enums
        var shipType = ReadEnum<ShipmentType>("Shipment type: ");
        var finishType = ReadEnum<DeliveryFinishType>("Delivery finish type: ");

        var d = new Delivery(
            deliveryId: 0,                 // running Id
            orderId: orderId,
            courierId: courierId,
            deliveryMaxDistance: deliveryMaxDistance,
            deliveryDate: deliveryDate,
            deliveryFinishDate: deliveryFinishDate,
            shipmentType: shipType,
            deliveryFinishType: finishType
        );

        s_dal.Delivery.Create(d);
        Console.WriteLine("Delivery created.");
    }
    private static void ReadDelivery()
    {
        int id = ReadInt("Delivery Id: ");
        var d = s_dal.Delivery.Read(id);
        if (d == null) throw new DalDoesNotExistException($"Delivery with ID={id} does not exist");
        Console.WriteLine(d);
    }
    private static void ReadAllDeliveries()
    {
        foreach (var d in s_dal.Delivery.ReadAll())
            Console.WriteLine($"{d} \n");
    }
    private static void UpdateDelivery()
    {
        // Fetch current entity
        int id = ReadInt("Delivery Id to update: ");
        var d = s_dal.Delivery.Read(id);
        if (d == null) throw new DalDoesNotExistException($"Delivery with ID={id} does not exist");

        Console.WriteLine($"Current: {d}");

        // ---- basic ids (Enter = keep) ----
        int orderId = ReadIntOptional($"Order Id [{d.orderId}]: ", d.orderId);
        int courierId = ReadIntOptional($"Courier Id [{d.courierId}]: ", d.courierId);

        // ---- nullable double (Enter = keep, '-' = null) ----
        double? maxDist = d.deliveryMaxDistance;
        Console.Write($"Max air distance [current={(d.deliveryMaxDistance?.ToString() ?? "null")}]. Enter=keep, '-'=null, or number: ");
        var distIn = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(distIn))
        {
            if (distIn.Trim() == "-") maxDist = null;
            else if (double.TryParse(distIn, out var dd)) maxDist = dd;
            else throw new FormatException("Invalid number for max distance.");
        }

        // ---- datetimes (Enter = keep) ----
        Console.Write($"Pickup/Start date (dd/MM/yy HH:mm:ss) [current={d.deliveryDate:dd/MM/yy HH:mm:ss}] (Enter=keep): ");
        var startIn = Console.ReadLine();
        DateTime deliveryDate = d.deliveryDate;
        if (!string.IsNullOrWhiteSpace(startIn))
        {
            if (!DateTime.TryParse(startIn, out deliveryDate))
                throw new FormatException("Invalid start date/time.");
        }

        Console.Write($"Finish date (dd/MM/yy HH:mm:ss) [current={d.deliveryFinishDate:dd/MM/yy HH:mm:ss}] (Enter=keep): ");
        var finIn = Console.ReadLine();
        DateTime deliveryFinishDate = d.deliveryFinishDate;
        if (!string.IsNullOrWhiteSpace(finIn))
        {
            if (!DateTime.TryParse(finIn, out deliveryFinishDate))
                throw new FormatException("Invalid finish date/time.");
        }

        // ---- enums (Enter = keep) ----
        var shipType = d.shipmentType;
        Console.WriteLine($"Shipment type (Enter=keep [{d.shipmentType}]): {string.Join(", ", Enum.GetNames<ShipmentType>())}");
        Console.Write("> ");
        var shipIn = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(shipIn))
        {
            if (!Enum.TryParse<ShipmentType>(shipIn, ignoreCase: true, out shipType))
                throw new FormatException("Invalid shipment type.");
        }

        var finishType = d.deliveryFinishType;
        Console.WriteLine($"Delivery finish type (Enter=keep [{d.deliveryFinishType}]): {string.Join(", ", Enum.GetNames<DeliveryFinishType>())}");
        Console.Write("> ");
        var finTypeIn = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(finTypeIn))
        {
            if (!Enum.TryParse<DeliveryFinishType>(finTypeIn, ignoreCase: true, out finishType))
                throw new FormatException("Invalid delivery finish type.");
        }

        // ---- persist update ----
        var updated = d with
        {
            orderId = orderId,
            courierId = courierId,
            deliveryMaxDistance = maxDist,
            deliveryDate = deliveryDate,
            deliveryFinishDate = deliveryFinishDate,
            shipmentType = shipType,
            deliveryFinishType = finishType
        };

        s_dal.Delivery.Update(updated);
        Console.WriteLine("Delivery updated.");
    }
    private static void DeleteDelivery()
    {
        int id = ReadInt("Delivery Id to delete: ");
        s_dal.Delivery.Delete(id);
        Console.WriteLine("Delivery deleted.");
    }


    // -------------------- Read helpers -------------------- \\
    private static int ReadIntOfMenu()
    {
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out int number))
            {
                if (number >= 0 && number <= 6) return number;
            }
            Console.WriteLine("\nTry again...");
            Console.Write("Choose: ");
        }
    }
    private static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? check = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(check)) return check.Trim();
            Console.WriteLine("Value is required.");
        }
    }
    private static string ReadOptional(string prompt, string current)
    {
        Console.Write(prompt);
        string? s = Console.ReadLine();
        return string.IsNullOrWhiteSpace(s) ? current : s.Trim();
    }
    private static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int v))
            {
                /*if (200000000 < v && 400000000 > v)*/ return v;
            }
            Console.WriteLine("Invalid integer. Try again.");
        }
    }
    private static double ReadDouble(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (double.TryParse(Console.ReadLine(), out double v)) return v;
            Console.WriteLine("Invalid number. Try again.");
        }
    }
    private static double ReadDoubleOptional(string prompt, double current)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s)) return current;
        if (double.TryParse(s, out double v)) return v;
        throw new DalInvalidNumberException("Invalid number.");
    }
    private static int ReadIntOptional(string prompt, int current)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s)) return current;
        if (int.TryParse(s, out int v)) return v;
        throw new DalInvalidNumberException("Invalid integer.");
    }
    private static bool ReadBool(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
            if (s is "y" or "yes" or "true") return true;
            if (s is "n" or "no" or "false") return false;
            Console.WriteLine("Please enter yes/no.");
        }
    }
    private static DateTime ReadDateTime(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (DateTime.TryParse(Console.ReadLine(), out DateTime dt)) return dt;
            Console.WriteLine("Invalid date/time. Expected e.g. 31/12/24 13:45:00");
        }
    }
    private static DateTime? ReadDateTimeOptional(string prompt)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (DateTime.TryParse(s, out DateTime dt)) return dt;
        throw new DalInvalidDateException("Invalid date/time.");
    }
    private static TEnum ReadEnum<TEnum>(string prompt) where TEnum : struct, Enum
    {
        while (true)
        {
            Console.Write($"{prompt}{Environment.NewLine}Options: {string.Join(", ", Enum.GetNames<TEnum>())}{Environment.NewLine}> ");
            var s = Console.ReadLine();
            if (Enum.TryParse<TEnum>(s, ignoreCase: true, out var val)) return val;
            Console.WriteLine("Invalid enum value.");
        }
    }

}