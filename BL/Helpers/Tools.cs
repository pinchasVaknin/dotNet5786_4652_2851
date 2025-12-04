namespace Helpers;
using DalApi;
using DO;

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

    // Converts degrees to radians
    private static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180.0;

    /// <summary>
    /// Calculates schedule status (OnTime / InRisk / Late)
    /// based on order date, last delivery finish time and allowed ranges.
    /// </summary>
    public static BO.ScheduleStatus CalcScheduleStatus(DateTime orderDate, DateTime clock, DateTime? lastDeliveryFinishDate,
                                                       TimeSpan maxRangeWithoutRisk, TimeSpan maxRange)
    {
        TimeSpan handleTime =
            (lastDeliveryFinishDate is null)
                ? clock - orderDate
                : lastDeliveryFinishDate.Value - orderDate;

        if (handleTime <= maxRangeWithoutRisk)
            return BO.ScheduleStatus.OnTime;

        if (handleTime <= maxRange)
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
            throw new Exception($"User {requesterId} is not authorized to perform action '{actionName}'.");
    }
}

