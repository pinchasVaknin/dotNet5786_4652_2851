namespace Helpers;

using DalApi;
using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;

internal static class Tools
{

    private static readonly IDal s_dal = Factory.Get;

    //==============================================================\\

    /// <summary>
    /// Calculates great-circle distance (air distance) between two points
    /// given their latitude/longitude in degrees, using the Haversine formula.
    /// </summary>
    internal static double DistanceKm(double lat1Deg, double lon1Deg, double lat2Deg, double lon2Deg)
    {
        const double R = 6371.0; // Earth radius in km

        // convert degrees to radians
        double lat1 = DegreesToRadians(lat1Deg);
        double lon1 = DegreesToRadians(lon1Deg);
        double lat2 = DegreesToRadians(lat2Deg);
        double lon2 = DegreesToRadians(lon2Deg);

        double dLat = lat2 - lat1;
        double dLon = lon2 - lon1;

        double a =
            Math.Pow(Math.Sin(dLat / 2), 2) +
            Math.Cos(lat1) * Math.Cos(lat2) *
            Math.Pow(Math.Sin(dLon / 2), 2);

        double c = 2 * Math.Asin(Math.Sqrt(a));

        double distance = R * c; // in kilometers
        return distance;
    }

    /* Converts degrees to radians */
    internal static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180.0;

    //==============================================================\\

    /// <summary>
    /// Calculates schedule status (OnTime / InRisk / Late)
    /// based on order date, last delivery finish time and allowed ranges.
    /// </summary>
    internal static BO.ScheduleStatus CalcScheduleStatus(DateTime orderDate, DateTime? lastDeliveryFinishDate)
    {
        var config = AdminManager.GetConfig();
        var maxRange = config.MaxDelTimeRnge;
        var maxRangeWithoutRisk = maxRange - config.RiskTimeRnge;

        TimeSpan handleTime =
            (lastDeliveryFinishDate is null)
                ? AdminManager.Now - orderDate
                : lastDeliveryFinishDate.Value - orderDate;

        if (handleTime <= maxRangeWithoutRisk)
            return BO.ScheduleStatus.OnTime;

        if (handleTime <= config.MaxDelTimeRnge)
            return BO.ScheduleStatus.InRisk;

        return BO.ScheduleStatus.Late;
    }

    /// <summary>
    /// Ensures that the requester is the admin. 
    /// Throws an exception if not authorized.
    /// </summary>
    /// <param name="requesterId">The user ID performing the action.</param>
    /// <param name="actionName">The name of the attempted action.</param>
    internal static void EnsureAdmin(int requesterId, string actionName)
    {
        var config = AdminManager.GetConfig();

        if (requesterId != config.AdminId)
            throw new BO.BlAdminPermissionException($"User {requesterId} is not authorized to perform action '{actionName}'.");
    }

    //==============================================================\\

