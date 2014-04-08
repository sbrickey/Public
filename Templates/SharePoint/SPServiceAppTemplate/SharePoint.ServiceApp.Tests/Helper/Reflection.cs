using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBrickey.SPServiceAppTemplate.SharePoint.ServiceApp.Tests.Helper
{
    public static class Reflection
    {

        public static bool InheritsFrom(this Type T, Type baseType)
        {
            // if T is an interface with no base interface, false
            if (T.BaseType == null)
                return false;

            // if T only inherits from System.Object, and that's not what we're testing for... false
            if (T.BaseType == typeof(System.Object) && baseType != typeof(System.Object))
                return false;

            // same goes for ValueType (structs)... even though ValueType eventually inherits from Object (for boxing), which just ends up confusing things
            if (T.BaseType == typeof(System.ValueType) && baseType != typeof(ValueType))
                return false;

            // otherwise, check the base type, or check for recursion
            var outval = (T.BaseType.TypeEquals(baseType)) ||
                         (T.BaseType.InheritsFrom(baseType)); // recursion

            return outval;
        }

        /// <summary>
        /// custom implementation of type comparer
        /// </summary>
        /// <remarks>
        /// As it turns out, there are two types of System.Type : the RuntimeType and the ReflectOnlyType.
        /// One is used for Types reflected from the runtime (assemblies loaded into the app domain).
        /// The other is used for Types reflected WITHOUT being loaded in the runtime.
        /// 
        /// Additionally, since the two types are different, their equality operators were never designed to handle this case.
        /// This implementation uses FullName and GUIDs to match, and hopefully is sufficiently accurate.
        /// </remarks>
        public static bool TypeEquals(this Type T1, Type T2)
        {
            return T1.FullName == T2.FullName && T1.GUID == T2.GUID;
        }

    } // class
} // namespace
