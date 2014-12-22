using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBrickey.Utility
{
    public static class Arguments
    {
        public static readonly string[] ParameterPrefixes = new string[] { "/", "-", "--" };
        /// <summary>
        /// Allowed list of separators between a Parameter and its Value
        /// </summary>
        public static readonly string[] ValueSeparators = new string[] { " ", ":", "=" };
        /// <summary>
        /// List of prefixes and suffixes pairs which are allowed for encapsulating a value
        /// </summary>
        public static readonly string[][] ValueEncapsulators = new string[][] { new string[] { "\"", "\"" }, new string[] { "'", "'" } };

        /// <summary>
        /// Parses the array of command line arguments into a dictionary of key/value pairs
        /// </summary>
        /// <remarks>
        /// Supports use of any of the allowed prefixes, separators, and encapsulators
        /// </remarks>
        public static Dictionary<string, string> Parse(string[] args)
        {
            var outval = new Dictionary<string, string>();


            Queue<string> RemainingArgs = new Queue<string>(args);

            while (RemainingArgs.Any())
            {
                // remove the first item
                string currentArg = RemainingArgs.Dequeue();                

                // validate that it is in fact a parameter
                if (!currentArg.StartsWith(ParameterPrefixes))
                    throw new ArgumentException("Parameter argument was not given a valid prefix");

                // trim the parameter prefix
                currentArg = currentArg.TrimStart(ParameterPrefixes);

                // Identify and store the key/value pair
                if (currentArg.ContainsAnyOf(ValueSeparators))                  // if value is in current argument (current arg contains a value separator)
                {
                    var SeparatorIndex = ValueSeparators.Select(s => currentArg.IndexOf(s)) // index of each value separator
                                                        .Where(i => i >= 0)                 // ignore values of -1 (no match)
                                                        .Min();                             // min value
                    var Param = currentArg.Substring(0, SeparatorIndex);
                    var Value = currentArg.Substring(SeparatorIndex + 1)
                                          .TrimStart(ValueEncapsulators.Select(e => e[0]))  // remove any Value Encapsulators' opening strings
                                          .TrimEnd(ValueEncapsulators.Select(e => e[1]));   // remove any Value Encapsulators' closing strings

                    outval.Add(Param, Value);
                }
                else if (RemainingArgs.Any() &&
                         !RemainingArgs.Peek().StartsWith(ParameterPrefixes))   // if value is in next argument (next argument doesn't start with a parameter prefix)
                {
                    var Param = currentArg;
                    var Value = RemainingArgs.Dequeue()
                                             .TrimStart(ValueEncapsulators.Select(e => e[0]))  // remove any Value Encapsulators' opening strings
                                             .TrimEnd(ValueEncapsulators.Select(e => e[1]));   // remove any Value Encapsulators' closing strings

                    outval.Add(Param, Value);
                }
                else                                                            // if no value specified, assume "true"
                {
                    var Param = currentArg;
                    var Value = "true";

                    outval.Add(Param, Value);
                }


            } // while RemainingArgs.Any()

            return outval;
        } // Parse(...)
    }

    public static partial class ExtensionMethods
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

    }
} // namespace