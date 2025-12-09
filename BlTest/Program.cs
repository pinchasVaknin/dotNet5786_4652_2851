namespace BlTest;
using BlApi;
using BO;

internal class Program
{
    // Initialize the logical layer interface using Factory
    static readonly IBl s_bl = BlApi.Factory.Get();

    // -------------------- Main -------------------- \\
    static void Main(string[] args)
    {
        // Always start with a clean console and show the clock
        Console.Clear();
        Console.WriteLine($"Clock: {s_bl.Admin.GetClock():yyyy-MM-dd HH:mm:ss}");
        
        // Show the root menu until user chooses Exit
        while (true)
        {
            try
            {
                switch (RootMenu())
                {
                    case 0:
                        Console.WriteLine("Bye!");
                        return;
                    case 1:
                        AdminMenu();
                        break;
                    case 2:
                        CourierMenu();
                        break;
                    case 3:
                        OrderMenu();
                        break;
                    default:
                        Console.WriteLine("Unknown option.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex}");
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.ReadLine();
            Console.Clear();
        }
    }

    // -------------------- Menus -------------------- \\

    /// <summary>
    /// Displays the main root menu with all logical service entities.
    /// Waits for the user's numeric choice and returns it.
    /// </summary>
    /// <returns>The selected menu option as an integer.</returns>
    private static int RootMenu()
    {
        Console.WriteLine(@"// ==== MAIN MENU ==== \\");
        Console.WriteLine("1) Admin");
        Console.WriteLine("2) Courier");
        Console.WriteLine("3) Order");
        Console.WriteLine("0) Exit");
        Console.Write("Choose: ");
        return ReadIntOfMenu();
    }

    /// <summary>
    /// Opens the Admin submenu, allowing the user to test all Admin operations.
    /// Loops until the user chooses to return to the previous menu.
    /// </summary>
    private static void AdminMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("---- ADMIN MENU ----");
            var number = AdminSubMenu();

            switch (number)
            {
                case 0:
                    return;
                case 1:
                    TestGetClock();
                    break;
                case 2:
                    TestForwardClock();
                    break;
                case 3:
                    TestGetConfig();
                    break;
                case 4:
                    TestSetConfig();
                    break;
                case 5:
                    TestResetDB();
                    break;
                case 6:
                    TestInitializeDB();
                    break;
                default:
                    Console.WriteLine("Unknown option.");
                    break;
            }

            Console.WriteLine("\nPress ENTER...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Displays the Admin submenu with all Admin operations.
    /// </summary>
    /// <returns>The selected submenu option as an integer.</returns>
    private static int AdminSubMenu()
    {
        Console.WriteLine("0) Back");
        Console.WriteLine("1) GetClock");
        Console.WriteLine("2) ForwardClock");
        Console.WriteLine("3) GetConfig");
        Console.WriteLine("4) SetConfig");
        Console.WriteLine("5) ResetDB");
        Console.WriteLine("6) InitializeDB");
        Console.Write("Choose: ");
        return ReadIntOfMenu();
    }

    /// <summary>
    /// Opens the Courier submenu, allowing the user to test all Courier operations.
    /// Loops until the user chooses to return to the previous menu.
    /// </summary>
    private static void CourierMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("---- COURIER MENU ----");
            var number = CourierSubMenu();

            switch (number)
            {
                case 0:
                    return;
                case 1:
                    TestLogin();
                    break;
                case 2:
                    TestGetCouriers();
                    break;
                case 3:
                    TestGetCourier();
                    break;
                case 4:
                    TestAddCourier();
                    break;
                case 5:
                    TestUpdateCourier();
                    break;
                case 6:
                    TestDeleteCourier();
                    break;
                default:
                    Console.WriteLine("Unknown option.");
                    break;
            }

            Console.WriteLine("\nPress ENTER...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Displays the Courier submenu with all Courier operations.
    /// </summary>
    /// <returns>The selected submenu option as an integer.</returns>
    private static int CourierSubMenu()
    {
        Console.WriteLine("0) Back");
        Console.WriteLine("1) Login");
        Console.WriteLine("2) GetCouriers");
        Console.WriteLine("3) GetCourier");
        Console.WriteLine("4) AddCourier");
        Console.WriteLine("5) UpdateCourier");
        Console.WriteLine("6) DeleteCourier");
        Console.Write("Choose: ");
        return ReadIntOfMenu();
    }

    /// <summary>
    /// Opens the Order submenu, allowing the user to test all Order operations.
    /// Loops until the user chooses to return to the previous menu.
    /// </summary>
    private static void OrderMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("---- ORDER MENU ----");
            var number = OrderSubMenu();

            switch (number)
            {
                case 0:
                    return;
                case 1:
                    TestGetOrderStatusSummary();
                    break;
                case 2:
                    TestGetOrders();
                    break;
                case 3:
                    TestGetOrder();
                    break;
                case 4:
                    TestUpdateOrder();
                    break;
                case 5:
                    TestCancelOrder();
                    break;
                case 6:
                    TestDeleteOrder();
                    break;
                case 7:
                    TestAddOrder();
                    break;
                case 8:
                    TestCompleteOrderHandling();
                    break;
                case 9:
                    TestAssignOrderToCourier();
                    break;
                case 10:
                    TestGetClosedDeliveriesByCourier();
                    break;
                case 11:
                    TestGetOpenOrdersForCourier();
                    break;
                default:
                    Console.WriteLine("Unknown option.");
                    break;
            }

            Console.WriteLine("\nPress ENTER...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Displays the Order submenu with all Order operations.
    /// </summary>
    /// <returns>The selected submenu option as an integer.</returns>
    private static int OrderSubMenu()
    {
        Console.WriteLine("0) Back");
        Console.WriteLine("1) GetOrderStatusSummary");
        Console.WriteLine("2) GetOrders");
        Console.WriteLine("3) GetOrder");
        Console.WriteLine("4) UpdateOrder");
        Console.WriteLine("5) CancelOrder");
        Console.WriteLine("6) DeleteOrder");
        Console.WriteLine("7) AddOrder");
        Console.WriteLine("8) CompleteOrderHandling");
        Console.WriteLine("9) AssignOrderToCourier");
        Console.WriteLine("10) GetClosedDeliveriesByCourier");
        Console.WriteLine("11) GetOpenOrdersForCourier");
        Console.Write("Choose: ");
        return ReadIntOfMenu();
    }

    // -------------------- Admin Operations -------------------- \\

    /// <summary>
    /// Tests the GetClock operation.
    /// </summary>
    private static void TestGetClock()
    {
        try
        {
            DateTime clock = s_bl.Admin.GetClock();
            Console.WriteLine(clock);
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the ForwardClock operation.
    /// </summary>
    private static void TestForwardClock()
    {
        try
        {
            Console.WriteLine("Available time units: Minute, Hour, Day, Month, Year");
            Console.Write("Enter time unit: ");
            string? unitInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(unitInput))
            {
                Console.WriteLine("Time unit is required.");
                return;
            }

            if (Enum.TryParse<TimeUnit>(unitInput, ignoreCase: true, out TimeUnit unit))
            {
                s_bl.Admin.ForwardClock(unit);
                Console.WriteLine($"Clock forwarded by {unit}.");
                Console.WriteLine($"New clock: {s_bl.Admin.GetClock()}");
            }
            else
            {
                Console.WriteLine("Invalid time unit.");
            }
        }
        catch (BlUnknownTimeUnitException ex)
        {
            Console.WriteLine($"BlUnknownTimeUnitException: {ex.Message}");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the GetConfig operation.
    /// </summary>
    private static void TestGetConfig()
    {
        try
        {
            Config configuration = s_bl.Admin.GetConfig();
            Console.WriteLine("Configuration:");
            Console.WriteLine($"Clock = {configuration.Clock}");
            Console.WriteLine($"AdminId = {configuration.AdminId}");
            Console.WriteLine($"AdminPassword = {configuration.AdminPassword}");
            Console.WriteLine($"CompanyAddress = {configuration.CompanyAddress ?? "null"}");
            Console.WriteLine($"Latitude = {configuration.Latitude?.ToString() ?? "null"}");
            Console.WriteLine($"Longitude = {configuration.Longitude?.ToString() ?? "null"}");
            Console.WriteLine($"MaxAirDistance = {configuration.MaxAirDistance?.ToString() ?? "null"}");
            Console.WriteLine($"AvgCarSpeed = {configuration.AvgCarSpeed}");
            Console.WriteLine($"AvgMotorcycleSpeed = {configuration.AvgMotorcycleSpeed}");
            Console.WriteLine($"AvgBicycleSpeed = {configuration.AvgBicycleSpeed}");
            Console.WriteLine($"AvgWalkSpeed = {configuration.AvgWalkSpeed}");
            Console.WriteLine($"MaxDelTimeRnge = {configuration.MaxDelTimeRnge}");
            Console.WriteLine($"RiskTimeRnge = {configuration.RiskTimeRnge}");
            Console.WriteLine($"UnactiveTimeRnge = {configuration.UnactiveTimeRnge}");
        }
        catch (BlInvalidIntegerException ex)
        {
            Console.WriteLine($"BlInvalidIntegerException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"BlXMLFileLoadCreateException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the SetConfig operation.
    /// </summary>
    private static void TestSetConfig()
    {
        try
        {
            Config currentConfig = s_bl.Admin.GetConfig();
            Console.WriteLine("Current configuration:");
            Console.WriteLine($"MaxAirDistance = {currentConfig.MaxAirDistance?.ToString() ?? "null"}");
            
            Console.Write("Enter new MaxAirDistance (or press Enter to skip): ");
            string? maxDistInput = Console.ReadLine();
            
            Config newConfig = new Config
            {
                Clock = currentConfig.Clock,
                AdminId = currentConfig.AdminId,
                AdminPassword = currentConfig.AdminPassword,
                CompanyAddress = currentConfig.CompanyAddress,
                Latitude = currentConfig.Latitude,
                Longitude = currentConfig.Longitude,
                MaxAirDistance = currentConfig.MaxAirDistance,
                AvgCarSpeed = currentConfig.AvgCarSpeed,
                AvgMotorcycleSpeed = currentConfig.AvgMotorcycleSpeed,
                AvgBicycleSpeed = currentConfig.AvgBicycleSpeed,
                AvgWalkSpeed = currentConfig.AvgWalkSpeed,
                MaxDelTimeRnge = currentConfig.MaxDelTimeRnge,
                RiskTimeRnge = currentConfig.RiskTimeRnge,
                UnactiveTimeRnge = currentConfig.UnactiveTimeRnge
            };

            if (!string.IsNullOrWhiteSpace(maxDistInput))
            {
                if (double.TryParse(maxDistInput, out double maxDist))
                {
                    newConfig.MaxAirDistance = maxDist;
                }
                else
                {
                    Console.WriteLine("Invalid number format for MaxAirDistance.");
                    return;
                }
            }

            s_bl.Admin.SetConfig(newConfig);
            Console.WriteLine("Configuration updated.");
            
            Config updatedConfig = s_bl.Admin.GetConfig();
            Console.WriteLine("Updated configuration:");
            Console.WriteLine($"MaxAirDistance = {updatedConfig.MaxAirDistance?.ToString() ?? "null"}");
        }
        catch (BlInvalidDoubleException ex)
        {
            Console.WriteLine($"BlInvalidDoubleException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlInvalidIntegerException ex)
        {
            Console.WriteLine($"BlInvalidIntegerException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"BlXMLFileLoadCreateException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the ResetDB operation.
    /// </summary>
    private static void TestResetDB()
    {
        try
        {
            s_bl.Admin.ResetDB();
            Console.WriteLine("Database reset completed.");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the InitializeDB operation.
    /// </summary>
    private static void TestInitializeDB()
    {
        try
        {
            s_bl.Admin.InitializeDB();
            Console.WriteLine("Database initialization completed.");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    // -------------------- Courier Operations -------------------- \\

    /// <summary>
    /// Tests the Login operation.
    /// </summary>
    private static void TestLogin()
    {
        try
        {
            Console.Write("Enter user ID: ");
            string? userIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(userIdInput))
            {
                Console.WriteLine("User ID is required.");
                return;
            }

            if (!int.TryParse(userIdInput, out int userId))
            {
                Console.WriteLine("Invalid user ID format. Must be a number.");
                return;
            }

            Console.Write("Enter password: ");
            string? password = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Password is required.");
                return;
            }

            UserRole role = s_bl.Courier.Login(userId, password);
            Console.WriteLine($"Login successful. User role: {role}");
        }
        catch (BlInvalidPasswordException ex)
        {
            Console.WriteLine($"BlInvalidPasswordException: {ex.Message}");
        }
        catch (BlUserNotFoundException ex)
        {
            Console.WriteLine($"BlUserNotFoundException: {ex.Message}");
        }
        catch (BlInvalidIntegerException ex)
        {
            Console.WriteLine($"BlInvalidIntegerException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlXMLFileLoadCreateException ex)
        {
            Console.WriteLine($"BlXMLFileLoadCreateException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the GetCouriers operation.
    /// </summary>
    private static void TestGetCouriers()
    {
        try
        {
            Console.Write("Enter requester ID (admin): ");
            string? requesterIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(requesterIdInput))
            {
                Console.WriteLine("Requester ID is required.");
                return;
            }

            if (!int.TryParse(requesterIdInput, out int requesterId))
            {
                Console.WriteLine("Invalid requester ID format. Must be a number.");
                return;
            }

            Console.Write("Filter by active status? (true/false/null, press Enter for null): ");
            string? activeFilterInput = Console.ReadLine();
            bool? isActiveFilter = null;
            
            if (!string.IsNullOrWhiteSpace(activeFilterInput))
            {
                if (bool.TryParse(activeFilterInput, out bool activeValue))
                {
                    isActiveFilter = activeValue;
                }
                else if (activeFilterInput.ToLower() == "null")
                {
                    isActiveFilter = null;
                }
                else
                {
                    Console.WriteLine("Invalid boolean value. Using null (no filter).");
                }
            }

            Console.Write("Sort by? (CourierId/CourierFullName/CourierIsActive/VehicleType/StartWorkDate/DeliveriesInTime/DeliveriesOverTime/OrderIdInHandle, press Enter for null): ");
            string? sortByInput = Console.ReadLine();
            CourierListSortBy? sortBy = null;
            
            if (!string.IsNullOrWhiteSpace(sortByInput))
            {
                if (Enum.TryParse<CourierListSortBy>(sortByInput, ignoreCase: true, out CourierListSortBy sortValue))
                {
                    sortBy = sortValue;
                }
                else
                {
                    Console.WriteLine("Invalid sort option. Using null (default sort).");
                }
            }

            IEnumerable<CourierInList> couriers = s_bl.Courier.GetCouriers(requesterId, isActiveFilter, sortBy);
            
            foreach (var courier in couriers)
            {
                Console.WriteLine(courier);
            }
        }
        catch (BlAdminPermissionException ex)
        {
            Console.WriteLine($"BlAdminPermissionException: {ex.Message}");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the GetCourier operation.
    /// </summary>
    private static void TestGetCourier()
    {
        try
        {
            Console.Write("Enter requester ID: ");
            string? requesterIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(requesterIdInput))
            {
                Console.WriteLine("Requester ID is required.");
                return;
            }

            if (!int.TryParse(requesterIdInput, out int requesterId))
            {
                Console.WriteLine("Invalid requester ID format. Must be a number.");
                return;
            }

            Console.Write("Enter courier ID: ");
            string? courierIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(courierIdInput))
            {
                Console.WriteLine("Courier ID is required.");
                return;
            }

            if (!int.TryParse(courierIdInput, out int courierId))
            {
                Console.WriteLine("Invalid courier ID format. Must be a number.");
                return;
            }

            Courier courier = s_bl.Courier.GetCourier(requesterId, courierId);
            Console.WriteLine(courier);
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlAdminPermissionException ex)
        {
            Console.WriteLine($"BlAdminPermissionException: {ex.Message}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the AddCourier operation.
    /// </summary>
    private static void TestAddCourier()
    {
        try
        {
            Console.Write("Enter requester ID (admin): ");
            string? requesterIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(requesterIdInput))
            {
                Console.WriteLine("Requester ID is required.");
                return;
            }

            if (!int.TryParse(requesterIdInput, out int requesterId))
            {
                Console.WriteLine("Invalid requester ID format. Must be a number.");
                return;
            }

            Console.Write("Enter courier ID: ");
            string? courierIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(courierIdInput))
            {
                Console.WriteLine("Courier ID is required.");
                return;
            }

            if (!int.TryParse(courierIdInput, out int courierId))
            {
                Console.WriteLine("Invalid courier ID format. Must be a number.");
                return;
            }

            Console.Write("Enter courier full name: ");
            string? fullName = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(fullName))
            {
                Console.WriteLine("Full name is required.");
                return;
            }

            Console.Write("Enter courier cell phone: ");
            string? cellPhone = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(cellPhone))
            {
                Console.WriteLine("Cell phone is required.");
                return;
            }

            Console.Write("Enter courier email: ");
            string? email = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("Email is required.");
                return;
            }

            Console.Write("Enter courier password: ");
            string? password = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Password is required.");
                return;
            }

            Console.Write("Is courier active? (true/false): ");
            string? isActiveInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(isActiveInput))
            {
                Console.WriteLine("Active status is required.");
                return;
            }

            if (!bool.TryParse(isActiveInput, out bool isActive))
            {
                Console.WriteLine("Invalid boolean value.");
                return;
            }

            Console.Write("Enter courier location: ");
            string? location = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(location))
            {
                Console.WriteLine("Location is required.");
                return;
            }

            Console.Write("Enter max courier distance (or press Enter to skip): ");
            string? maxDistInput = Console.ReadLine();
            double? maxDistance = null;
            
            if (!string.IsNullOrWhiteSpace(maxDistInput))
            {
                if (!double.TryParse(maxDistInput, out double maxDist))
                {
                    Console.WriteLine("Invalid number format for max distance.");
                    return;
                }
                maxDistance = maxDist;
            }

            Console.WriteLine("Available vehicle types: Car, Motorcycle, Bicycle, Foot");
            Console.Write("Enter vehicle type: ");
            string? vehicleTypeInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(vehicleTypeInput))
            {
                Console.WriteLine("Vehicle type is required.");
                return;
            }

            if (!Enum.TryParse<VehicleType>(vehicleTypeInput, ignoreCase: true, out VehicleType vehicleType))
            {
                Console.WriteLine("Invalid vehicle type.");
                return;
            }

            Console.Write("Enter start work date (dd/MM/yyyy HH:mm:ss, or press Enter to skip): ");
            string? startDateInput = Console.ReadLine();
            DateTime? startWorkDate = null;
            
            if (!string.IsNullOrWhiteSpace(startDateInput))
            {
                if (!DateTime.TryParse(startDateInput, out DateTime startDate))
                {
                    Console.WriteLine("Invalid date format.");
                    return;
                }
                startWorkDate = startDate;
            }

            Courier newCourier = new Courier
            {
                CourierId = courierId,
                CourierFullName = fullName,
                CourierCellPhone = cellPhone,
                CourierEmail = email,
                CourierPassword = password,
                CourierIsActive = isActive,
                CourierLocation = location,
                MaxCourierDistance = maxDistance,
                VehicleType = vehicleType,
                StartWorkDate = startWorkDate
            };

            s_bl.Courier.AddCourier(requesterId, newCourier);
            Console.WriteLine("Courier added successfully.");
        }
        catch (BlAlreadyExistsException ex)
        {
            Console.WriteLine($"BlAlreadyExistsException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlInvalidIntegerException ex)
        {
            Console.WriteLine($"BlInvalidIntegerException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlInvalidStringException ex)
        {
            Console.WriteLine($"BlInvalidStringException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlInvalidVehicleTypeException ex)
        {
            Console.WriteLine($"BlInvalidVehicleTypeException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlInvalidDateException ex)
        {
            Console.WriteLine($"BlInvalidDateException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlAdminPermissionException ex)
        {
            Console.WriteLine($"BlAdminPermissionException: {ex.Message}");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the UpdateCourier operation.
    /// </summary>
    private static void TestUpdateCourier()
    {
        try
        {
            Console.Write("Enter requester ID: ");
            string? requesterIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(requesterIdInput))
            {
                Console.WriteLine("Requester ID is required.");
                return;
            }

            if (!int.TryParse(requesterIdInput, out int requesterId))
            {
                Console.WriteLine("Invalid requester ID format. Must be a number.");
                return;
            }

            Console.Write("Enter courier ID to update: ");
            string? courierIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(courierIdInput))
            {
                Console.WriteLine("Courier ID is required.");
                return;
            }

            if (!int.TryParse(courierIdInput, out int courierId))
            {
                Console.WriteLine("Invalid courier ID format. Must be a number.");
                return;
            }

            Courier currentCourier = s_bl.Courier.GetCourier(requesterId, courierId);
            Console.WriteLine($"Current courier: {currentCourier}");

            Console.Write($"Enter courier full name (or press Enter to keep [{currentCourier.CourierFullName}]): ");
            string? fullName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(fullName))
                fullName = currentCourier.CourierFullName;

            Console.Write($"Enter courier cell phone (or press Enter to keep [{currentCourier.CourierCellPhone}]): ");
            string? cellPhone = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(cellPhone))
                cellPhone = currentCourier.CourierCellPhone;

            Console.Write($"Enter courier email (or press Enter to keep [{currentCourier.CourierEmail}]): ");
            string? email = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(email))
                email = currentCourier.CourierEmail;

            Console.Write($"Enter courier password (or press Enter to keep [***]): ");
            string? password = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(password))
                password = currentCourier.CourierPassword;

            Console.Write($"Is courier active? (true/false, or press Enter to keep [{currentCourier.CourierIsActive}]): ");
            string? isActiveInput = Console.ReadLine();
            bool isActive = currentCourier.CourierIsActive;
            if (!string.IsNullOrWhiteSpace(isActiveInput))
            {
                if (!bool.TryParse(isActiveInput, out isActive))
                {
                    Console.WriteLine("Invalid boolean value. Keeping current value.");
                }
            }

            Console.Write($"Enter courier location (or press Enter to keep [{currentCourier.CourierLocation}]): ");
            string? location = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(location))
                location = currentCourier.CourierLocation;

            Console.Write($"Enter max courier distance (or press Enter to keep [{currentCourier.MaxCourierDistance?.ToString() ?? "null"}]): ");
            string? maxDistInput = Console.ReadLine();
            double? maxDistance = currentCourier.MaxCourierDistance;
            if (!string.IsNullOrWhiteSpace(maxDistInput))
            {
                if (maxDistInput.ToLower() == "null")
                {
                    maxDistance = null;
                }
                else if (double.TryParse(maxDistInput, out double maxDist))
                {
                    maxDistance = maxDist;
                }
                else
                {
                    Console.WriteLine("Invalid number format. Keeping current value.");
                }
            }

            Console.WriteLine($"Available vehicle types: {string.Join(", ", Enum.GetNames<VehicleType>())}");
            Console.Write($"Enter vehicle type (or press Enter to keep [{currentCourier.VehicleType}]): ");
            string? vehicleTypeInput = Console.ReadLine();
            VehicleType vehicleType = currentCourier.VehicleType;
            if (!string.IsNullOrWhiteSpace(vehicleTypeInput))
            {
                if (!Enum.TryParse<VehicleType>(vehicleTypeInput, ignoreCase: true, out vehicleType))
                {
                    Console.WriteLine("Invalid vehicle type. Keeping current value.");
                }
            }

            Courier updatedCourier = new Courier
            {
                CourierId = courierId,
                CourierFullName = fullName,
                CourierCellPhone = cellPhone,
                CourierEmail = email,
                CourierPassword = password,
                CourierIsActive = isActive,
                CourierLocation = location,
                MaxCourierDistance = maxDistance,
                VehicleType = vehicleType,
                StartWorkDate = currentCourier.StartWorkDate
            };

            s_bl.Courier.UpdateCourier(requesterId, updatedCourier);
            Console.WriteLine("Courier updated successfully.");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlInvalidIntegerException ex)
        {
            Console.WriteLine($"BlInvalidIntegerException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlInvalidStringException ex)
        {
            Console.WriteLine($"BlInvalidStringException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlInvalidVehicleTypeException ex)
        {
            Console.WriteLine($"BlInvalidVehicleTypeException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlInvalidDateException ex)
        {
            Console.WriteLine($"BlInvalidDateException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlAdminPermissionException ex)
        {
            Console.WriteLine($"BlAdminPermissionException: {ex.Message}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the DeleteCourier operation.
    /// </summary>
    private static void TestDeleteCourier()
    {
        try
        {
            Console.Write("Enter requester ID (admin): ");
            string? requesterIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(requesterIdInput))
            {
                Console.WriteLine("Requester ID is required.");
                return;
            }

            if (!int.TryParse(requesterIdInput, out int requesterId))
            {
                Console.WriteLine("Invalid requester ID format. Must be a number.");
                return;
            }

            Console.Write("Enter courier ID to delete: ");
            string? courierIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(courierIdInput))
            {
                Console.WriteLine("Courier ID is required.");
                return;
            }

            if (!int.TryParse(courierIdInput, out int courierId))
            {
                Console.WriteLine("Invalid courier ID format. Must be a number.");
                return;
            }

            s_bl.Courier.DeleteCourier(requesterId, courierId);
            Console.WriteLine("Courier deleted successfully.");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlCourierHasActiveDeliveryException ex)
        {
            Console.WriteLine($"BlCourierHasActiveDeliveryException: {ex.Message}");
        }
        catch (BlAdminPermissionException ex)
        {
            Console.WriteLine($"BlAdminPermissionException: {ex.Message}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    // -------------------- Order Operations -------------------- \\

    /// <summary>
    /// Tests the GetOrderStatusSummary operation.
    /// </summary>
    private static void TestGetOrderStatusSummary()
    {
        try
        {
            Console.Write("Enter requester ID (admin): ");
            string? requesterIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(requesterIdInput))
            {
                Console.WriteLine("Requester ID is required.");
                return;
            }

            if (!int.TryParse(requesterIdInput, out int requesterId))
            {
                Console.WriteLine("Invalid requester ID format. Must be a number.");
                return;
            }

            int[] summary = s_bl.Order.GetOrderStatusSummary(requesterId);
            
            Console.WriteLine("Order Status Summary:");
            Console.WriteLine($"Open_OnTime: {summary[(int)LogicalOrderStatus.Open_OnTime]}");
            Console.WriteLine($"Open_InRisk: {summary[(int)LogicalOrderStatus.Open_InRisk]}");
            Console.WriteLine($"Open_Late: {summary[(int)LogicalOrderStatus.Open_Late]}");
            Console.WriteLine($"InProgress_OnTime: {summary[(int)LogicalOrderStatus.InProgress_OnTime]}");
            Console.WriteLine($"InProgress_InRisk: {summary[(int)LogicalOrderStatus.InProgress_InRisk]}");
            Console.WriteLine($"InProgress_Late: {summary[(int)LogicalOrderStatus.InProgress_Late]}");
            Console.WriteLine($"Supplied: {summary[(int)LogicalOrderStatus.Supplied]}");
            Console.WriteLine($"Refused: {summary[(int)LogicalOrderStatus.Refused]}");
            Console.WriteLine($"Canceled: {summary[(int)LogicalOrderStatus.Canceled]}");
        }
        catch (BlAdminPermissionException ex)
        {
            Console.WriteLine($"BlAdminPermissionException: {ex.Message}");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the GetOrders operation.
    /// </summary>
    private static void TestGetOrders()
    {
        try
        {
            Console.Write("Enter requester ID (admin): ");
            string? requesterIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(requesterIdInput))
            {
                Console.WriteLine("Requester ID is required.");
                return;
            }

            if (!int.TryParse(requesterIdInput, out int requesterId))
            {
                Console.WriteLine("Invalid requester ID format. Must be a number.");
                return;
            }

            Console.Write("Filter by? (OrderId/TypeOfOrder/OrderStatus/ScheduleStatus, press Enter for null): ");
            string? filterFieldInput = Console.ReadLine();
            OrderInListFilterBy? filterField = null;
               
            if (!string.IsNullOrWhiteSpace(filterFieldInput))
            {
                if (Enum.TryParse<OrderInListFilterBy>(filterFieldInput, ignoreCase: true, out OrderInListFilterBy filterFieldValue))
                {
                    filterField = filterFieldValue;
                }
                else
                {
                    Console.WriteLine("Invalid filter field. Using null (no filter).");
                }
            }

            object? filterValueObj = null;
            if (filterField != null)
            {
                Console.Write("Enter filter value (or press Enter for null): ");
                string? filterValueInput = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(filterValueInput))
                {
                    switch (filterField)
                    {
                        case OrderInListFilterBy.OrderId:
                            if (int.TryParse(filterValueInput, out int orderId))
                                filterValueObj = orderId;
                            break;
                        case OrderInListFilterBy.TypeOfOrder:
                            if (Enum.TryParse<TypeOfOrder>(filterValueInput, ignoreCase: true, out TypeOfOrder typeOfOrder))
                                filterValueObj = typeOfOrder;
                            break;
                        case OrderInListFilterBy.OrderStatus:
                            if (Enum.TryParse<OrderStatus>(filterValueInput, ignoreCase: true, out OrderStatus orderStatus))
                                filterValueObj = orderStatus;
                            break;
                        case OrderInListFilterBy.ScheduleStatus:
                            if (Enum.TryParse<ScheduleStatus>(filterValueInput, ignoreCase: true, out ScheduleStatus scheduleStatus))
                                filterValueObj = scheduleStatus;
                            break;
                    }
                }
            }

            Console.Write("Sort by? (OrderId/TypeOfOrder/AirDistance/OrderStatus/ScheduleStatus/TimeLeftToFinish/TotalHandleTime/TotalDeliveries, press Enter for null): ");
            string? sortByInput = Console.ReadLine();
            OrderInListSortBy? sortBy = null;
            
            if (!string.IsNullOrWhiteSpace(sortByInput))
            {
                if (Enum.TryParse<OrderInListSortBy>(sortByInput, ignoreCase: true, out OrderInListSortBy sortValue))
                {
                    sortBy = sortValue;
                }
                else
                {
                    Console.WriteLine("Invalid sort option. Using null (default sort).");
                }
            }

            IEnumerable<OrderInList> orders = s_bl.Order.GetOrders(requesterId, filterField, filterValueObj, sortBy);
            
            foreach (var order in orders)
            {
                Console.WriteLine(order);
            }
        }
        catch (BlAdminPermissionException ex)
        {
            Console.WriteLine($"BlAdminPermissionException: {ex.Message}");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the GetOrder operation.
    /// </summary>
    private static void TestGetOrder()
    {
        try
        {
            Console.Write("Enter requester ID: ");
            string? requesterIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(requesterIdInput))
            {
                Console.WriteLine("Requester ID is required.");
                return;
            }

            if (!int.TryParse(requesterIdInput, out int requesterId))
            {
                Console.WriteLine("Invalid requester ID format. Must be a number.");
                return;
            }

            Console.Write("Enter order ID: ");
            string? orderIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(orderIdInput))
            {
                Console.WriteLine("Order ID is required.");
                return;
            }

            if (!int.TryParse(orderIdInput, out int orderId))
            {
                Console.WriteLine("Invalid order ID format. Must be a number.");
                return;
            }

            Order order = s_bl.Order.GetOrder(requesterId, orderId);
            Console.WriteLine(order);
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the UpdateOrder operation.
    /// </summary>
    private static void TestUpdateOrder()
    {
        try
        {
            Console.Write("Enter requester ID (admin): ");
            string? requesterIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(requesterIdInput))
            {
                Console.WriteLine("Requester ID is required.");
                return;
            }

            if (!int.TryParse(requesterIdInput, out int requesterId))
            {
                Console.WriteLine("Invalid requester ID format. Must be a number.");
                return;
            }

            Console.Write("Enter order ID to update: ");
            string? orderIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(orderIdInput))
            {
                Console.WriteLine("Order ID is required.");
                return;
            }

            if (!int.TryParse(orderIdInput, out int orderId))
            {
                Console.WriteLine("Invalid order ID format. Must be a number.");
                return;
            }

            Order currentOrder = s_bl.Order.GetOrder(requesterId, orderId);
            Console.WriteLine($"Current order: {currentOrder}");

            Console.Write($"Enter order detail (or press Enter to keep [{currentOrder.OrderDetail ?? "null"}]): ");
            string? detail = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(detail))
                detail = currentOrder.OrderDetail;

            Console.Write($"Enter order address (or press Enter to keep [{currentOrder.OrderAddress}]): ");
            string? address = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(address))
                address = currentOrder.OrderAddress;

            Console.Write($"Enter latitude (or press Enter to keep [{currentOrder.OrderLatitude}]): ");
            string? latInput = Console.ReadLine();
            double latitude = currentOrder.OrderLatitude;
            if (!string.IsNullOrWhiteSpace(latInput))
            {
                if (!double.TryParse(latInput, out latitude))
                {
                    Console.WriteLine("Invalid number format. Keeping current value.");
                    latitude = currentOrder.OrderLatitude;
                }
            }

            Console.Write($"Enter longitude (or press Enter to keep [{currentOrder.OrderLongitude}]): ");
            string? lonInput = Console.ReadLine();
            double longitude = currentOrder.OrderLongitude;
            if (!string.IsNullOrWhiteSpace(lonInput))
            {
                if (!double.TryParse(lonInput, out longitude))
                {
                    Console.WriteLine("Invalid number format. Keeping current value.");
                    longitude = currentOrder.OrderLongitude;
                }
            }

            Console.Write($"Enter customer full name (or press Enter to keep [{currentOrder.CustomerFullName}]): ");
            string? customerName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(customerName))
                customerName = currentOrder.CustomerFullName;

            Console.Write($"Enter customer phone (or press Enter to keep [{currentOrder.CustomerPhone}]): ");
            string? customerPhone = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(customerPhone))
                customerPhone = currentOrder.CustomerPhone;

            Console.Write($"Enter order weight (or press Enter to keep [{currentOrder.OrderWeight}]): ");
            string? weightInput = Console.ReadLine();
            double weight = currentOrder.OrderWeight;
            if (!string.IsNullOrWhiteSpace(weightInput))
            {
                if (!double.TryParse(weightInput, out weight))
                {
                    Console.WriteLine("Invalid number format. Keeping current value.");
                    weight = currentOrder.OrderWeight;
                }
            }

            Console.Write($"Is fragile? (true/false, or press Enter to keep [{currentOrder.IsFragile}]): ");
            string? fragileInput = Console.ReadLine();
            bool isFragile = currentOrder.IsFragile;
            if (!string.IsNullOrWhiteSpace(fragileInput))
            {
                if (!bool.TryParse(fragileInput, out isFragile))
                {
                    Console.WriteLine("Invalid boolean value. Keeping current value.");
                }
            }

            Console.Write($"Enter order size (or press Enter to keep [{currentOrder.OrderSize}]): ");
            string? sizeInput = Console.ReadLine();
            double size = currentOrder.OrderSize;
            if (!string.IsNullOrWhiteSpace(sizeInput))
            {
                if (!double.TryParse(sizeInput, out size))
                {
                    Console.WriteLine("Invalid number format. Keeping current value.");
                    size = currentOrder.OrderSize;
                }
            }

            Console.WriteLine($"Available order types: {string.Join(", ", Enum.GetNames<TypeOfOrder>())}");
            Console.Write($"Enter order type (or press Enter to keep [{currentOrder.TypeOfOrder}]): ");
            string? typeInput = Console.ReadLine();
            TypeOfOrder typeOfOrder = currentOrder.TypeOfOrder;
            if (!string.IsNullOrWhiteSpace(typeInput))
            {
                if (!Enum.TryParse<TypeOfOrder>(typeInput, ignoreCase: true, out typeOfOrder))
                {
                    Console.WriteLine("Invalid order type. Keeping current value.");
                }
            }

            Order updatedOrder = new Order
            {
                OrderId = orderId,
                OrderDetail = detail,
                OrderAddress = address,
                OrderLatitude = latitude,
                OrderLongitude = longitude,
                CustomerFullName = customerName,
                CustomerPhone = customerPhone,
                OrderWeight = weight,
                IsFragile = isFragile,
                OrderSize = size,
                TypeOfOrder = typeOfOrder,
                OrderOpenTime = currentOrder.OrderOpenTime,
                ExpectedDeliveryTime = currentOrder.ExpectedDeliveryTime,
                MaxDeliveryTime = currentOrder.MaxDeliveryTime,
                OrderStatus = currentOrder.OrderStatus,
                ScheduleStatus = currentOrder.ScheduleStatus,
                TimeRemaining = currentOrder.TimeRemaining,
                DeliveryPerOrderInList = currentOrder.DeliveryPerOrderInList
            };

            s_bl.Order.UpdateOrder(requesterId, updatedOrder);
            Console.WriteLine("Order updated successfully.");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlInvalidIntegerException ex)
        {
            Console.WriteLine($"BlInvalidIntegerException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlInvalidDoubleException ex)
        {
            Console.WriteLine($"BlInvalidDoubleException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlInvalidStringException ex)
        {
            Console.WriteLine($"BlInvalidStringException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlAdminPermissionException ex)
        {
            Console.WriteLine($"BlAdminPermissionException: {ex.Message}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the CancelOrder operation.
    /// </summary>
    private static void TestCancelOrder()
    {
        try
        {
            Console.Write("Enter requester ID: ");
            string? requesterIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(requesterIdInput))
            {
                Console.WriteLine("Requester ID is required.");
                return;
            }

            if (!int.TryParse(requesterIdInput, out int requesterId))
            {
                Console.WriteLine("Invalid requester ID format. Must be a number.");
                return;
            }

            Console.Write("Enter order ID to cancel: ");
            string? orderIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(orderIdInput))
            {
                Console.WriteLine("Order ID is required.");
                return;
            }

            if (!int.TryParse(orderIdInput, out int orderId))
            {
                Console.WriteLine("Invalid order ID format. Must be a number.");
                return;
            }

            s_bl.Order.CancelOrder(requesterId, orderId);
            Console.WriteLine("Order canceled successfully.");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlOrderAlreadyCanceledException ex)
        {
            Console.WriteLine($"BlOrderAlreadyCanceledException: {ex.Message}");
        }
        catch (BlOrderHasActiveDeliveryException ex)
        {
            Console.WriteLine($"BlOrderHasActiveDeliveryException: {ex.Message}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the DeleteOrder operation.
    /// </summary>
    private static void TestDeleteOrder()
    {
        try
        {
            Console.Write("Enter requester ID: ");
            string? requesterIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(requesterIdInput))
            {
                Console.WriteLine("Requester ID is required.");
                return;
            }

            if (!int.TryParse(requesterIdInput, out int requesterId))
            {
                Console.WriteLine("Invalid requester ID format. Must be a number.");
                return;
            }

            Console.Write("Enter order ID to delete: ");
            string? orderIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(orderIdInput))
            {
                Console.WriteLine("Order ID is required.");
                return;
            }

            if (!int.TryParse(orderIdInput, out int orderId))
            {
                Console.WriteLine("Invalid order ID format. Must be a number.");
                return;
            }

            s_bl.Order.DeleteOrder(requesterId, orderId);
            Console.WriteLine("Order delete operation called (expected to throw exception).");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the AddOrder operation.
    /// </summary>
    private static void TestAddOrder()
    {
        try
        {
            Console.Write("Enter requester ID (admin): ");
            string? requesterIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(requesterIdInput))
            {
                Console.WriteLine("Requester ID is required.");
                return;
            }

            if (!int.TryParse(requesterIdInput, out int requesterId))
            {
                Console.WriteLine("Invalid requester ID format. Must be a number.");
                return;
            }

            Console.Write("Enter order detail: ");
            string? detail = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(detail))
            {
                Console.WriteLine("Order detail is required.");
                return;
            }

            Console.Write("Enter order address: ");
            string? address = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(address))
            {
                Console.WriteLine("Order address is required.");
                return;
            }

            Console.Write("Enter latitude: ");
            string? latInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(latInput))
            {
                Console.WriteLine("Latitude is required.");
                return;
            }

            if (!double.TryParse(latInput, out double latitude))
            {
                Console.WriteLine("Invalid latitude format. Must be a number.");
                return;
            }

            Console.Write("Enter longitude: ");
            string? lonInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(lonInput))
            {
                Console.WriteLine("Longitude is required.");
                return;
            }

            if (!double.TryParse(lonInput, out double longitude))
            {
                Console.WriteLine("Invalid longitude format. Must be a number.");
                return;
            }

            Console.Write("Enter customer full name: ");
            string? customerName = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(customerName))
            {
                Console.WriteLine("Customer full name is required.");
                return;
            }

            Console.Write("Enter customer phone: ");
            string? customerPhone = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(customerPhone))
            {
                Console.WriteLine("Customer phone is required.");
                return;
            }

            Console.Write("Enter order weight: ");
            string? weightInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(weightInput))
            {
                Console.WriteLine("Order weight is required.");
                return;
            }

            if (!double.TryParse(weightInput, out double weight))
            {
                Console.WriteLine("Invalid weight format. Must be a number.");
                return;
            }

            Console.Write("Is fragile? (true/false): ");
            string? fragileInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(fragileInput))
            {
                Console.WriteLine("Fragile status is required.");
                return;
            }

            if (!bool.TryParse(fragileInput, out bool isFragile))
            {
                Console.WriteLine("Invalid boolean value.");
                return;
            }

            Console.Write("Enter order size: ");
            string? sizeInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(sizeInput))
            {
                Console.WriteLine("Order size is required.");
                return;
            }

            if (!double.TryParse(sizeInput, out double size))
            {
                Console.WriteLine("Invalid size format. Must be a number.");
                return;
            }

            Console.Write("Enter order open time (dd/MM/yyyy HH:mm:ss): ");
            string? openTimeInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(openTimeInput))
            {
                Console.WriteLine("Order open time is required.");
                return;
            }

            if (!DateTime.TryParse(openTimeInput, out DateTime openTime))
            {
                Console.WriteLine("Invalid date format.");
                return;
            }

            Console.WriteLine($"Available order types: {string.Join(", ", Enum.GetNames<TypeOfOrder>())}");
            Console.Write("Enter order type: ");
            string? typeInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(typeInput))
            {
                Console.WriteLine("Order type is required.");
                return;
            }

            if (!Enum.TryParse<TypeOfOrder>(typeInput, ignoreCase: true, out TypeOfOrder typeOfOrder))
            {
                Console.WriteLine("Invalid order type.");
                return;
            }

            Order newOrder = new Order
            {
                OrderId = 0,
                OrderDetail = detail,
                OrderAddress = address,
                OrderLatitude = latitude,
                OrderLongitude = longitude,
                CustomerFullName = customerName,
                CustomerPhone = customerPhone,
                OrderWeight = weight,
                IsFragile = isFragile,
                OrderSize = size,
                TypeOfOrder = typeOfOrder,
                OrderOpenTime = openTime
            };

            s_bl.Order.AddOrder(requesterId, newOrder);
            Console.WriteLine("Order added successfully.");
        }
        catch (BlAlreadyExistsException ex)
        {
            Console.WriteLine($"BlAlreadyExistsException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlInvalidIntegerException ex)
        {
            Console.WriteLine($"BlInvalidIntegerException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlInvalidDoubleException ex)
        {
            Console.WriteLine($"BlInvalidDoubleException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlInvalidStringException ex)
        {
            Console.WriteLine($"BlInvalidStringException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlInvalidDateException ex)
        {
            Console.WriteLine($"BlInvalidDateException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlAdminPermissionException ex)
        {
            Console.WriteLine($"BlAdminPermissionException: {ex.Message}");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the CompleteOrderHandling operation.
    /// </summary>
    private static void TestCompleteOrderHandling()
    {
        try
        {
            Console.Write("Enter requester ID (courier): ");
            string? requesterIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(requesterIdInput))
            {
                Console.WriteLine("Requester ID is required.");
                return;
            }

            if (!int.TryParse(requesterIdInput, out int requesterId))
            {
                Console.WriteLine("Invalid requester ID format. Must be a number.");
                return;
            }

            Console.Write("Enter courier ID: ");
            string? courierIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(courierIdInput))
            {
                Console.WriteLine("Courier ID is required.");
                return;
            }

            if (!int.TryParse(courierIdInput, out int courierId))
            {
                Console.WriteLine("Invalid courier ID format. Must be a number.");
                return;
            }

            Console.Write("Enter delivery ID: ");
            string? deliveryIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(deliveryIdInput))
            {
                Console.WriteLine("Delivery ID is required.");
                return;
            }

            if (!int.TryParse(deliveryIdInput, out int deliveryId))
            {
                Console.WriteLine("Invalid delivery ID format. Must be a number.");
                return;
            }

            s_bl.Order.CompleteOrderHandling(requesterId, courierId, deliveryId);
            Console.WriteLine("Order handling completed successfully.");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlCourierNotAssignedToDeliveryException ex)
        {
            Console.WriteLine($"BlCourierNotAssignedToDeliveryException: {ex.Message}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the AssignOrderToCourier operation.
    /// </summary>
    private static void TestAssignOrderToCourier()
    {
        try
        {
            Console.Write("Enter requester ID: ");
            string? requesterIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(requesterIdInput))
            {
                Console.WriteLine("Requester ID is required.");
                return;
            }

            if (!int.TryParse(requesterIdInput, out int requesterId))
            {
                Console.WriteLine("Invalid requester ID format. Must be a number.");
                return;
            }

            Console.Write("Enter courier ID: ");
            string? courierIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(courierIdInput))
            {
                Console.WriteLine("Courier ID is required.");
                return;
            }

            if (!int.TryParse(courierIdInput, out int courierId))
            {
                Console.WriteLine("Invalid courier ID format. Must be a number.");
                return;
            }

            Console.Write("Enter order ID: ");
            string? orderIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(orderIdInput))
            {
                Console.WriteLine("Order ID is required.");
                return;
            }

            if (!int.TryParse(orderIdInput, out int orderId))
            {
                Console.WriteLine("Invalid order ID format. Must be a number.");
                return;
            }

            s_bl.Order.AssignOrderToCourier(requesterId, courierId, orderId);
            Console.WriteLine("Order assigned to courier successfully.");
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlOrderNotOpenForAssignmentException ex)
        {
            Console.WriteLine($"BlOrderNotOpenForAssignmentException: {ex.Message}");
        }
        catch (BlCourierDisabledException ex)
        {
            Console.WriteLine($"BlCourierDisabledException: {ex.Message}");
        }
        catch (BlCourierHasActiveDeliveryException ex)
        {
            Console.WriteLine($"BlCourierHasActiveDeliveryException: {ex.Message}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the GetClosedDeliveriesByCourier operation.
    /// </summary>
    private static void TestGetClosedDeliveriesByCourier()
    {
        try
        {
            Console.Write("Enter requester ID: ");
            string? requesterIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(requesterIdInput))
            {
                Console.WriteLine("Requester ID is required.");
                return;
            }

            if (!int.TryParse(requesterIdInput, out int requesterId))
            {
                Console.WriteLine("Invalid requester ID format. Must be a number.");
                return;
            }

            Console.Write("Enter courier ID: ");
            string? courierIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(courierIdInput))
            {
                Console.WriteLine("Courier ID is required.");
                return;
            }

            if (!int.TryParse(courierIdInput, out int courierId))
            {
                Console.WriteLine("Invalid courier ID format. Must be a number.");
                return;
            }

            Console.Write("Filter by order type? (Smartphone/Laptop/Tablet/TV/Camera/Audio/SmartHome/GamingConsole/Accessory, press Enter for null): ");
            string? typeFilterInput = Console.ReadLine();
            TypeOfOrder? typeFilter = null;
            
            if (!string.IsNullOrWhiteSpace(typeFilterInput))
            {
                if (Enum.TryParse<TypeOfOrder>(typeFilterInput, ignoreCase: true, out TypeOfOrder typeValue))
                {
                    typeFilter = typeValue;
                }
                else
                {
                    Console.WriteLine("Invalid order type. Using null (no filter).");
                }
            }

            Console.Write("Sort by? (OrderId/TypeOfOrder/TotalHandleTime/ActualDistance/DeliveryFinishType, press Enter for null): ");
            string? sortByInput = Console.ReadLine();
            ClosedDeliverySortBy? sortBy = null;
            
            if (!string.IsNullOrWhiteSpace(sortByInput))
            {
                if (Enum.TryParse<ClosedDeliverySortBy>(sortByInput, ignoreCase: true, out ClosedDeliverySortBy sortValue))
                {
                    sortBy = sortValue;
                }
                else
                {
                    Console.WriteLine("Invalid sort option. Using null (default sort).");
                }
            }

            IEnumerable<ClosedDeliveryInList> deliveries = s_bl.Order.GetClosedDeliveriesByCourier(requesterId, courierId, typeFilter, sortBy);
            
            foreach (var delivery in deliveries)
            {
                Console.WriteLine(delivery);
            }
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Tests the GetOpenOrdersForCourier operation.
    /// </summary>
    private static void TestGetOpenOrdersForCourier()
    {
        try
        {
            Console.Write("Enter requester ID: ");
            string? requesterIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(requesterIdInput))
            {
                Console.WriteLine("Requester ID is required.");
                return;
            }

            if (!int.TryParse(requesterIdInput, out int requesterId))
            {
                Console.WriteLine("Invalid requester ID format. Must be a number.");
                return;
            }

            Console.Write("Enter courier ID: ");
            string? courierIdInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(courierIdInput))
            {
                Console.WriteLine("Courier ID is required.");
                return;
            }

            if (!int.TryParse(courierIdInput, out int courierId))
            {
                Console.WriteLine("Invalid courier ID format. Must be a number.");
                return;
            }

            Console.Write("Filter by order type? (Smartphone/Laptop/Tablet/TV/Camera/Audio/SmartHome/GamingConsole/Accessory, press Enter for null): ");
            string? typeFilterInput = Console.ReadLine();
            TypeOfOrder? typeFilter = null;
            
            if (!string.IsNullOrWhiteSpace(typeFilterInput))
            {
                if (Enum.TryParse<TypeOfOrder>(typeFilterInput, ignoreCase: true, out TypeOfOrder typeValue))
                {
                    typeFilter = typeValue;
                }
                else
                {
                    Console.WriteLine("Invalid order type. Using null (no filter).");
                }
            }

            Console.Write("Sort by? (OrderId/TypeOfOrder/OrderWeight/IsFragile/OrderSize/AirDistance/OrderStatus/ScheduleStatus/TimeLeftToFinish/MaxDeliveryTime, press Enter for null): ");
            string? sortByInput = Console.ReadLine();
            OpenOrderSortBy? sortBy = null;
            
            if (!string.IsNullOrWhiteSpace(sortByInput))
            {
                if (Enum.TryParse<OpenOrderSortBy>(sortByInput, ignoreCase: true, out OpenOrderSortBy sortValue))
                {
                    sortBy = sortValue;
                }
                else
                {
                    Console.WriteLine("Invalid sort option. Using null (default sort).");
                }
            }

            IEnumerable<OpenOrderInList> orders = s_bl.Order.GetOpenOrdersForCourier(requesterId, courierId, typeFilter, sortBy);
            
            foreach (var order in orders)
            {
                Console.WriteLine(order);
            }
        }
        catch (BlDoesNotExistException ex)
        {
            Console.WriteLine($"BlDoesNotExistException: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
        catch (BlTemporaryNotAvailableException ex)
        {
            Console.WriteLine($"BlTemporaryNotAvailableException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    // -------------------- Read Helpers -------------------- \\

    /// <summary>
    /// Read a menu selection from the console and return it only if it is in [0..11].
    /// Keeps prompting until a valid integer in range is entered.
    /// </summary>
    /// <returns>The chosen integer between 0 and 11 (inclusive).</returns>
    private static int ReadIntOfMenu()
    {
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out int number))
            {
                if (number >= 0 && number <= 11)
                    return number;
            }
            Console.WriteLine("\nTry again...");
            Console.Write("Choose: ");
        }
    }
}

