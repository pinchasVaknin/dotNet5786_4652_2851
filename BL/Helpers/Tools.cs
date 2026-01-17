namespace Helpers;

using DalApi;
using System;
using System.Collections;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

//==================== General Tools & Helpers ===================\\

/// <summary>
/// A collection of static utility methods for the Business Logic layer.
/// Includes validation, geometric calculations, reflection helpers, and external API integrations.
/// </summary>
internal static class Tools
{
    // Access to DAL for configuration checks (lazy loaded via Factory)
    private static readonly IDal s_dal = Factory.Get;

    // Shared HttpClient for external requests to avoid socket exhaustion
    internal static readonly HttpClient s_httpClient = new HttpClient();

    //==================== Math & Schedule Logic ===================\\

    #region MathAndLogic

    /// <summary>
    /// Calculates the great-circle distance (air distance) between two points on Earth
    /// given their latitude and longitude in degrees, using the Haversine formula.
    /// </summary>
    /// <param name="lat1Deg">Latitude of the first point in degrees.</param>
    /// <param name="lon1Deg">Longitude of the first point in degrees.</param>
    /// <param name="lat2Deg">Latitude of the second point in degrees.</param>
    /// <param name="lon2Deg">Longitude of the second point in degrees.</param>
    /// <returns>The distance between the points in Kilometers.</returns>
    internal static double DistanceKm(double lat1Deg, double lon1Deg, double lat2Deg, double lon2Deg)
    {
        const double R = 6371.0; // Earth radius in km

        // Convert degrees to radians for trigonometric calculations
        double lat1 = DegreesToRadians(lat1Deg);
        double lon1 = DegreesToRadians(lon1Deg);
        double lat2 = DegreesToRadians(lat2Deg);
        double lon2 = DegreesToRadians(lon2Deg);

        // Differences in coordinates
        double dLat = lat2 - lat1;
        double dLon = lon2 - lon1;

        // Haversine formula
        double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Pow(Math.Sin(dLon / 2), 2);

        double c = 2 * Math.Asin(Math.Sqrt(a));

        return R * c; // Result in kilometers
    }

    /// <summary>
    /// Helper method to convert degrees to radians.
    /// </summary>
    /// <param name="degrees">The value in degrees.</param>
    /// <returns>The value in radians.</returns>
    internal static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180.0;

    /// <summary>
    /// Calculates the schedule status (OnTime / InRisk / Late) of an order
    /// based on its creation date, the last delivery finish time, and the system's configured time ranges.
    /// </summary>
    /// <param name="orderDate">The date and time the order was created.</param>
    /// <param name="lastDeliveryFinishDate">The date and time the last delivery attempt finished (nullable).</param>
    /// <returns>A <see cref="BO.ScheduleStatus"/> enum value.</returns>
    internal static BO.ScheduleStatus CalcScheduleStatus(DateTime orderDate, DateTime? lastDeliveryFinishDate)
    {
        // Normalize MinValue to null
        if (lastDeliveryFinishDate.HasValue && lastDeliveryFinishDate.Value == DateTime.MinValue)
            lastDeliveryFinishDate = null;

        // Fetch time configuration
        var config = AdminManager.GetConfig();
        var maxRange = config.MaxDelTimeRnge;

        // "Risk" starts before the max time is reached
        var maxRangeWithoutRisk = maxRange - config.RiskTimeRnge;

        // Calculate how much time has passed since the order started
        // If delivery isn't finished (null or DateTime.MinValue), calculate against current system clock
        TimeSpan handleTime = (lastDeliveryFinishDate is null || lastDeliveryFinishDate.Value == DateTime.MinValue)
                ? AdminManager.Now - orderDate
                : lastDeliveryFinishDate.Value - orderDate;

        // Determine status
        if (handleTime <= maxRangeWithoutRisk)
            return BO.ScheduleStatus.OnTime;

        if (handleTime <= config.MaxDelTimeRnge)
            return BO.ScheduleStatus.InRisk;

        return BO.ScheduleStatus.Late;
    }

