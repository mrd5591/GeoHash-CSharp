using System;
using System.Text.RegularExpressions;

namespace GeoHashCSharp.Common.Util
{
    public static class Base32Utils
    {
        /* number of bits per base 32 character */
        public static readonly int BITS_PER_BASE32_CHAR = 5;

        private static readonly string BASE32_CHARS = "0123456789bcdefghjkmnpqrstuvwxyz";

        public static char ValueToBase32Char(int value)
        {
            if (value < 0 || value >= BASE32_CHARS.Length)
            {
                throw new ArgumentException("Not a valid base32 value: " + value);
            }
            return BASE32_CHARS.ToCharArray()[value];
        }

        public static int Base32CharToValue(char base32Char)
        {
            int value = BASE32_CHARS.IndexOf(base32Char);
            if (value == -1)
            {
                throw new ArgumentException("Not a valid base32 char: " + base32Char);
            }
            else
            {
                return value;
            }
        }

        public static bool IsValidBase32String(string str)
        {
            return Regex.Match(str, "^[" + BASE32_CHARS + "]*$").Success;
        }
    }
}
