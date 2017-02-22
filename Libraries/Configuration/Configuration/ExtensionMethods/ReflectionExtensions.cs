namespace SBrickey.Libraries.Configuration.ExtensionMethods
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Reflection;

    public static class ReflectionExtensions
    {
        public static List<MemberInfo> GetPropertiesAndFields(this Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
        {
            var props = type.GetProperties(flags | BindingFlags.GetProperty | BindingFlags.SetProperty);
            var fields = type.GetFields(flags | BindingFlags.GetField | BindingFlags.SetField);
            var members = new List<MemberInfo>();
            members.AddRange(props);
            members.AddRange(fields);

            return members;
        }
        
    } // class
} // namespace