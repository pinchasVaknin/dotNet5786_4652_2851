using BlApi;
using BO;

namespace BlTest
{
    internal class Program
    {
        // ======================== FIELDS ======================== \\

        #region Fields

        // Get BL instance from factory
        static readonly IBl s_bl = Factory.Get();

        // Current logged-in user ID (-1 means no user logged in)
        static int s_currentUserId = -1;

        #endregion Fields

        // ======================== MAIN METHOD ======================== \\

        static void Main(string[] args)
        {
            Console.WriteLine(@"//==== Delivery System BL Test ====\\");

            try { s_bl.Courier.Login(333333333, "ChangeMe!1234"); }

            catch (BO.BlUserNotFoundException)
            {
                Console.Write("Database is empty (DalList detected). Initializing... ");
                s_bl.Admin.InitializeDB();
                Console.WriteLine("Done.");
            }
            catch (Exception) { }

            while (true)
            {

                try
                {
                    if (s_currentUserId == -1)
                    {
                        Login();
                    }


                    Console.WriteLine($"\n//==== Main Menu (Logged in as: {s_currentUserId}) =====\\\\");
                    Console.WriteLine("1. Admin Operations");
                    Console.WriteLine("2. Courier Operations");
                    Console.WriteLine("3. Order Operations");
                    Console.WriteLine("9. Logout (Switch User)");
                    Console.WriteLine("0. Exit Program");

                    int choice = SafeReadInt("Choose option: ");

                    switch (choice)
                    {
                        case 1: AdminMenu(); break;
                        case 2: CourierMenu(); break;
                        case 3: OrderMenu(); break;
                        case 9: s_currentUserId = -1; break;
                        case 0: return;
                        default: Console.WriteLine("Invalid option."); break;
                    }
                }
                catch (Exception ex)
                {
                    PrintException(ex);
                }
            }
        }

        // ======================== LOGIN ======================== \\
        private static void Login()
        {
            Console.WriteLine("\n//==== Login ====\\\\");
            bool loggedIn = false;
            while (!loggedIn)
            {
                try
                {

                    int id = SafeReadInt("Enter User ID: ");
                    Console.Write("Enter Password: ");
                    string pass = Console.ReadLine() ?? "";


                    UserRole role = s_bl.Courier.Login(id, pass);

                    s_currentUserId = id;
                    loggedIn = true;
                    Console.WriteLine($"Login Successful!\n \nRole: {role}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Login Failed: {ex.Message}. Try again.");
                }
            }
        }

        // ======================== ADMIN MENU ======================== \\
        private static void AdminMenu()
        {
            Console.WriteLine("\n--- Admin Operations ---");
            Console.WriteLine("1. Initialize Database (Seed Data)");
            Console.WriteLine("2. Reset Database (Clear All)");
            Console.WriteLine("3. Show System Clock");
            Console.WriteLine("4. Forward Clock");
            Console.WriteLine("5. Show Configuration");
            Console.WriteLine("6. Update Config (MaxAirDistance)");
            Console.WriteLine("0. Back");

            int choice = SafeReadInt("Choose option: ");
            try
            {
                switch (choice)
                {
                    case 1:
                        Console.Write("Initializing...");
                        s_bl.Admin.InitializeDB();
                        Console.WriteLine(" Done.");
                        break;
                    case 2:
                        Console.Write("Resetting...");
                        s_bl.Admin.ResetDB();
                        s_currentUserId = -1;
                        Console.WriteLine(" Done. You have been logged out.");
                        break;
                    case 3:
                        Console.WriteLine($"Current Clock: {s_bl.Admin.GetClock()}");
                        break;
                    case 4:
                        TimeUnit unit = SafeReadEnum<TimeUnit>("Select time unit to advance:");
                        s_bl.Admin.ForwardClock(unit);
                        Console.WriteLine($"Clock forwarded. New time: {s_bl.Admin.GetClock()}");
                        break;
                    case 5:
                        Console.WriteLine(s_bl.Admin.GetConfig());
                        break;
                    case 6:
                        double newDist = SafeReadDouble("Enter new Max Air Distance: ");
                        var conf = s_bl.Admin.GetConfig();
                        conf.MaxAirDistance = newDist;
                        s_bl.Admin.SetConfig(conf);
                        Console.WriteLine("Config updated.");
                        break;
                    case 0: return;
                    default: Console.WriteLine("Invalid option."); break;
                }
            }
            catch (Exception ex) { PrintException(ex); }
        }

