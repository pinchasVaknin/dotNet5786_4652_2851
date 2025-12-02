using DalApi;

namespace Helpers;

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

    
}

