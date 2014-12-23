namespace SBrickey.Utility
{
    public static class Environment
    {
        public enum Environments
        { UNKNOWN, DEV, TEST, PROD }
        public interface iEnvironmentIdentifier
        {
            Environments Environment(string ComputerName);
        }

        /// <summary>
        /// Sample implementation of a machine name based environment identifier.
        /// </summary>
        /// <remarks>
        /// This implementation assumes the following naming convention: SERVER-[app][D/T env][##]
        ///     wereby app is a three character identifier for the system (IIS, SQL, etc)
        ///     D/T are optional to represent DEV/TEST environments
        ///     ## is a two-digit number to identify a specific server among several that host the same app
        /// Examples:
        /// -   SERVER-IIS01    : IIS PROD server 1
        /// -   SERVER-IISD01   : IIS  DEV server 1
        /// -   SERVER-IIST10   : IIS TEST server 10
        /// </remarks>
        public class MachineNameEnvironmentIdentifier : iEnvironmentIdentifier
        {
            public Environments Environment(string ComputerName)
            {
                // regex to validate the naming convention ("SERVER-[app][D?/T?][##]")
                string regex_Server = @"^(SERVER)-\w{3}[DT]?[0-9]{2}$";
                if (System.Text.RegularExpressions.Regex.IsMatch(ComputerName, regex_Server))
                {
                    // 10th character to determine D or T... otherwise assume PROD
                    return ComputerName[10] == 'D' ? Environments.DEV
                         : ComputerName[10] == 'T' ? Environments.TEST
                         : Environments.PROD;
                }

                // Chances are, this just means that it's not a server. Likely a workstation being used for development.
                return Environments.UNKNOWN;
            }
        }

        // injectable singleton instance as optional use case
        public static iEnvironmentIdentifier Instance { get; set; } // TODO: determine access modifier for SET operation

        // static ctor
        static Environment()
        {
            Environment.Instance = new MachineNameEnvironmentIdentifier();
        } // static ctor

    } // class
} // namespace