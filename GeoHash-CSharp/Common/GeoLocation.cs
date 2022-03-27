using System;
namespace GeoHashCSharp
{
    public class GeoLocation
    {
        /** The latitude of this location in the range of [-90, 90] */
        public double Latitude { get; set; }

        /** The longitude of this location in the range of [-180, 180] */
        public double Longitude { get; set; }

        /**
         * Creates a new GeoLocation with the given latitude and longitude.
         *
         * @throws IllegalArgumentException If the coordinates are not valid geo coordinates
         * @param latitude The latitude in the range of [-90, 90]
         * @param longitude The longitude in the range of [-180, 180]
         */
        public GeoLocation(double latitude, double longitude)
        {
            if (!GeoLocation.CoordinatesValid(latitude, longitude))
            {
                throw new Exception("Not a valid geo location: " + latitude + ", " + longitude);
            }
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

        /**
         * Checks if these coordinates are valid geo coordinates.
         * @param latitude The latitude must be in the range [-90, 90]
         * @param longitude The longitude must be in the range [-180, 180]
         * @return True if these are valid geo coordinates
         */
        public static bool CoordinatesValid(double latitude, double longitude)
        {
            return latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            GeoLocation that = (GeoLocation)o;

            if (Latitude.CompareTo(that.Latitude) != 0) return false;
            if (Longitude.CompareTo(that.Longitude) != 0) return false;

            return true;
        }

        public override int GetHashCode()
        {
            int result;
            long temp;
            temp = BitConverter.DoubleToInt64Bits(Latitude);
            result = (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(Longitude);
            result = 31 * result + (int)(temp ^ (temp >> 32));
            return result;
        }

        public override string ToString()
        {
            return "GeoLocation(" + Latitude + ", " + Longitude + ")";
        }
    }
}
