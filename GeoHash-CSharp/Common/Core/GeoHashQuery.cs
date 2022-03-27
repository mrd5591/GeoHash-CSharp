using System;
using System.Collections.Generic;
using GeoHashCSharp.Common.Util;
using System.Linq;

namespace GeoHashCSharp.Common.Core
{
    public class GeoHashQuery
    {
        public static class Utils
        {
            public static double BitsLatitude(double resolution)
            {
                return Math.Min(Math.Log(Constants.EARTH_MERIDIONAL_CIRCUMFERENCE / 2 / resolution) / Math.Log(2),
                        GeoHash.MAX_PRECISION_BITS);
            }

            public static double BitsLongitude(double resolution, double latitude)
            {
                double degrees = GeoUtils.DistanceToLongitudeDegrees(resolution, latitude);
                return (Math.Abs(degrees) > 0) ? Math.Max(1, Math.Log(360 / degrees) / Math.Log(2)) : 1;
            }

            public static int BitsForBoundingBox(GeoLocation location, double size)
            {
                double latitudeDegreesDelta = GeoUtils.DistanceToLatitudeDegrees(size);
                double latitudeNorth = Math.Min(90, location.Latitude + latitudeDegreesDelta);
                double latitudeSouth = Math.Max(-90, location.Latitude - latitudeDegreesDelta);
                int bitsLatitude = (int)Math.Floor(Utils.BitsLatitude(size)) * 2;
                int bitsLongitudeNorth = (int)Math.Floor(Utils.BitsLongitude(size, latitudeNorth)) * 2 - 1;
                int bitsLongitudeSouth = (int)Math.Floor(Utils.BitsLongitude(size, latitudeSouth)) * 2 - 1;
                return Math.Min(bitsLatitude, Math.Min(bitsLongitudeNorth, bitsLongitudeSouth));
            }
        }

        private readonly string StartValue;
        private readonly string EndValue;

        public GeoHashQuery(string startValue, string endValue)
        {
            StartValue = startValue;
            EndValue = endValue;
        }

        public static GeoHashQuery QueryForGeoHash(GeoHash geohash, int bits)
        {
            string hash = geohash.GetGeoHashString();
            int precision = (int)Math.Ceiling((double)bits / Base32Utils.BITS_PER_BASE32_CHAR);
            if (hash.Length < precision)
            {
                return new GeoHashQuery(hash, hash + "~");
            }
            hash = hash.Substring(0, precision);
            string hashBase = hash.Substring(0, hash.Length - 1);
            int lastValue = Base32Utils.Base32CharToValue(hash.ToCharArray()[hash.Length - 1]);
            int significantBits = bits - (hashBase.Length * Base32Utils.BITS_PER_BASE32_CHAR);
            int unusedBits = Base32Utils.BITS_PER_BASE32_CHAR - significantBits;
            // delete unused bits
            int startValue = (lastValue >> unusedBits) << unusedBits;
            int endValue = startValue + (1 << unusedBits);
            String startHash = hashBase + Base32Utils.ValueToBase32Char(startValue);
            String endHash;
            if (endValue > 31)
            {
                endHash = hashBase + "~";
            }
            else
            {
                endHash = hashBase + Base32Utils.ValueToBase32Char(endValue);
            }
            return new GeoHashQuery(startHash, endHash);
        }

