using System;
using System.Collections.Generic;
using GeoHashCSharp.Common.Core;
using GeoHashCSharp.Common.Util;

namespace GeoHashCSharp.Common
{
    public class GeoHashUtils
    {
        /**
        * Converts a lat/lng location into a GeoHash with default precision (10).
        */
        public static string GetGeoHashForLocation(GeoLocation location)
        {
            if(location == null)
            {
                throw new ArgumentException("location is null");
            }

            return GetGeoHashForLocation(location, 10);
        }

        /**
         * Converts a lat/lng location into a GeoHash with specified precision.
         *
         * @param location  the location to convert.
         * @param precision the precision between 1 and 22 (10 is default).
         * @return the GeoHash string.
         */
        public static String GetGeoHashForLocation(GeoLocation location, int precision)
        {
            return new GeoHash(location.Latitude, location.Longitude, precision).GetGeoHashString();
        }

        /**
         * Calculates the distance between two locations in meters.
         *
         * @param a the first location.
         * @param b the second location.
         * @return the distance between the two locations, in meters.
         */
        public static double GetDistanceBetween(GeoLocation a, GeoLocation b)
        {
            return GeoUtils.Distance(a, b);
        }

        /**
         * Determines the starting and ending geohashes to use as bounds for a database query.
         *
         * @param location the center of the query.
         * @param radius   the radius of the query, in meters. The maximum radius that is
         *                 supported is about 8587km.
         * @return a list of query bounds containing between 1 and 9 queries.
         */
        
        public static List<GeoQueryBounds> GetGeoHashQueryBounds(GeoLocation location, double radius)
        {
            List<GeoQueryBounds> result = new List<GeoQueryBounds>();
            HashSet<GeoHashQuery> queries = GeoHashQuery.QueriesAtLocation(location, radius);
            foreach(GeoHashQuery q in queries)
            {
                result.Add(new GeoQueryBounds { StartHash = q.GetStartValue(), EndHash = q.GetEndValue() });
            }
            return result;
        }
    }
}
