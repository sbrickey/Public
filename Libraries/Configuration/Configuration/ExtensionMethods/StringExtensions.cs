namespace SBrickey.Libraries.Configuration.ExtensionMethods
{
    using System.Collections.Generic;
    using System.Linq;

    public static class StringExtensions
    {

        public static bool StartsWith(this string inpStr, IEnumerable<string> matches) { return matches.Any(x => inpStr.StartsWith(x)); }
        public static bool EndsWith(this string inpStr, IEnumerable<string> matches) { return matches.Any(x => inpStr.EndsWith(x)); }
        public static bool ContainsAnyOf(this string inpStr, IEnumerable<string> matches) { return matches.Any(x => inpStr.Contains(x)); }

        public static string TrimStart(this string inpStr, string match) { return inpStr.StartsWith(match) ? inpStr.Remove(0, match.Length) : inpStr; }
        public static string TrimEnd(this string inpStr, string match) { return inpStr.EndsWith(match) ? inpStr.Remove(inpStr.Length - match.Length) : inpStr; }

        public static string TrimStart(this string inpStr, IEnumerable<string> matches)
        {
            string outval = inpStr;
            var DistinctSubstrings = matches.Distinct();

            while (outval.StartsWith(matches))
            {
                var Match = DistinctSubstrings.Where(x => outval.StartsWith(x))
                                              .OrderByDescending(x => x.Length)   // largest matches first
                                              .First();
                outval = outval.TrimStart(Match);
            }

            return outval;
        }
        public static string TrimEnd(this string inpStr, IEnumerable<string> matches)
        {
            string outval = inpStr;
            var DistinctSubstrings = matches.Distinct();

            while (outval.EndsWith(matches))
            {
                var Match = DistinctSubstrings.Where(x => outval.EndsWith(x))
                                              .OrderByDescending(x => x.Length)   // largest matches first
                                              .First();
                outval = outval.TrimEnd(Match);
            }

            return outval;
        }
    } // class StringExtensions
}