    /// <summary>
    /// Tries to convert an object to an enum value of type TEnum.
    /// Supports: TEnum itself, string (name), numeric values.
    /// </summary>
    internal static bool TryConvertEnum<TEnum>(object? value, out TEnum result)
        where TEnum : struct, Enum
    {

        if (value is TEnum enumVal)
        {
            result = enumVal;
            return true;
        }

        if (value is string s &&
            Enum.TryParse<TEnum>(s, ignoreCase: true, out var parsedByName))
        {
            result = parsedByName;
            return true;
        }

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

    internal static string ToStringProperty<T>(this T t)
    {
        if (t == null) return "";
        string str = "";
        foreach (PropertyInfo item in t.GetType().GetProperties())
        {
            // get property value
            var value = item.GetValue(t, null);

            // check if property is a collection
            if (value is IEnumerable list && value is not string)
            {
                // collection property
                str += "\n" + item.Name + ":";
                foreach (var listItem in list)
                {
                    str += "\n  " + listItem.ToString();
                }
            }
            else // simple property
            {
                str += "\n" + item.Name + ": " + (value ?? "null");
            }
        }
        return str;
    }

    //==============================================================\\

    internal static void ValidatePersonId(int checkInt)
    {
        if (checkInt < 200000000 || checkInt > 400000000)
            throw new BO.BlInvalidIntegerException($"ID {checkInt} is invalid. " +
                                                $"Id must be between 200,000,000 and 400,000,000.");
    }

    internal static void ValidateSystemId(int id)
    {
        if (id < 0)
            throw new BO.BlInvalidIntegerException($"ID {id} is invalid. System ID must be positive.");
    }

    internal static void ValidateNotNull<T>(T objectTemplate)
    {
        if (objectTemplate == null)
            throw new BO.BlNullPropertyException($"Entity of type {typeof(T).Name} cannot be null.");
    }

    internal static void ValidateOrder(BO.Order order)
    {

        ValidateNotNull(order);

        if (string.IsNullOrWhiteSpace(order.OrderAddress))
            throw new BO.BlInvalidStringException("Order Address is missing");

        if (string.IsNullOrWhiteSpace(order.CustomerFullName))
            throw new BO.BlInvalidStringException("Customer Name is missing");

        if (string.IsNullOrWhiteSpace(order.CustomerPhone))
            throw new BO.BlInvalidStringException("Customer Phone is missing");

        if (!IsValidIsraelPhoneNumber(order.CustomerPhone))
            throw new BO.BlInvalidStringException($"Phone number '{order.CustomerPhone}' is invalid.");

        if (order.OrderWeight <= 0)
            throw new BO.BlInvalidDoubleException("Order Weight must be greater than 0");

        if (order.OrderSize <= 0)
            throw new BO.BlInvalidDoubleException("Order Size must be greater than 0");

        DateTime currentSystemTime = s_dal.Config.Clock;

        if (order.OrderOpenTime > currentSystemTime.AddMinutes(30))
        {
            throw new BO.BlInvalidDateException("Order time cannot be in the future (relative to system clock).");
        }

        if (order.MaxDeliveryTime < order.OrderOpenTime)
            throw new BO.BlInvalidDateException("Max Delivery Time must be later than Order Time");

        if (order.ExpectedDeliveryTime < order.OrderOpenTime)
            throw new BO.BlInvalidDateException("Max Delivery Time must be later than Order Time");
    }

    internal static void ValidateCourier(BO.Courier courier)
    {

        ValidateNotNull(courier);

        ValidatePersonId(courier.CourierId);

        if (string.IsNullOrWhiteSpace(courier.CourierFullName))
            throw new BO.BlInvalidStringException("Courier Name cannot be empty.");

        if (string.IsNullOrWhiteSpace(courier.CourierPassword))
            throw new BO.BlInvalidStringException("Password cannot be empty.");

        if (string.IsNullOrWhiteSpace(courier.CourierLocation))
            throw new BO.BlInvalidStringException("Location cannot be empty.");

        if (!IsValidIsraelPhoneNumber(courier.CourierCellPhone))
            throw new BO.BlInvalidStringException($"Phone number '{courier.CourierCellPhone}' is invalid.");

        if (!IsValidEmail(courier.CourierEmail))
            throw new BO.BlInvalidStringException($"Email '{courier.CourierEmail}' is invalid.");

        if (courier.MaxCourierDistance.HasValue && courier.MaxCourierDistance <= 0)
            throw new BO.BlInvalidDoubleException("Max distance must be greater than 0.");

        if (courier.TotalOnTimeDeliveries < 0 || courier.TotalLateDeliveries < 0)
            throw new BO.BlInvalidIntegerException("Delivery counters cannot be negative.");

        if (courier.StartWorkDate.HasValue && courier.StartWorkDate > DateTime.Now)
            throw new BO.BlInvalidDateException("Start work date cannot be in the future.");
    }

    internal static void ValidateConfig(BO.Config config)
    {

        ValidateNotNull(config);

        ValidatePersonId(config.AdminId);

        if (string.IsNullOrWhiteSpace(config.AdminPassword))
            throw new BO.BlInvalidStringException("Admin Password cannot be empty.");

        if (string.IsNullOrWhiteSpace(config.CompanyAddress))
            throw new BO.BlInvalidStringException("Company Address cannot be empty.");

        if (config.Latitude.HasValue && (config.Latitude < -90 || config.Latitude > 90))
            throw new BO.BlInvalidDoubleException("Latitude must be between -90 and 90.");

        if (config.Longitude.HasValue && (config.Longitude < -180 || config.Longitude > 180))
            throw new BO.BlInvalidDoubleException("Longitude must be between -180 and 180.");

        if (config.AvgCarSpeed <= 0 ||
            config.AvgMotorcycleSpeed <= 0 ||
            config.AvgBicycleSpeed <= 0 ||
            config.AvgWalkSpeed <= 0)
        {
            throw new BO.BlInvalidDoubleException("All average speeds must be greater than 0.");
        }

        if (config.MaxAirDistance.HasValue && config.MaxAirDistance <= 0)
            throw new BO.BlInvalidDoubleException("Max Air Distance must be greater than 0.");

        if (config.MaxDelTimeRnge <= TimeSpan.Zero)
            throw new BO.BlInvalidDateException("Max delivery time range must be positive.");

        if (config.RiskTimeRnge < TimeSpan.Zero)
            throw new BO.BlInvalidDateException("Risk time range cannot be negative.");

        if (config.UnactiveTimeRnge < TimeSpan.Zero)
            throw new BO.BlInvalidDateException("Unactive time range cannot be negative.");

        if (config.RiskTimeRnge >= config.MaxDelTimeRnge)
        {
            throw new BO.BlInvalidDateException("Risk time range must be shorter than Max delivery time range.");
        }
    }



    public static bool IsValidIsraelPhoneNumber(string phone)
    {
        // 1. Check if the string is empty
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        // 2. Handle hyphen format (05X-XXXXXXX)
        // If length is 11 and there is a hyphen at index 3, remove it.
        if (phone.Length == 11 && phone[3] == '-')
        {
            phone = phone.Remove(3, 1); // Remove the '-' to check only digits later
        }

        // 3. Check length (Must be exactly 10 digits after removing hyphen)
        if (phone.Length != 10)
            return false;

        // 4. Check prefix (Must start with "05")
        if (!phone.StartsWith("05"))
            return false;

        // 5. Check content (All characters must be digits)
        foreach (char c in phone)
        {
            if (!char.IsDigit(c))
                return false;
        }

        return true;
    }

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        // בדיקה בסיסית: חייב להיות '@' וחייבת להיות נקודה '.' אחריו
        int atIndex = email.IndexOf('@');
        int dotIndex = email.LastIndexOf('.');

        // התנאים:
        // 1. ה-@ קיים ולא בהתחלה (אינדקס > 0)
        // 2. הנקודה קיימת ונמצאת אחרי ה-@ (עם לפחות תו אחד ביניהם)
        // 3. הנקודה לא בסוף המחרוזת
        return (atIndex > 0) && (dotIndex > atIndex + 1) && (dotIndex < email.Length - 1);
    }