        // ======================== COURIER MENU ======================== \\
        private static void CourierMenu()
        {
            Console.WriteLine("\n--- Courier Operations ---");
            Console.WriteLine("1. Add Courier");
            Console.WriteLine("2. Get Courier Details");
            Console.WriteLine("3. Update Courier");
            Console.WriteLine("4. Delete Courier");
            Console.WriteLine("5. Get Couriers List");
            Console.WriteLine("0. Back");

            int choice = SafeReadInt("Choose option: ");
            try
            {
                switch (choice)
                {
                    case 1:
                        BO.Courier newC = new BO.Courier
                        {
                            CourierId = SafeReadInt("Enter ID (Teudat Zehut): "),
                            CourierFullName = SafeReadString("Name: "),
                            CourierCellPhone = SafeReadString("Phone: "),
                            CourierEmail = SafeReadString("Email: "),
                            CourierPassword = SafeReadString("Password: "),
                            VehicleType = SafeReadEnum<VehicleType>("Vehicle Type:"),
                            MaxCourierDistance = SafeReadDouble("Max Distance: "),
                            CourierLocation = SafeReadString("Location: "),
                            CourierIsActive = true,
                            StartWorkDate = DateTime.Now
                        };
                        s_bl.Courier.AddCourier(s_currentUserId, newC);
                        Console.WriteLine("Courier added successfully.");
                        break;

                    case 2:
                        int idGet = SafeReadInt("Enter Courier ID: ");
                        Console.WriteLine(s_bl.Courier.GetCourier(s_currentUserId, idGet));
                        break;

                    case 3:
                        int idUpd = SafeReadInt("Enter Courier ID to update: ");
                        var cToUpd = s_bl.Courier.GetCourier(s_currentUserId, idUpd);
                        Console.WriteLine($"Updating {cToUpd.CourierFullName}.");


                        cToUpd.CourierFullName = SafeReadString($"Enter new name (current: {cToUpd.CourierFullName}): ");
                        cToUpd.CourierCellPhone = SafeReadString($"Enter new phone (current: {cToUpd.CourierCellPhone}): ");

                        s_bl.Courier.UpdateCourier(s_currentUserId, cToUpd);
                        Console.WriteLine("Courier updated.");
                        break;

                    case 4:
                        int idDel = SafeReadInt("Enter Courier ID to delete: ");
                        s_bl.Courier.DeleteCourier(s_currentUserId, idDel);
                        Console.WriteLine("Courier deleted.");
                        break;

                    case 5:

                        IEnumerable<CourierInList> list = s_bl.Courier.GetCouriers(s_currentUserId);
                        foreach (var item in list)
                            Console.WriteLine(item);
                        break;

                    case 0: return;
                    default: Console.WriteLine("Invalid option."); break;
                }
            }
            catch (Exception ex) { PrintException(ex); }
        }

