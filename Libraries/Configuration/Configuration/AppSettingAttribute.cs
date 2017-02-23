namespace SBrickey.Libraries.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Configuration;
    using SBrickey.Libraries.Configuration.ExtensionMethods;
    using System.Collections.Specialized;

    [AttributeUsage(validOn: AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class AppSettingAttribute : Attribute
    {

        public string Name { get; }

        public AppSettingAttribute(string name) : base() { this.Name = name; }


        public static void Load(object parametersObject) { Load(parametersObject, ConfigurationManager.AppSettings); }
        //public static void Load(object parametersObject, Configuration config) { Load(parametersObject, config.AppSettings.); }
        internal static void Load(object parametersObject, NameValueCollection appConfigSettings)
        {
            // properties and fields must be: public, instance (not static), and have public getter/setter
            var members = parametersObject.GetType().GetPropertiesAndFields();

            if (!members.Any()) // if no applicable members, short circuit
                return;

            // initially this had some code around loading the app config here, based on the Configuration object as a parameter.
            // effectively, that approach was damn annoying to test (shims and fakes and such). Instead, this method
            // is just wrapped by overload(s) to handle configuration injection
            if (appConfigSettings == null)
                return;

            foreach (var member in members)
            {
                // check for the attribute
                var attribAppSetting = member.GetCustomAttribute<AppSettingAttribute>(false);

                // if there is no attribute, or no AppSetting value, skip
                if (attribAppSetting == null || appConfigSettings[attribAppSetting.Name] == null)
                    continue;

                var typedValue = null as object;
                var memberType = (member.MemberType == MemberTypes.Property) ? (member as PropertyInfo).PropertyType
                               : (member.MemberType == MemberTypes.Field) ? (member as FieldInfo).FieldType
                               : null;

                // for performance sake, if the property is a string, assign directly; otherwise, convert to a typed object
                if (memberType == typeof(string))
                    typedValue = appConfigSettings[attribAppSetting.Name];
                else
                    typedValue = System.Convert.ChangeType(appConfigSettings[attribAppSetting.Name], memberType);

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