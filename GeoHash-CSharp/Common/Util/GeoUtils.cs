using System;
namespace GeoHashCSharp.Common.Util
{
    public class GeoUtils
    {
        private static readonly double MAX_SUPPORTED_RADIUS = 8587;

        public static double Distance(GeoLocation location1, GeoLocation location2)
        {
            return Distance(location1.Latitude, location1.Longitude, location2.Latitude, location2.Longitude);
        }

        public static double Distance(double lat1, double long1, double lat2, double long2)
        {
            // Earth's mean radius in meters
            double radius = (Constants.EARTH_EQ_RADIUS + Constants.EARTH_POLAR_RADIUS) / 2;
            double latDelta = ToRadians(lat1 - lat2);
            double lonDelta = ToRadians(long1 - long2);

            double a = (Math.Sin(latDelta / 2) * Math.Sin(latDelta / 2)) +
                       (Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                               Math.Sin(lonDelta / 2) * Math.Sin(lonDelta / 2));
            return radius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        public static double DistanceToLatitudeDegrees(double distance)
        {
            return distance / Constants.METERS_PER_DEGREE_LATITUDE;
        }

        public static double DistanceToLongitudeDegrees(double distance, double latitude)
        {
            double radians = ToRadians(latitude);
            double numerator = Math.Cos(radians) * Constants.EARTH_EQ_RADIUS * Math.PI / 180;
            double denominator = 1 / Math.Sqrt(1 - Constants.EARTH_E2 * Math.Sin(radians) * Math.Sin(radians));
            double deltaDegrees = numerator * denominator;
            if (deltaDegrees < Constants.EPSILON)
            {
                return distance > 0 ? 360 : distance;
            }
            else
            {
                return Math.Min(360, distance / deltaDegrees);
            }
        }

        public static double WrapLongitude(double longitude)
        {
            if (longitude >= -180 && longitude <= 180)
            {
                return longitude;
            }
            double adjusted = longitude + 180;
            if (adjusted > 0)
            {
                return (adjusted % 360.0) - 180;
            }
            else
            {
                return 180 - (-adjusted % 360);
            }
        }

        public static double CapRadius(double radius)
        {
            if (radius > MAX_SUPPORTED_RADIUS)
            {
                return MAX_SUPPORTED_RADIUS;
            }

            return radius;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