    #region Geocoding using OpenStreetMap (Nominatim)

    /// <summary>
    /// Synchronously retrieves the geographic coordinates (Latitude, Longitude) for a given address string.
    /// Uses the free Nominatim API from OpenStreetMap.
    /// </summary>
    /// <param name="address">The address to lookup.</param>
    /// <returns>A tuple containing (Lat, Lon) if found, otherwise null.</returns>
    internal static (double? Lat, double? Lon)? GetLocationFromAddress(string address)
    {
        // 1. Validate input
        if (string.IsNullOrWhiteSpace(address))
            return null;

        try
        {
            // 2. Build the request URL for Nominatim API (XML format)
            string url = $"https://nominatim.openstreetmap.org/search?q={address}&format=xml&limit=1";

            // 3. Create a temporary HttpClient (using block ensures disposal)
            using (var client = new HttpClient())
            {
                // Important: Nominatim requires a valid User-Agent header, otherwise it blocks the request.
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                // 4. Send the request synchronously (GetAwaiter().GetResult() forces sync execution for Stage 4)
                var response = client.GetStringAsync(url).GetAwaiter().GetResult();

                // 5. Parse the XML response
                XElement xml = XElement.Parse(response);

                // 6. Check if any 'place' element exists in the response
                if (xml.HasElements)
                {
                    // Extract the first result
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
        catch (Exception ex)
        {
            // In case of network error or invalid format, return null instead of crashing
            // (Optional: Log the error using Console.WriteLine($"Geocoding error: {ex.Message}");)
            return null;
        }

        // Return null if address was not found
        return null;
    }

    #endregion Geocoding using OpenStreetMap (Nominatim)

    //======== Distance Calculation using OSRM API ======\\

    #region Distance Calculation using OSRM API

    // HttpClient instance for making HTTP requests
    internal static readonly HttpClient s_httpClient = new HttpClient();

    internal static async Task<double?> GetActualDistanceAsync(
        double? fromLat, double? fromLon,
        double? toLat, double? toLon,
        DO.CourierVehicleType vehicleType)
    {
        // Validate coordinates
        if (!fromLat.HasValue || !fromLon.HasValue || !toLat.HasValue || !toLon.HasValue)
            return null;

        // Use OSRM API to get distance
        try
        {
            string profile = vehicleType switch
            {
                DO.CourierVehicleType.Car => "car",
                DO.CourierVehicleType.Motorcycle => "car",
                DO.CourierVehicleType.Bicycle => "bike",
                _ => "foot"
            };

            string url = $"https://router.project-osrm.org/table/v1/{profile}/{fromLon},{fromLat};{toLon},{toLat}?annotations=distance";

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(jsonString);
                    JsonElement root = doc.RootElement;

                    double distance = root.GetProperty("distances")[0][1].GetDouble();
                    return distance / 1000; // km
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

    #endregion Distance Calculation using OSRM API

}

