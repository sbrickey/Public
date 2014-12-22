using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBrickey.Abstraction
{
    public static class Arguments
    {
        public interface iArguments
        {
            Dictionary<string, string> GetArguments { get; }
        }
        public class CmdLineArgs : iArguments
        {
            public Dictionary<string, string> GetArguments
            {
                // skip first command line argument ("first element is exe file name")
                // as per http://msdn.microsoft.com/en-us/library/system.environment.getcommandlineargs.aspx
                get { return Parse(Environment.GetCommandLineArgs().Skip(1).ToArray()); }
            }
        }
        public class MyArgs : iArguments
        {
            private readonly string[] _args;
            public MyArgs(string[] args) { this._args = args; }

            public Dictionary<string, string> GetArguments { get { return Parse(_args); } }
        }
        public static iArguments Current { get; set; }

        // static CTor
        static Arguments()
        {
            Arguments.Current = new CmdLineArgs();
        }

    }
} // namespace