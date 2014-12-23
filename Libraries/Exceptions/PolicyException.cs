using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBrickey.Exceptions
{

    /// <summary>
    /// The exception that is thrown when an operation cannot be performed due to a registry-based policy setting.
    /// </summary>
    public class RegistryPolicyException : PolicyException
    {
        public string RegKey { get; set; }
        public string RegName { get; set; }
    }
    /// <summary>
    /// The exception that is thrown when an operation cannot be performed due to a network applied group policy setting.
    /// </summary>
    public class GroupPolicyException : PolicyException
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// The exception that is thrown when an operation cannot be performed due to a policy setting.
    /// </summary>
    abstract public class PolicyException : Exception
    {
        public string ReferenceUrls { get; set; }
    }

}