        public static HashSet<GeoHashQuery> QueriesAtLocation(GeoLocation location, double radius)
        {
            int queryBits = Math.Max(1, Utils.BitsForBoundingBox(location, radius));
            int geoHashPrecision = (int)Math.Ceiling((float)queryBits / Base32Utils.BITS_PER_BASE32_CHAR);

            double latitude = location.Latitude;
            double longitude = location.Longitude;
            double latitudeDegrees = radius / Constants.METERS_PER_DEGREE_LATITUDE;
            double latitudeNorth = Math.Min(90, latitude + latitudeDegrees);
            double latitudeSouth = Math.Max(-90, latitude - latitudeDegrees);
            double longitudeDeltaNorth = GeoUtils.DistanceToLongitudeDegrees(radius, latitudeNorth);
            double longitudeDeltaSouth = GeoUtils.DistanceToLongitudeDegrees(radius, latitudeSouth);
            double longitudeDelta = Math.Max(longitudeDeltaNorth, longitudeDeltaSouth);

            HashSet<GeoHashQuery> queries = new HashSet<GeoHashQuery>();

            GeoHash geoHash = new GeoHash(latitude, longitude, geoHashPrecision);
            GeoHash geoHashW = new GeoHash(latitude, GeoUtils.WrapLongitude(longitude - longitudeDelta), geoHashPrecision);
            GeoHash geoHashE = new GeoHash(latitude, GeoUtils.WrapLongitude(longitude + longitudeDelta), geoHashPrecision);

            GeoHash geoHashN = new GeoHash(latitudeNorth, longitude, geoHashPrecision);
            GeoHash geoHashNW = new GeoHash(latitudeNorth, GeoUtils.WrapLongitude(longitude - longitudeDelta), geoHashPrecision);
            GeoHash geoHashNE = new GeoHash(latitudeNorth, GeoUtils.WrapLongitude(longitude + longitudeDelta), geoHashPrecision);

            GeoHash geoHashS = new GeoHash(latitudeSouth, longitude, geoHashPrecision);
            GeoHash geoHashSW = new GeoHash(latitudeSouth, GeoUtils.WrapLongitude(longitude - longitudeDelta), geoHashPrecision);
            GeoHash geoHashSE = new GeoHash(latitudeSouth, GeoUtils.WrapLongitude(longitude + longitudeDelta), geoHashPrecision);

            queries.Add(QueryForGeoHash(geoHash, queryBits));
            queries.Add(QueryForGeoHash(geoHashE, queryBits));
            queries.Add(QueryForGeoHash(geoHashW, queryBits));
            queries.Add(QueryForGeoHash(geoHashN, queryBits));
            queries.Add(QueryForGeoHash(geoHashNE, queryBits));
            queries.Add(QueryForGeoHash(geoHashNW, queryBits));
            queries.Add(QueryForGeoHash(geoHashS, queryBits));
            queries.Add(QueryForGeoHash(geoHashSE, queryBits));
            queries.Add(QueryForGeoHash(geoHashSW, queryBits));

            // Join queries
            bool didJoin;
            do
            {
                GeoHashQuery query1 = null;
                GeoHashQuery query2 = null;
                foreach(GeoHashQuery query in queries)
                {
                    foreach(GeoHashQuery other in queries)
                    {
                        if (query != other && query.CanJoinWith(other))
                        {
                            query1 = query;
                            query2 = other;
                            break;
                        }
                    }
                }
                if (query1 != null && query2 != null)
                {
                    queries.Remove(query1);
                    queries.Remove(query2);
                    queries.Add(query1.JoinWith(query2));
                    didJoin = true;
                }
                else
                {
                    didJoin = false;
                }
            } while (didJoin);

            return queries;
        }

        private bool IsPrefix(GeoHashQuery other)
        {
            return (other.EndValue.Replace("~","z").CompareTo(StartValue.Replace("~", "z")) >= 0) &&
                   (other.StartValue.Replace("~","z").CompareTo(StartValue.Replace("~", "z")) < 0) &&
                   (other.EndValue.Replace("~","z").CompareTo(EndValue.Replace("~", "z")) < 0);
        }

        private bool IsSuperQuery(GeoHashQuery other)
        {
            int startCompare = other.StartValue.Replace("~","z").CompareTo(StartValue.Replace("~","z"));
            return startCompare <= 0 && other.EndValue.Replace("~","z").CompareTo(EndValue.Replace("~", "z")) >= 0;
        }

        public bool CanJoinWith(GeoHashQuery other)
        {
            return IsPrefix(other) || other.IsPrefix(this) || IsSuperQuery(other) || other.IsSuperQuery(this);
        }

        public GeoHashQuery JoinWith(GeoHashQuery other)
        {
            if (other.IsPrefix(this))
            {
                return new GeoHashQuery(StartValue, other.EndValue);
            }
            else if (IsPrefix(other))
            {
                return new GeoHashQuery(other.StartValue, EndValue);
            }
            else if (IsSuperQuery(other))
            {
                return other;
            }
            else if (other.IsSuperQuery(this))
            {
                return this;
            }
            else
            {
                throw new ArgumentException("Can't join these 2 queries: " + this + ", " + other);
            }
        }

        public bool ContainsGeoHash(GeoHash hash)
        {
            string hashStr = hash.GetGeoHashString();
            return StartValue.Replace("~","z").CompareTo(hashStr) <= 0 && EndValue.Replace("~","z").CompareTo(hashStr) > 0;
        }

        public string GetStartValue()
        {
            return StartValue;
        }

        public string GetEndValue()
        {
            return EndValue;
        }
    }
}
