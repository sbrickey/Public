namespace SBrickey.Libraries.Configuration
{
    using System.Collections.ObjectModel;

    public class CommandLineParameters : ReadOnlyDictionary<string, string>
    {
        // ctor(s)
        public CommandLineParameters(string args, CommandLineParameterSettings settings = null)
            : this(args.Split(' ')) { }
        public CommandLineParameters(string[] args, CommandLineParameterSettings settings = null)
            : base((settings ?? new CommandLineParameterSettings()).Parse(args)) { }
    } // class
} // namespace