    #endregion MathAndLogic

    //==================== General Utilities ===================\\

    #region GeneralUtilities

    /// <summary>
    /// Ensures that the user requesting an action is the Admin.
    /// </summary>
    /// <param name="requesterId">The user ID performing the action.</param>
    /// <param name="actionName">The name of the attempted action (for error messaging).</param>
    /// <exception cref="BO.BlAdminPermissionException">Thrown if the requester is not the admin.</exception>
    internal static void EnsureAdmin(int requesterId, string actionName)
    {
        var config = AdminManager.GetConfig();

        if (requesterId != config.AdminId)
            throw new BO.BlAdminPermissionException($"User {requesterId} is not authorized to perform action '{actionName}'.");
    }

    /// <summary>
    /// Tries to convert an object (string, int, or enum) to a specific Enum type safely.
    /// </summary>
    /// <typeparam name="TEnum">The target Enum type.</typeparam>
    /// <param name="value">The object value to convert.</param>
    /// <param name="result">The converted enum value if successful.</param>
    /// <returns>True if conversion succeeded, otherwise False.</returns>
    internal static bool TryConvertEnum<TEnum>(object? value, out TEnum result)
        where TEnum : struct, Enum
    {
        // Value is already the enum type
        if (value is TEnum enumVal)
        {
            result = enumVal;
            return true;
        }

        // Value is a string (case-insensitive parsing)
        if (value is string s &&
            Enum.TryParse<TEnum>(s, ignoreCase: true, out var parsedByName))
        {
            result = parsedByName;
            return true;
        }

        // Value is numeric (int) and is defined in the enum
        if (value is IConvertible &&
            int.TryParse(Convert.ToString(value), out int intVal) &&
            Enum.IsDefined(typeof(TEnum), intVal))
        {
            result = (TEnum)Enum.ToObject(typeof(TEnum), intVal);
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Extension method that generates a formatted string representation of an object's properties.
    /// Useful for debugging and logging. Handles nested collections as well.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="t">The object instance.</param>
    /// <returns>A string containing property names and values.</returns>
    internal static string ToStringProperty<T>(this T t)
    {
        if (t == null) return "";
        string str = "";

        // Iterate over all public properties
        foreach (PropertyInfo item in t.GetType().GetProperties())
        {
            // Get property value
            var value = item.GetValue(t, null);

            // Special handling for collections (lists, arrays) to print their items
            if (value is IEnumerable list && value is not string)
            {
                str += "\n" + item.Name + ":";
                foreach (var listItem in list)
                {
                    str += "\n  " + listItem.ToString();
                }
            }
            else // Simple property
            {
                str += "\n" + item.Name + ": " + (value ?? "null");
            }
        }
        return str;
    }

    #endregion GeneralUtilities

    //==================== Validation Logic ===================\\

    #region ValidationLogic

    /// <summary>
    /// Validates that a person's ID (Courier or Admin) is within the valid range (200M - 400M).
    /// </summary>
    /// <param name="checkInt">The ID to check.</param>
    /// <exception cref="BO.BlInvalidIntegerException">Thrown if ID is out of range.</exception>
    internal static void ValidatePersonId(int checkInt)
    {
        if (checkInt < 200000000 || checkInt > 400000000)
            throw new BO.BlInvalidIntegerException($"ID {checkInt} is invalid. Id must be between 200,000,000 and 400,000,000.");
    }

    /// <summary>
    /// Validates that a system ID (like OrderId or DeliveryId) is non-negative.
    /// </summary>
    /// <param name="id">The ID to check.</param>
    /// <exception cref="BO.BlInvalidIntegerException">Thrown if ID is negative.</exception>
    internal static void ValidateSystemId(int id)
    {
        if (id < 0)
            throw new BO.BlInvalidIntegerException($"ID {id} is invalid. System ID must be positive.");
    }

    /// <summary>
    /// Validates that a reference object is not null.
    /// </summary>
    /// <typeparam name="T">Type of the object.</typeparam>
    /// <param name="objectTemplate">The object instance.</param>
    /// <exception cref="BO.BlNullPropertyException">Thrown if the object is null.</exception>
    internal static void ValidateNotNull<T>(T objectTemplate)
    {
        if (objectTemplate == null)
            throw new BO.BlNullPropertyException($"Entity of type {typeof(T).Name} cannot be null.");
    }

    /// <summary>
    /// Validates the logical integrity of a <see cref="BO.Order"/> object.
    /// Checks for required fields, valid values, and logical date constraints.
    /// </summary>
    /// <param name="order">The order object to validate.</param>
    /// <exception cref="BO.BlInvalidStringException">Thrown if strings are empty or invalid format.</exception>
    /// <exception cref="BO.BlInvalidDoubleException">Thrown if numeric values are non-positive.</exception>
    /// <exception cref="BO.BlInvalidDateException">Thrown if dates are in the future or logically incorrect.</exception>
    internal static void ValidateOrder(BO.Order order)
    {
        ValidateNotNull(order);

        // String validations
        if (string.IsNullOrWhiteSpace(order.OrderAddress))
            throw new BO.BlInvalidStringException("Order Address is missing");

        if (string.IsNullOrWhiteSpace(order.CustomerFullName))
            throw new BO.BlInvalidStringException("Customer Name is missing");

        if (string.IsNullOrWhiteSpace(order.CustomerPhone))
            throw new BO.BlInvalidStringException("Customer Phone is missing");

        if (!IsValidIsraelPhoneNumber(order.CustomerPhone))
            throw new BO.BlInvalidStringException($"Phone number '{order.CustomerPhone}' is invalid.");

        // Numeric validations
        if (order.OrderWeight <= 0)
            throw new BO.BlInvalidDoubleException("Order Weight must be greater than 0");

        if (order.OrderSize <= 0)
            throw new BO.BlInvalidDoubleException("Order Size must be greater than 0");

        // Date validations
        DateTime currentSystemTime = s_dal.Config.Clock;

        // Allow a small buffer (30 mins) for clock skew, but reject future orders
        if (order.OrderOpenTime > currentSystemTime.AddMinutes(30))
        {
            throw new BO.BlInvalidDateException("Order time cannot be in the future (relative to system clock).");
        }

        // Logic check: Delivery cannot happen before the order is opened
        if (order.MaxDeliveryTime < order.OrderOpenTime)
            throw new BO.BlInvalidDateException("Max Delivery Time must be later than Order Time");

        if (order.ExpectedDeliveryTime < order.OrderOpenTime)
            throw new BO.BlInvalidDateException("Expected Delivery Time must be later than Order Time");
    }

    /// <summary>
    /// Validates the logical integrity of a <see cref="BO.Courier"/> object.
    /// Checks required fields, credentials strength, and valid stats.
    /// </summary>
    /// <param name="courier">The courier object.</param>
    internal static void ValidateCourier(BO.Courier courier)
    {
        ValidateNotNull(courier);
        ValidatePersonId(courier.CourierId);

        // Required Text Fields
        if (string.IsNullOrWhiteSpace(courier.CourierFullName))
            throw new BO.BlInvalidStringException("Courier Name cannot be empty.");

        if (string.IsNullOrWhiteSpace(courier.CourierPassword))
            throw new BO.BlInvalidStringException("Password cannot be empty.");

        if (string.IsNullOrWhiteSpace(courier.CourierLocation))
            throw new BO.BlInvalidStringException("Location cannot be empty.");

        // Format Validations
        if (!IsValidIsraelPhoneNumber(courier.CourierCellPhone))
            throw new BO.BlInvalidStringException($"Phone number '{courier.CourierCellPhone}' is invalid.");

        if (!IsValidEmail(courier.CourierEmail))
            throw new BO.BlInvalidStringException($"Email '{courier.CourierEmail}' is invalid.");

        // Logical Numeric Checks
        if (courier.MaxCourierDistance.HasValue && courier.MaxCourierDistance <= 0)
            throw new BO.BlInvalidDoubleException("Max distance must be greater than 0.");

        if (courier.TotalOnTimeDeliveries < 0 || courier.TotalLateDeliveries < 0)
            throw new BO.BlInvalidIntegerException("Delivery counters cannot be negative.");

        // Date Check
        if (courier.StartWorkDate.HasValue && courier.StartWorkDate > DateTime.Now)
            throw new BO.BlInvalidDateException("Start work date cannot be in the future.");
    }

    /// <summary>
    /// Validates the system configuration settings.
    /// </summary>
    /// <param name="config">The config object.</param>
    internal static void ValidateConfig(BO.Config config)
    {

        // Basic null check
        ValidateNotNull(config);

        // 0 AdminId indicates uninitialized config - skip validation
        if (config.AdminId == 0) return;

        // Required fields
        ValidatePersonId(config.AdminId);

        if (string.IsNullOrWhiteSpace(config.AdminPassword))
            throw new BO.BlInvalidStringException("Admin Password cannot be empty.");

        if (string.IsNullOrWhiteSpace(config.CompanyAddress))
            throw new BO.BlInvalidStringException("Company Address cannot be empty.");

        // Coordinate limits
        if (config.Latitude.HasValue && (config.Latitude < -90 || config.Latitude > 90))
            throw new BO.BlInvalidDoubleException("Latitude must be between -90 and 90.");

        if (config.Longitude.HasValue && (config.Longitude < -180 || config.Longitude > 180))
            throw new BO.BlInvalidDoubleException("Longitude must be between -180 and 180.");

        // Speeds must be positive
        if (config.AvgCarSpeed <= 0 ||
            config.AvgMotorcycleSpeed <= 0 ||
            config.AvgBicycleSpeed <= 0 ||
            config.AvgWalkSpeed <= 0)
        {
            throw new BO.BlInvalidDoubleException("All average speeds must be greater than 0.");
        }

        if (config.MaxAirDistance.HasValue && config.MaxAirDistance <= 0)
            throw new BO.BlInvalidDoubleException("Max Air Distance must be greater than 0.");

        // Time ranges must be positive
        if (config.MaxDelTimeRnge <= TimeSpan.Zero)
            throw new BO.BlInvalidDateException("Max delivery time range must be positive.");

        if (config.RiskTimeRnge < TimeSpan.Zero)
            throw new BO.BlInvalidDateException("Risk time range cannot be negative.");

        if (config.UnactiveTimeRnge < TimeSpan.Zero)
            throw new BO.BlInvalidDateException("Unactive time range cannot be negative.");

        // Logic check: Risk time cannot be greater than the total allowed time
        if (config.RiskTimeRnge >= config.MaxDelTimeRnge)
        {
            throw new BO.BlInvalidDateException("Risk time range must be shorter than Max delivery time range.");
        }
    }

    /// <summary>
    /// Validates standard Israeli phone number format.
    /// Supports: 05X-XXXXXXX or 05XXXXXXXX.
    /// </summary>
    /// <param name="phone">The phone string.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValidIsraelPhoneNumber(string phone)
    {
        // 1. Check if the string is empty
        if (string.IsNullOrWhiteSpace(phone)) return false;

        // 2. Handle hyphen format (05X-XXXXXXX) -> remove hyphen
        if (phone.Length == 11 && phone[3] == '-')
        {
            phone = phone.Remove(3, 1);
        }

        // 3. Check length (Must be exactly 10 digits)
        if (phone.Length != 10) return false;

        // 4. Check prefix (Must start with "05")
        if (!phone.StartsWith("05")) return false;

        // 5. Check content (All characters must be digits)
        foreach (char c in phone)
        {
            if (!char.IsDigit(c)) return false;
        }

        return true;
    }

    /// <summary>
    /// Basic email validation. Checks for presence and correct order of '@' and '.'.
    /// </summary>
    /// <param name="email">The email string.</param>
    /// <returns>True if basic structure is valid.</returns>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        int atIndex = email.IndexOf('@');
        int dotIndex = email.LastIndexOf('.');

        // Conditions:
        // 1. '@' exists and not at start.
        // 2. '.' exists and is after '@' (with space in between).
        // 3. '.' is not at the very end.
        return (atIndex > 0) && (dotIndex > atIndex + 1) && (dotIndex < email.Length - 1);
    }

    #endregion ValidationLogic

    //==================== External APIs ===================\\

    #region ExternalAPIs

    /// <summary>
    /// Synchronously retrieves geographic coordinates (Latitude, Longitude) for an address string
    /// using the Nominatim OpenStreetMap API.
    /// </summary>
    /// <param name="address">The address string to search.</param>
    /// <returns>A tuple (Lat, Lon) or null if failed/not found.</returns>
    internal static (double? Lat, double? Lon)? GetLocationFromAddress(string address)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(address)) return null;

