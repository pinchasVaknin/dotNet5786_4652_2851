namespace Helpers;

using System.Text.Json;

internal static class Tools
{

    /// <summary>
    /// Calculates great-circle distance (air distance) between two points
    /// given their latitude/longitude in degrees, using the Haversine formula.
    /// </summary>
    public static double DistanceKm(double lat1Deg, double lon1Deg, double lat2Deg, double lon2Deg)
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
    private static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180.0;

    /// <summary>
    /// Calculates schedule status (OnTime / InRisk / Late)
    /// based on order date, last delivery finish time and allowed ranges.
    /// </summary>
    public static BO.ScheduleStatus CalcScheduleStatus(DateTime orderDate, DateTime? lastDeliveryFinishDate)
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


    // HttpClient instance for making HTTP requests
    internal static readonly HttpClient s_httpClient = new HttpClient();
    
    internal static async Task<double?> GetActualDistanceAsync(
        double ?fromLat, double ?fromLon,
        double ?toLat, double ?toLon,
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



}

