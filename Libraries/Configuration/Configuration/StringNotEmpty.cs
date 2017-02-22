namespace SBrickey.Libraries.Configuration
{
    using SBrickey.Libraries.Configuration.ExtensionMethods;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    [AttributeUsage(validOn: AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class StringNotEmpty : Attribute
    {
        public StringNotEmpty() : base() { }

        public static IEnumerable<string> FindNull(object obj)
        {
            // properties and fields must be: public, instance (not static), and have public getter/setter
            var members = obj.GetType().GetPropertiesAndFields();
            if (!members.Any()) // if no applicable members, short circuit
                yield return null;

            foreach (var member in members)
            {
                // check for the attribute
                var attribRequired = member.GetCustomAttribute<StringNotEmpty>(false);

                // if there is no attribute, or no AppSetting value, skip
                if (attribRequired == null)
                    continue;

                // assign value using property or field applicable setter
                var typedValue = (member.MemberType == MemberTypes.Property) ? (member as PropertyInfo).GetValue(obj)
                               : (member.MemberType == MemberTypes.Field) ? (member as FieldInfo).GetValue(obj)
                               : null;

                if (typedValue == null ||
                    (typedValue is string && (typedValue as string) == String.Empty))
                    yield return member.Name;

            } // foreach prop in props

            yield return null;
        } // Validate(..)

    } // class
} // namespace