        try
        {
            // Build the request URL
            string url = $"https://nominatim.openstreetmap.org/search?q={address}&format=xml&limit=1";

            // 3. Send request (Synchronous via GetAwaiter().GetResult() for consistency in sync flows)
            using (var client = new HttpClient())
            {
                // Nominatim requires a valid User-Agent header
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                var response = client.GetStringAsync(url).GetAwaiter().GetResult();

                // 4. Parse XML response
                XElement xml = XElement.Parse(response);

                if (xml.HasElements)
                {
                    var place = xml.Element("place");
                    if (place != null)
                    {
                        double lat = double.Parse(place.Attribute("lat").Value);
                        double lon = double.Parse(place.Attribute("lon").Value);
                        return (lat, lon);
                    }
                }
            }
        }
        catch (Exception)
        {
            // In case of network error or API failure, return null (fail gracefully)
            return null;
        }

        return null;
    }

    /// <summary>
    /// Asynchronously calculates the actual driving/walking distance between two points
    /// using the OSRM (Open Source Routing Machine) API.
    /// </summary>
    /// <param name="fromLat">Starting Latitude</param>
    /// <param name="fromLon">Starting Longitude</param>
    /// <param name="toLat">Ending Latitude</param>
    /// <param name="toLon">Ending Longitude</param>
    /// <param name="vehicleType">The type of vehicle to determine routing profile (car, bike, foot).</param>
    /// <returns>Distance in Kilometers, or 0 if calculation failed.</returns>
    internal static async Task<double?> GetActualDistanceAsync(
        double? fromLat, double? fromLon,
        double? toLat, double? toLon,
        DO.CourierVehicleType vehicleType)
    {
        // Validate coordinates exist
        if (!fromLat.HasValue || !fromLon.HasValue || !toLat.HasValue || !toLon.HasValue)
            return null;

        try
        {
            // Determine OSRM profile based on vehicle type
            string profile = vehicleType switch
            {
                DO.CourierVehicleType.Car => "car",
                DO.CourierVehicleType.Motorcycle => "car", // OSRM 'car' is closest approx for motorcycle
                DO.CourierVehicleType.Bicycle => "bike",
                _ => "foot"
            };

            // Build API URL
            string url = $"https://router.project-osrm.org/table/v1/{profile}/{fromLon},{fromLat};{toLon},{toLat}?annotations=distance";

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(jsonString);
                    JsonElement root = doc.RootElement;

                    // OSRM returns distance in meters, convert to KM
                    double distanceMeters = root.GetProperty("distances")[0][1].GetDouble();
                    return distanceMeters / 1000.0;
                }
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating distance: {ex.Message}");
            return 0;
        }
    }

    #endregion ExternalAPIs

}