        // ======================== ORDER MENU ======================== \\
        private static void OrderMenu()
        {
            Console.WriteLine("\n--- Order Operations ---");
            Console.WriteLine("1. Add Order");
            Console.WriteLine("2. Get Order Details");
            Console.WriteLine("3. Update Order");
            Console.WriteLine("4. Cancel Order");
            Console.WriteLine("5. Assign Order to Courier");
            Console.WriteLine("6. Complete Order (Delivery Supplied)");
            Console.WriteLine("7. Get Orders List (Admin)");
            Console.WriteLine("8. Get Open Orders for Courier");
            Console.WriteLine("9. Get Status Summary");
            Console.WriteLine("0. Back");

            int choice = SafeReadInt("Choose option: ");
            try
            {
                switch (choice)
                {

                    case 1:
                        // 1. שליפת הזמן הנוכחי של המערכת (השעון המדומה)
                        DateTime now = s_bl.Admin.GetClock();

                        BO.Order newO = new BO.Order
                        {
                            OrderId = 0,
                            CustomerFullName = SafeReadString("Customer Name: "),
                            CustomerPhone = SafeReadString("Customer Phone: "),
                            OrderAddress = SafeReadString("Address: "),
                            TypeOfOrder = SafeReadEnum<TypeOfOrder>("Product Type:"),

                            OrderLatitude = 0,
                            OrderLongitude = 0,

                            OrderOpenTime = now, 
                            MaxDeliveryTime = now.AddDays(14), 

                            OrderWeight = SafeReadDouble("Weight: "),
                            OrderSize = SafeReadDouble("Size: "),
                            IsFragile = false,
                            OrderDetail = "Test Order",
                            OrderStatus = BO.OrderStatus.Open
                        };

                        s_bl.Order.AddOrder(s_currentUserId, newO);
                        Console.WriteLine("Order added.");
                        break;

                    case 2:
                        int oId = SafeReadInt("Enter Order ID: ");
                        Console.WriteLine(s_bl.Order.GetOrder(s_currentUserId, oId));
                        break;

                    case 3:
                        int oIdUpd = SafeReadInt("Enter Order ID to update: ");
                        var oToUpd = s_bl.Order.GetOrder(s_currentUserId, oIdUpd);
                        oToUpd.CustomerFullName = SafeReadString("New Customer Name: ");
                        s_bl.Order.UpdateOrder(s_currentUserId, oToUpd);
                        Console.WriteLine("Order updated.");
                        break;

                    case 4:
                        int oIdCancel = SafeReadInt("Enter Order ID to cancel: ");
                        s_bl.Order.CancelOrder(s_currentUserId, oIdCancel);
                        Console.WriteLine("Order canceled.");
                        break;

                    case 5:
                        int cIdAssign = SafeReadInt("Enter Courier ID: ");
                        int oIdAssign = SafeReadInt("Enter Order ID: ");
                        s_bl.Order.AssignOrderToCourier(s_currentUserId, cIdAssign, oIdAssign);
                        Console.WriteLine("Order assigned.");
                        break;

                    case 6:
                        int cIdComp = SafeReadInt("Enter Courier ID: ");
                        int dIdComp = SafeReadInt("Enter Delivery ID: ");
                        s_bl.Order.CompleteOrderHandling(s_currentUserId, cIdComp, dIdComp);
                        Console.WriteLine("Order supplied.");
                        break;

                    case 7:
                        foreach (var item in s_bl.Order.GetOrders(s_currentUserId))
                            Console.WriteLine(item);
                        break;

                    case 8:
                        int cIdForOpen = SafeReadInt("Enter Courier ID: ");
                        foreach (var item in s_bl.Order.GetOpenOrdersForCourier(s_currentUserId, cIdForOpen))
                            Console.WriteLine(item);
                        break;

                    case 9:
                        int[] stats = s_bl.Order.GetOrderStatusSummary(s_currentUserId);
                        Console.WriteLine("Stats array:");
                        for (int i = 0; i < stats.Length; i++) Console.Write($" [{i}]:{stats[i]} ");
                        Console.WriteLine();
                        break;

                    case 0: return;
                    default: Console.WriteLine("Invalid option."); break;
                }
            }
            catch (Exception ex) { PrintException(ex); }
        }

        // ================ HELPER METHODS (INPUT VALIDATION) ================ \\

        #region Helper Methods

        private static int SafeReadInt(string prompt)
        {
            Console.Write(prompt);
            int result;
            while (!int.TryParse(Console.ReadLine(), out result))
            {
                Console.WriteLine("Invalid input. Please enter an integer.");
                Console.Write(prompt);
            }
            return result;
        }

        private static double SafeReadDouble(string prompt)
        {
            Console.Write(prompt);
            double result;
            while (!double.TryParse(Console.ReadLine(), out result))
            {
                Console.WriteLine("Invalid input. Please enter a number.");
                Console.Write(prompt);
            }
            return result;
        }

        private static string SafeReadString(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine() ?? "";
        }

        private static T SafeReadEnum<T>(string prompt) where T : struct, Enum
        {
            Console.WriteLine(prompt);
            foreach (var val in Enum.GetValues<T>())
            {
                Console.WriteLine($" - {val}");
            }

            Console.Write("Enter value: ");
            T result;
            while (!Enum.TryParse(Console.ReadLine(), out result) || !Enum.IsDefined(typeof(T), result))
            {
                Console.WriteLine("Invalid choice. Try again.");
                Console.Write("Enter value: ");
            }
            return result;
        }

        private static void PrintException(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[ERROR]: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"[Details]: {ex.InnerException.Message}");

            Console.WriteLine($"[Type]: {ex.GetType().Name}\n");
            Console.ResetColor();
        }
    }

    #endregion Helper Methods


}