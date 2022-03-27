using System;
using GeoHashCSharp.Common.Util;

namespace GeoHashCSharp.Common.Core
{
    public class GeoHash
    {
        private readonly string Geo_Hash;

        // The default precision of a geohash
        private static readonly int DEFAULT_PRECISION = 10;

        // The maximal precision of a geohash
        public static readonly int MAX_PRECISION = 22;

        // The maximal number of bits precision for a geohash
        public static readonly int MAX_PRECISION_BITS = MAX_PRECISION * Base32Utils.BITS_PER_BASE32_CHAR;

        /**
         * Convert a GeoHash string back into a GeoLocation.
         *
         * See: https://en.wikipedia.org/wiki/Geohash#Algorithm_and_example
         */
        
        public static GeoLocation LocationFromHash(string hashString)
        {
            long decoded = 0;
            int numBits = hashString.Length * Base32Utils.BITS_PER_BASE32_CHAR;

            for (int i = 0; i < hashString.Length; i++)
            {
                int charVal = Base32Utils.Base32CharToValue(hashString.ToCharArray()[i]);
                decoded = decoded << Base32Utils.BITS_PER_BASE32_CHAR;
                decoded = decoded + charVal;
            }

            double minLng = -180;
            double maxLng = 180;

            double minLat = -90;
            double maxLat = 90;

            for (int i = 0; i < numBits; i++)
            {
                // Get the high bit
                long bit = (decoded >> (numBits - i - 1)) & 1;

                // Even bits are longitude, odd bits are latitude
                if (i % 2 == 0)
                {
                    if (bit == 1)
                    {
                        minLng = (minLng + maxLng) / 2;
                    }
                    else
                    {
                        maxLng = (minLng + maxLng) / 2;
                    }
                }
                else
                {
                    if (bit == 1)
                    {
                        minLat = (minLat + maxLat) / 2;
                    }
                    else
                    {
                        maxLat = (minLat + maxLat) / 2;
                    }
                }
            }

            double lat = (minLat + maxLat) / 2;
            double lng = (minLng + maxLng) / 2;

            return new GeoLocation(lat, lng);
        }

        public GeoHash(double latitude, double longitude) : this(latitude, longitude, DEFAULT_PRECISION) { }

        public GeoHash(GeoLocation location) : this(location.Latitude, location.Longitude, DEFAULT_PRECISION) { }

        public GeoHash(double latitude, double longitude, int precision)
        {
            if (precision < 1)
            {
                throw new ArgumentException("Precision of GeoHash must be larger than zero!");
            }
            if (precision > MAX_PRECISION)
            {
                throw new ArgumentException("Precision of a GeoHash must be less than " + (MAX_PRECISION + 1) + "!");
            }
            if (!GeoLocation.CoordinatesValid(latitude, longitude))
            {
                throw new ArgumentException("Not valid location coordinates: [" + latitude + ", " + longitude + "]");
            }
            double[] longitudeRange = { -180, 180 };
            double[] latitudeRange = { -90, 90 };

            char[] buffer = new char[precision];

            for (int i = 0; i < precision; i++)
            {
                int hashValue = 0;
                for (int j = 0; j < Base32Utils.BITS_PER_BASE32_CHAR; j++)
                {
                    bool even = (((i * Base32Utils.BITS_PER_BASE32_CHAR) + j) % 2) == 0;
                    double val = even ? longitude : latitude;
                    double[] range = even ? longitudeRange : latitudeRange;
                    double mid = (range[0] + range[1]) / 2;
                    if (val > mid)
                    {
                        hashValue = (hashValue << 1) + 1;
                        range[0] = mid;
                    }
                    else
                    {
                        hashValue = hashValue << 1;
                        range[1] = mid;
                    }
                }
                buffer[i] = Base32Utils.ValueToBase32Char(hashValue);
            }
            Geo_Hash = new string(buffer);
        }

        public GeoHash(string hash)
        {
            if (hash.Length == 0 || !Base32Utils.IsValidBase32String(hash))
            {
                throw new ArgumentException("Not a valid geoHash: " + hash);
            }

            Geo_Hash = hash;
        }

        public string GetGeoHashString()
        {
            return Geo_Hash;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            GeoHash other = (GeoHash)o;

            return Geo_Hash.Equals(other.Geo_Hash);
        }

        public override string ToString()
        {
            return "GeoHash{" +
                    "geoHash='" + Geo_Hash + '\'' +
                    '}';
        }

        public override int GetHashCode()
        {
            return Geo_Hash.GetHashCode();
        }
    }
}
