namespace SBrickey.Libraries.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Configuration;
    using SBrickey.Libraries.Configuration.ExtensionMethods;

    [AttributeUsage(validOn: AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class AppSettingAttribute : Attribute
    {

        public string Name { get; }

        public AppSettingAttribute(string name) : base() { this.Name = name; }



        public static void Load(object parametersObject, Configuration config = null)
        {
            // properties and fields must be: public, instance (not static), and have public getter/setter
            var members = parametersObject.GetType().GetPropertiesAndFields();

            if (!members.Any()) // if no applicable members, short circuit
                return;

            // now that we anticipate requiring the configuration, load and validate that settings exist
            // NOTE: according to the reflected ConfigurationManager.OpenExeConfiguration overloads, null is auto mapped to the app's exe name
            var appConfig = (config ?? ConfigurationManager.OpenExeConfiguration(null)).AppSettings();
            if (appConfig == null)
                return;

            foreach (var member in members)
            {
                // check for the attribute
                var attribAppSetting = member.GetCustomAttribute<AppSettingAttribute>(false);

                // if there is no attribute, or no AppSetting value, skip
                if (attribAppSetting == null || appConfig[attribAppSetting.Name] == null)
                    continue;

                var typedValue = null as object;
                var memberType = (member.MemberType == MemberTypes.Property) ? (member as PropertyInfo).PropertyType
                               : (member.MemberType == MemberTypes.Field) ? (member as FieldInfo).FieldType
                               : null;

                // for performance sake, if the property is a string, assign directly; otherwise, convert to a typed object
                if (memberType == typeof(string))
                    typedValue = appConfig[attribAppSetting.Name];
                else
                    typedValue = System.Convert.ChangeType(appConfig[attribAppSetting.Name], memberType);

                // if no value found, skip
                if (typedValue == null)
                    continue;

                // assign value using property or field applicable setter
                if (member.MemberType == MemberTypes.Property)
                    (member as PropertyInfo).SetValue(parametersObject, typedValue);
                if (member.MemberType == MemberTypes.Field)
                    (member as FieldInfo).SetValue(parametersObject, typedValue);

            } // foreach prop in props

        } // Load(..)

    } // class
} // namespace