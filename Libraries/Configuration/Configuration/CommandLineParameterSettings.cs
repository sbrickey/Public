namespace SBrickey.Libraries.Configuration
{
    using SBrickey.Libraries.Configuration.ExtensionMethods;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CommandLineParameterSettings
    {
        public readonly List<string> ParameterPrefixes = new List<string> { "/", "-", "--" };

        /// <summary>
        /// Allowed list of separators between a Parameter and its Value
        /// </summary>
        public readonly List<string> ValueSeparators = new List<string> { " ", ":", "=" };

        /// <summary>
        /// List of prefixes and suffixes pairs which are allowed for encapsulating a single value
        /// </summary>
        public readonly List<string> ValueEncapsulators = new List<string> { "\"", "'" };


        /// <summary>
        /// Parses the array of command line arguments into a dictionary of key/value pairs
        /// </summary>
        /// <remarks>
        /// Supports use of any of the allowed prefixes, separators, and encapsulators
        /// </remarks>
        public Dictionary<string, string> Parse(string[] args)
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
                                          .TrimStart(ValueEncapsulators)                    // remove any Value Encapsulators' opening strings
                                          .TrimEnd(ValueEncapsulators);                     // remove any Value Encapsulators' closing strings

                    outval.Add(Param, Value);
                }
                else if (RemainingArgs.Any() &&
                         !RemainingArgs.Peek().StartsWith(ParameterPrefixes))   // if value is in next argument (next argument doesn't start with a parameter prefix)
                {
                    var Param = currentArg;
                    var Value = RemainingArgs.Dequeue()
                                             .TrimStart(ValueEncapsulators)                    // remove any Value Encapsulators' opening strings
                                             .TrimEnd(ValueEncapsulators);                     // remove any Value Encapsulators' closing strings

                    outval.Add(Param, Value);
                }
                else                                                            // if no value specified, assume "true"
                {
                    var Param = currentArg;
                    var Value = Boolean.TrueString;

                    outval.Add(Param, Value);
                }


            } // while RemainingArgs.Any()

            return outval;
        } // Parse(...)

    } // class
} // namespace