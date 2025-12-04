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

    //static readonly IDal s_dal = new DalList(); //stage 2

    //static readonly IDal s_dal = new DalXml(); //stage 3

    static readonly IDal s_dal = Factory.Get; //stage 4

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

    /// <summary>
    /// Displays the main root menu with all program sections.  
    /// Waits for the user's numeric choice and returns it.
    /// </summary>
    /// <returns>The selected menu option as an integer.</returns>
    private static int RootMenu()
    {
        // print main menu
        Console.WriteLine(@"// ==== MAIN MENU ==== \\");
        Console.WriteLine("1) Initialization.Do ");
        Console.WriteLine("2) Print entities count summary ");
        Console.WriteLine("3) Couriers ");
        Console.WriteLine("4) Orders ");
        Console.WriteLine("5) Deliveries ");
        Console.WriteLine("6) Reset ALL data ");
        Console.WriteLine("0) Exit ");
        Console.Write("Choose: ");
        return ReadIntOfMenu(); // read user choice (0–6)
    }

    /// <summary>
    /// Opens the Couriers submenu, allowing the user to manage couriers (CRUD operations).  
    /// Loops until the user chooses to return to the previous menu.
    /// </summary>
    private static void CourierMenu()
    {
        while (true)
        {
            Console.Clear(); // clear screen each time
            Console.WriteLine("---- COURIERS MENU ----");
            var number = shopMenu(); // display submenu options

            switch (number)
            {
                case 0: return; // back to main menu
                case 1: CreateCourier(); break; // add new courier
                case 2: ReadCourier(); break; // read courier by id
                case 3: ReadAllCouriers(); break; // list all couriers
                case 4: UpdateCourier(); break; // update courier details
                case 5: DeleteCourier(); break; // delete one courier
                case 6: s_dal.Courier.DeleteAll(); Console.WriteLine("All couriers deleted."); break; // delete all
                default: Console.WriteLine("Unknown option."); break;
            }

            Console.WriteLine("\nPress ENTER...");
            Console.ReadLine(); // wait for user before refreshing
        }
    }

    /// <summary>
    /// Opens the Orders submenu, allowing the user to manage orders (CRUD operations).  
    /// Loops until the user chooses to return to the previous menu.
    /// </summary>
    private static void OrderMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("---- ORDERS MENU ----");
            var number = shopMenu(); // show order options

            switch (number)
            {
                case 0: return; // back
                case 1: CreateOrder(); break; // create new order
                case 2: ReadOrder(); break; // read order by id
                case 3: ReadAllOrders(); break; // list all orders
                case 4: UpdateOrder(); break; // update existing order
                case 5: DeleteOrder(); break; // delete one order
                case 6: s_dal.Order.DeleteAll(); Console.WriteLine("All orders deleted."); break; // clear all
                default: Console.WriteLine("Unknown option."); break;
            }

            Console.WriteLine("\nPress ENTER...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Opens the Deliveries submenu, allowing the user to manage deliveries (CRUD operations).  
    /// Loops until the user chooses to return to the previous menu.
    /// </summary>
    private static void DeliveryMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("---- DELIVERIES MENU ----");
            var number = shopMenu(); // show delivery options

            switch (number)
            {
                case 0: return; // back
                case 1: CreateDelivery(); break; // create new delivery
                case 2: ReadDelivery(); break; // read delivery by id
                case 3: ReadAllDeliveries(); break; // list all deliveries
                case 4: UpdateDelivery(); break; // update delivery
                case 5: DeleteDelivery(); break; // delete one delivery
                case 6: s_dal.Delivery.DeleteAll(); Console.WriteLine("All deliveries deleted."); break; // clear all
                default: Console.WriteLine("Unknown option."); break;
            }

            Console.WriteLine("\nPress ENTER...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Displays a generic submenu for managing a specific entity (Courier, Order, or Delivery).  
    /// The menu includes options for Create, Read, Update, Delete, etc.  
    /// Waits for the user's numeric choice and returns it.
    /// </summary>
    /// <returns>The selected submenu option as an integer.</returns>
    private static int shopMenu()
    {
        // print standard CRUD submenu
        Console.WriteLine("0) Back");
        Console.WriteLine("1) Create");
        Console.WriteLine("2) Read by Id");
        Console.WriteLine("3) ReadAll");
        Console.WriteLine("4) Update");
        Console.WriteLine("5) Delete by Id");
        Console.WriteLine("6) DeleteAll");
        Console.Write("Choose: ");
        return ReadIntOfMenu(); // read user choice
    }


    // -------------------- Stage ops (init/print/reset) -------------------- \\

    /// <summary>
    /// Performs the system initialization process.  
    /// Calls the <c>Initialization.Do</c> method to seed base data using DAL instances.  
    /// Used in Stage 2 of the project.
    /// </summary>
    private static void DoInitialization()
    {
        // Seed base data calls Initialization.Do using current DAL
        /*
        Initialization.Do(s_dal); //stage 2
        */
        Initialization.Do(); // stage 4
        Console.WriteLine("Initialization.Do completed.");
    }

    /// <summary>
    /// Prints a summary of entity counts (Couriers, Orders, Deliveries)  
    /// and displays the current system clock from configuration.
    /// </summary>
    private static void PrintCounts()
    {
        // display entity totals and clock time
        Console.WriteLine(@"// ---- COUNTS ---- \\");
        Console.WriteLine($"Couriers  : {s_dal.Courier.ReadAll().Count()}");        // number of couriers
        Console.WriteLine($"Orders    : {s_dal.Order.ReadAll().Count()}");          // number of orders
        Console.WriteLine($"Deliveries: {s_dal.Delivery.ReadAll().Count()}");       // number of deliveries
        Console.WriteLine($"Clock     : {s_dal.Config.Clock:yyyy-MM-dd HH:mm:ss}"); // current clock time
    }

    /// <summary>
    /// Resets all stored system data completely.  
    /// Deletes all entities (Couriers, Orders, Deliveries)  
    /// and resets configuration values to defaults.
    /// </summary>
    private static void ResetAllData()
    {

        //s_dal.Courier.DeleteAll(); // stage 2
        //s_dal.Order.DeleteAll(); // stage 2
        //s_dal.Delivery.DeleteAll(); // stage 2
        // reset configuration (e.g., clock and IDs)
        //s_dal.Config.Reset(); // stage 2


        // clear all data entities
        s_dal.ResetDB(); // stage 3
        Console.WriteLine("All lists cleared and Config reset.");
    }


    // -------------------- Courier -------------------- \\

    /// <summary>
    /// Create a new Courier and save it to the data layer
    /// </summary>
    private static void CreateCourier()
    {
        Console.WriteLine("Creating a Courier...");
        int id = ReadInt("Id: "); // read courier ID
        string fullName = ReadRequired("Full name: "); // read full name
        string companyAddress = ReadRequired("Courier start address: "); // read start address
        string phone = ReadRequired("PhoneNumber: "); // read phone number
        string email = ReadRequired("Email address: "); // read email
        string password = ReadRequired("Password: "); // read password
        bool isActive = ReadBool("Is the courier active? "); // read status

        Console.Write("Max delivery distance or Enter to skip: ");
        string? distInput = Console.ReadLine();
        double? distance = null; // nullable distance
        if (double.TryParse(distInput, out double distVal))
            distance = distVal;

        Console.Write("Employment start date dd/MM/yyyy or Enter to skip: ");
        string? dateInput = Console.ReadLine();
        DateTime? experience = null; // nullable start date
        if (DateTime.TryParse(dateInput, out DateTime dateVal))
            experience = dateVal;

        // choose vehicle type from enum
        CourierVehicleType VehicleType = ReadEnum<CourierVehicleType>("Vehicle type");

        // create new courier object
        var newCourier = new Courier(
            CourierId: id,
            CourierFullName: fullName,
            CourierAddress: companyAddress,
            CourierCellPhone: phone,
            CourierEmail: email,
            CourierPassword: password,
            CourierEnabled: isActive,
            MaxCourierDistance: distance,
            SeniorityOfCourier: experience,
            CourierVehicleType: VehicleType
        );

        s_dal.Courier.Create(newCourier); // save to data layer
        Console.WriteLine("Courier created.");
    }

    /// <summary>
    /// Read and display an Courier by its ID
    /// </summary>
    /// <exception cref="DalDoesNotExistException"> Thrown if the Courier does not exist </exception>
    private static void ReadCourier()
    {
        int id = ReadInt("Courier Id: "); // read courier ID
        var c = s_dal.Courier.Read(id); // read from DAL
        if (c == null) throw new DalDoesNotExistException($"Courier with ID={id} does not exist");
        Console.WriteLine(c); // print courier info
    }

    /// <summary>
    /// Display all couriers from the data layer
    /// </summary>
    private static void ReadAllCouriers()
    {
        foreach (var c in s_dal.Courier.ReadAll()) // loop all couriers
            Console.WriteLine($"{c} \n");
    }

    /// <summary>
    /// Update an existing Courier by ID
    /// </summary>
    /// <exception cref="DalDoesNotExistException"> Thrown if the Courier does not exist </exception>
    /// <exception cref="DalInvalidIntegerException"> Thrown if the number is invalid </exception>
    /// <exception cref="DalInvalidDateException"> Thrown if the Date is invalid </exception>
    /// <exception cref="DalInvalidVehicleTypeException"> Thrown if the Vehicle is invalid </exception>
    private static void UpdateCourier()
    {
        int id = ReadInt("Courier Id to update: "); // read courier Id
        var c = s_dal.Courier.Read(id); // get existing courier if exist or null
        if (c == null) throw new DalDoesNotExistException($"Courier with ID={id} does not exist");

        Console.WriteLine($"Current: {c}");
        // read new values (optional)
        string fullName = ReadOptional($"Full name [{c.CourierFullName}]: ", c.CourierFullName);
        string address = ReadOptional($"Courier address [{c.CourierAddress}]: ", c.CourierAddress);
        string phone = ReadOptional($"Phone [{c.CourierCellPhone}]: ", c.CourierCellPhone);
        string email = ReadOptional($"Email [{c.CourierEmail}]: ", c.CourierEmail);
        string password = ReadOptional($"Password [{c.CourierPassword}]: ", c.CourierPassword);

        // update active status
        bool isActive = c.CourierEnabled;
        Console.Write($"Active? (y/n, Enter=keep [{(c.CourierEnabled ? "y" : "n")}]): ");
        var activeIn = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
        if (activeIn is "y" or "yes" or "true") isActive = true;
        else if (activeIn is "n" or "no" or "false") isActive = false;

        // update distance
        double? distance = c.MaxCourierDistance;
        Console.Write($"Max distance km [current={(c.MaxCourierDistance?.ToString() ?? "null")}]. Enter=keep, '-'=null, or number: ");
        var distIn = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(distIn))
        {
            if (distIn.Trim() == "-") distance = null;
            else if (double.TryParse(distIn, out var d)) distance = d;
            else throw new DalInvalidIntegerException("Invalid number for distance.");
        }

        // update experience date
        DateTime? experience = c.SeniorityOfCourier;
        Console.Write($"Employment start date dd/MM/yyyy [current={(c.SeniorityOfCourier?.ToString("dd/MM/yyyy") ?? "null")}]. Enter=keep, '-'=null: ");
        var dateIn = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(dateIn))
        {
            if (dateIn.Trim() == "-") experience = null;
            else if (DateTime.TryParse(dateIn, out var dt)) experience = dt;
            else throw new DalInvalidDateException("Invalid date/time.");
        }

        // update vehicle type
        var vehicleType = c.CourierVehicleType;
        Console.WriteLine($"Vehicle type (Enter=keep [{c.CourierVehicleType}]): {string.Join(", ", Enum.GetNames<CourierVehicleType>())}");
        Console.Write("> ");
        var enumIn = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(enumIn))
        {
            if (!Enum.TryParse<CourierVehicleType>(enumIn, ignoreCase: true, out vehicleType))
                throw new DalInvalidVehicleTypeException("Invalid vehicle type.");
        }

        // create updated copy and save
        var updated = c with
        {
            CourierFullName = fullName,
            CourierAddress = address,
            CourierCellPhone = phone,
            CourierEmail = email,
            CourierPassword = password,
            CourierEnabled = isActive,
            MaxCourierDistance = distance,
            SeniorityOfCourier = experience,
            CourierVehicleType = vehicleType
        };

        s_dal.Courier.Update(updated);
        Console.WriteLine("Courier updated.");
    }

    /// <summary>
    /// Delete Courier by ID
    /// </summary>
    private static void DeleteCourier()
    {
        int id = ReadInt("Courier Id to delete: "); // read ID
        s_dal.Courier.Delete(id); // delete from DAL
        Console.WriteLine("Courier deleted.");
    }


    // -------------------- Order -------------------- \\

    /// <summary>
    /// Create a new Order and save it to the data layer
    /// </summary>
    private static void CreateOrder()
    {
        Console.WriteLine("Creating an Order...");
        string status = "[OPEN]"; // default order status
        string detail = ReadRequired("Detail/Description: "); // order description
        string address = ReadRequired("Address: "); // delivery address
        double lat = ReadDouble("Latitude: "); // latitude
        double lon = ReadDouble("Longitude: "); // longitude
        string customer = ReadRequired("Customer full name: "); // customer name
        string phone = ReadRequired("Customer phone: "); // customer phone
        double weight = ReadDouble("Weight (kg): "); // order weight
        bool fragile = ReadBool("Fragile (y/n): "); // is fragile
        double size = ReadDouble("Size (arbitrary units): "); // order size
        DateTime date = ReadDateTime("Order date (dd/MM/yy HH:mm:ss): "); // order date/time
        var kind = ReadEnum<TypeOfOrder>("Order kind (enum): "); // select order type

        // create new Order object
        var o = new Order(
            OrderId: 0, // DAL assigns running Id
            OrderStatus: status,
            OrderDetail: detail,
            OrderAddress: address,
            OrderLatitude: lat,
            OrderLongitude: lon,
            OrderCustomerFullName: customer,
            OrderCustomerPhone: phone,
            OrderWeight: weight,
            IsFragile: fragile,
            OrderSize: size,
            OrderDate: date,
            TypeOfOrder: kind
        );

        s_dal.Order.Create(o); // save to data layer
        Console.WriteLine("Order created.");
    }

    /// <summary>
    /// Read and display an Order by its ID
    /// </summary>
    /// <exception cref="DalDoesNotExistException"> Thrown if the Order does not exist </exception>
    private static void ReadOrder()
    {
        int id = ReadInt("Order Id: "); // read order Id
        var o = s_dal.Order.Read(id); // fetch order from DAL
        if (o == null) throw new DalDoesNotExistException($"Order with ID={id} does not exist");
        Console.WriteLine(o); // print order details
    }

    /// <summary>
    /// Display all Orders from the data layer
    /// </summary>
    private static void ReadAllOrders()
    {
        foreach (var o in s_dal.Order.ReadAll()) // loop through all orders
            Console.WriteLine($"{o} \n");
    }

    /// <summary>
    /// Update an existing Order by ID
    /// </summary>
    /// <exception cref="DalDoesNotExistException"> Thrown if the Order does not exist </exception>
    private static void UpdateOrder()
    {
        int id = ReadInt("Order Id to update: "); // read order Id
        var o = s_dal.Order.Read(id); // get existing order
        if (o == null) throw new DalDoesNotExistException($"Order with ID={id} does not exist");
        Console.WriteLine($"Current: {o}");
        

        // read updated (optional) fields
        string detail = ReadOptional($"Detail [{o.OrderDetail}]: ", o.OrderDetail);
        string address = ReadOptional($"Address [{o.OrderAddress}]: ", o.OrderAddress);
        double lat = ReadDoubleOptional($"Latitude [{o.OrderLatitude}]: ", o.OrderLatitude);
        double lon = ReadDoubleOptional($"Longitude [{o.OrderLongitude}]: ", o.OrderLongitude);
        string phone = ReadOptional($"Phone [{o.OrderCustomerPhone}]: ", o.OrderCustomerPhone);

        // create updated copy
        var updated = o with
        {
            OrderDetail = detail,
            OrderAddress = address,
            OrderLatitude = lat,
            OrderLongitude = lon,
            OrderCustomerPhone = phone
        };

        s_dal.Order.Update(updated); // update in DAL
        Console.WriteLine("Order updated.");
    }

    /// <summary>
    /// Delete an Order by its ID
    /// </summary>
    private static void DeleteOrder()
    {
        int id = ReadInt("Order Id to delete: "); // read order Id
        s_dal.Order.Delete(id); // delete from DAL
        Console.WriteLine("Order deleted.");
    }


    // -------------------- Delivery -------------------- \\

    /// <summary>
    /// Create a new Delivery and save it to the data layer
    /// </summary>
    private static void CreateDelivery()
    {
        Console.WriteLine("Creating a Delivery...");

        int orderId = ReadInt("Order Id to deliver: "); // order ID
        int courierId = ReadInt("Courier Id: "); // courier ID

        // optional max air distance
        Console.Write("Max air distance or Enter to skip: ");
        string? distInput = Console.ReadLine();
        double? deliveryMaxDistance = null;
        if (double.TryParse(distInput, out double dVal))
            deliveryMaxDistance = dVal;

        // timestamps
        DateTime deliveryDate = ReadDateTime("Pickup/Start date (dd/MM/yy HH:mm:ss): "); // start time
        DateTime deliveryFinishDate = ReadDateTime("Finish date (dd/MM/yy HH:mm:ss): "); // finish time

        // enums for shipment and finish type
        var shipType = ReadEnum<ShipmentType>("Shipment type: ");
        var finishType = ReadEnum<DeliveryFinishType>("Delivery finish type: ");

        // create new Delivery object
        var d = new Delivery(
            DeliveryId: 0, // running Id assigned by DAL
            OrderId: orderId,
            CourierId: courierId,
            DeliveryMaxDistance: deliveryMaxDistance,
            DeliveryDate: deliveryDate,
            DeliveryFinishDate: deliveryFinishDate,
            ShipmentType: shipType,
            DeliveryFinishType: finishType
        );

        s_dal.Delivery.Create(d); // save to DAL
        Console.WriteLine("Delivery created.");
    }

    /// <summary>
    /// Read and display a Delivery by its ID
    /// </summary>
    /// <exception cref="DalDoesNotExistException"> Thrown if the Delivery does not exist </exception>
    private static void ReadDelivery()
    {
        int id = ReadInt("Delivery Id: "); // read delivery Id
        var d = s_dal.Delivery.Read(id); // fetch delivery from DAL
        if (d == null) throw new DalDoesNotExistException($"Delivery with ID={id} does not exist");
        Console.WriteLine(d); // print delivery details
    }

    /// <summary>
    /// Display all Deliveries from the data layer
    /// </summary>
    private static void ReadAllDeliveries()
    {
        foreach (var d in s_dal.Delivery.ReadAll()) // loop through all deliveries
            Console.WriteLine($"{d} \n");
    }

    /// <summary>
    /// Update an existing Delivery by ID
    /// </summary>
    /// <exception cref="DalDoesNotExistException"> Thrown if the Delivery does not exist </exception>
    /// <exception cref="DalInvalidIntegerException"> Thrown if the number is invalid </exception>
    /// <exception cref="DalInvalidDateException"> Thrown if the Date is invalid </exception>
    /// <exception cref="DalInvalidShipmentTypeException"> Thrown if the Shipment Type is invalid </exception>
    /// <exception cref="DalInvalidDeliveryStatusException"> Thrown if the Delivery Status is invalid </exception>
    private static void UpdateDelivery()
    {
        // fetch current entity
        int id = ReadInt("Delivery Id to update: ");
        var d = s_dal.Delivery.Read(id);
        if (d == null) throw new DalDoesNotExistException($"Delivery with ID={id} does not exist");

        Console.WriteLine($"Current: {d}");

        // update basic IDs
        int orderId = ReadIntOptional($"Order Id [{d.OrderId}]: ", d.OrderId);
        int courierId = ReadIntOptional($"Courier Id [{d.CourierId}]: ", d.CourierId);

        // update optional double
        double? maxDist = d.DeliveryMaxDistance;
        Console.Write($"Max air distance [current={(d.DeliveryMaxDistance?.ToString() ?? "null")}]. Enter=keep, '-'=null, or number: ");
        var distIn = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(distIn))
        {
            if (distIn.Trim() == "-") maxDist = null;
            else if (double.TryParse(distIn, out var dd)) maxDist = dd;
            else throw new DalInvalidIntegerException("Invalid number for max distance.");
        }

        // update dates
        Console.Write($"Pickup/Start date (dd/MM/yy HH:mm:ss) [current={d.DeliveryDate:dd/MM/yy HH:mm:ss}] (Enter=keep): ");
        var startIn = Console.ReadLine();
        DateTime deliveryDate = d.DeliveryDate;
        if (!string.IsNullOrWhiteSpace(startIn))
        {
            if (!DateTime.TryParse(startIn, out deliveryDate))
                throw new DalInvalidDateException("Invalid start date/time.");
        }

        Console.Write($"Finish date (dd/MM/yy HH:mm:ss) [current={d.DeliveryFinishDate:dd/MM/yy HH:mm:ss}] (Enter=keep): ");
        var finIn = Console.ReadLine();
        DateTime deliveryFinishDate = d.DeliveryFinishDate;
        if (!string.IsNullOrWhiteSpace(finIn))
        {
            if (!DateTime.TryParse(finIn, out deliveryFinishDate))
                throw new DalInvalidDateException("Invalid finish date/time.");
        }

        // update enums
        var shipType = d.ShipmentType;
        Console.WriteLine($"Shipment type (Enter=keep [{d.ShipmentType}]): {string.Join(", ", Enum.GetNames<ShipmentType>())}");
        Console.Write("> ");
        var shipIn = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(shipIn))
        {
            if (!Enum.TryParse<ShipmentType>(shipIn, ignoreCase: true, out shipType))
                throw new DalInvalidShipmentTypeException("Invalid shipment type.");
        }

        var finishType = d.DeliveryFinishType;
        Console.WriteLine($"Delivery finish type (Enter=keep [{d.DeliveryFinishType}]): {string.Join(", ", Enum.GetNames<DeliveryFinishType>())}");
        Console.Write("> ");
        var finTypeIn = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(finTypeIn))
        {
            if (!Enum.TryParse<DeliveryFinishType>(finTypeIn, ignoreCase: true, out finishType))
                throw new DalInvalidDeliveryStatusException("Invalid delivery finish type.");
        }

        // apply changes and save
        var updated = d with
        {
            OrderId = orderId,
            CourierId = courierId,
            DeliveryMaxDistance = maxDist,
            DeliveryDate = deliveryDate,
            DeliveryFinishDate = deliveryFinishDate,
            ShipmentType = shipType,
            DeliveryFinishType = finishType
        };

        s_dal.Delivery.Update(updated); // update in DAL
        Console.WriteLine("Delivery updated.");
    }

    /// <summary>
    /// Delete a Delivery by its ID
    /// </summary>
    private static void DeleteDelivery()
    {
        int id = ReadInt("Delivery Id to delete: "); // read delivery Id
        s_dal.Delivery.Delete(id); // delete from DAL
        Console.WriteLine("Delivery deleted.");
    }


    // -------------------- Read helpers -------------------- \\

    /// <summary>
    /// Read a menu selection from the console and return it only if it is in [0..6].
    /// Keeps prompting until a valid integer in range is entered.
    /// </summary>
    /// <returns> The chosen integer between 0 and 6 (inclusive) </returns>
    private static int ReadIntOfMenu()
    {
        while (true)
        {
            // try to parse user input as integer
            if (int.TryParse(Console.ReadLine(), out int number))
            {
                // accept only if number is between 0 and 6
                if (number >= 0 && number <= 6) return number;
            }
            // otherwise, prompt again
            Console.WriteLine("\nTry again...");
            Console.Write("Choose: ");
        }
    }

    /// <summary>
    /// Read a required non-empty string (trims whitespace).
    /// Keeps prompting until a non-blank value is entered.
    /// </summary>
    private static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write(prompt); // show question
            string? check = Console.ReadLine(); // read user input
            if (!string.IsNullOrWhiteSpace(check)) return check.Trim(); // return trimmed text if not empty
            Console.WriteLine("Value is required."); // re-prompt if blank
        }
    }

    /// <summary>
    /// Read an optional string; if empty, return the provided current value.
    /// </summary>
    private static string ReadOptional(string prompt, string current)
    {
        Console.Write(prompt);
        string? s = Console.ReadLine(); // read line
        return string.IsNullOrWhiteSpace(s) ? current : s.Trim(); // keep current if blank
    }

    /// <summary>
    /// Read an integer (any valid 32-bit int). Keeps prompting until parse succeeds.
    /// </summary>
    private static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt); // ask for number
            if (int.TryParse(Console.ReadLine(), out int v))
            {
                /*if (200000000 < v && 400000000 > v)*/
                return v; // could add validation range if needed
            }
            Console.WriteLine("Invalid integer. Try again.");
        }
    }

    /// <summary>
    /// Read a floating-point number (double). Keeps prompting until parse succeeds.
    /// </summary>
    private static double ReadDouble(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (double.TryParse(Console.ReadLine(), out double v)) return v; // valid number
            Console.WriteLine("Invalid number. Try again.");
        }
    }

    /// <summary>
    /// Read an optional double (floating-point number) from the console.  
    /// If the user presses Enter without typing anything, the existing value is kept.  
    /// Throws an exception if a non-blank input is not a valid number.
    /// </summary>
    /// <param name="prompt">The message displayed to the user before reading input.</param>
    /// <param name="current">The current value to return if the user leaves the input blank.</param>
    /// <returns>The parsed double value entered by the user, or the current value if left blank.</returns>
    /// <exception cref="DalInvalidIntegerException">Thrown if the user enters an invalid number.</exception>
    private static double ReadDoubleOptional(string prompt, double current)
    {
        Console.Write(prompt);
        var s = Console.ReadLine(); // read text
        if (string.IsNullOrWhiteSpace(s)) return current; // keep old value
        if (double.TryParse(s, out double v)) return v; // parse ok
        throw new DalInvalidIntegerException("Invalid number."); // invalid number
    }

    /// <summary>
    /// Read an optional integer value from the console.  
    /// If the user presses Enter without typing anything, the existing value is kept.  
    /// Throws an exception if a non-blank input is not a valid integer.
    /// </summary>
    /// <param name="prompt">The message displayed to the user before reading input.</param>
    /// <param name="current">The current integer value to return if the user leaves the input blank.</param>
    /// <returns>The parsed integer entered by the user, or the current value if left blank.</returns>
    /// <exception cref="DalInvalidIntegerException">Thrown if the user enters an invalid integer value.</exception>
    private static int ReadIntOptional(string prompt, int current)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s)) return current; // keep current if empty
        if (int.TryParse(s, out int v)) return v; // parse success
        throw new DalInvalidIntegerException("Invalid integer."); // invalid integer
    }

    /// <summary>
    /// Reads a yes/no (boolean) value from the console.  
    /// Accepts "y", "yes", or "true" (case-insensitive) as <c>true</c>,  
    /// and "n", "no", or "false" as <c>false</c>.  
    /// Keeps prompting the user until a valid value is entered.
    /// </summary>
    /// <param name="prompt">The message displayed to the user before reading input.</param>
    /// <returns><c>true</c> if the user enters a positive response (yes/true); otherwise, <c>false</c>.</returns>
    private static bool ReadBool(string prompt)
    {
        while (true)
        {
            Console.Write(prompt); // ask question
            var s = (Console.ReadLine() ?? "").Trim().ToLowerInvariant(); // normalize input
            if (s is "y" or "yes" or "true") return true;
            if (s is "n" or "no" or "false") return false;
            Console.WriteLine("Please enter yes/no."); // retry if invalid
        }
    }

    /// <summary>
    /// Reads a date and time value from the console.  
    /// Keeps prompting the user until a valid <see cref="DateTime"/> is entered.  
    /// Example of valid input: <c>31/12/24 13:45:00</c>.
    /// </summary>
    /// <param name="prompt">The message displayed to the user before reading input.</param>
    /// <returns>The parsed <see cref="DateTime"/> value entered by the user.</returns>
    private static DateTime ReadDateTime(string prompt)
    {
        while (true)
        {
            Console.Write(prompt); // show example format
            if (DateTime.TryParse(Console.ReadLine(), out DateTime dt)) return dt; // valid date
            Console.WriteLine("Invalid date/time. Expected e.g. 31/12/24 13:45:00");
        }
    }

    /// <summary>
    /// Reads a value of the specified enumeration type from the console.  
    /// Displays all available enum options and keeps prompting the user  
    /// until a valid value is entered (case-insensitive).
    /// </summary>
    /// <typeparam name="TEnum">The enumeration type to read from user input.</typeparam>
    /// <param name="prompt">The message displayed to the user before reading input.</param>
    /// <returns>The parsed enum value of type <typeparamref name="TEnum"/>.</returns>
    private static TEnum ReadEnum<TEnum>(string prompt) where TEnum : struct, Enum
    {
        while (true)
        {
            // show available options dynamically
            Console.Write($"{prompt}{Environment.NewLine}Options: {string.Join(", ", Enum.GetNames<TEnum>())}{Environment.NewLine}> ");
            var s = Console.ReadLine(); // read user input
            if (Enum.TryParse<TEnum>(s, ignoreCase: true, out var val)) return val; // success
            Console.WriteLine("Invalid enum value."); // try again
        }
    }

}