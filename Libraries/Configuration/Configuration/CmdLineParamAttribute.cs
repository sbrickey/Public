namespace SBrickey.Libraries.Configuration
{
    using SBrickey.Libraries.Configuration.ExtensionMethods;
    using System;
    using System.Linq;
    using System.Reflection;

    [AttributeUsage(validOn: AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class CmdLineParamAttribute : Attribute
    {
        public string Name { get; }

        public CmdLineParamAttribute(string name) : base() { this.Name = name; }


        /// <summary>
        /// Loads the parameters from the command line argument automatically via Environment.GetCommandLineArgs
        /// </summary>
        /// <param name="parametersObject"></param>
        public static void Load(object parametersObject) { Load(parametersObject, new CommandLineParameters(Environment.GetCommandLineArgs())); }

        /// <summary>
        /// Loads the parameters from the command line arguments provided
        /// </summary>
        /// <param name="parametersObject"></param>
        /// <param name="args"></param>
        public static void Load(object parametersObject, string[] args) { Load(parametersObject, new CommandLineParameters(args)); }

        /// <summary>
        /// Loads the parameters from the command line parameters provided. This option allows non-default CommandLineParameterSettings.
        /// </summary>
        /// <param name="parametersObject"></param>
        /// <param name="parameterValues"></param>
        public static void Load(object parametersObject, CommandLineParameters parameterValues)
        {
            if (parameterValues == null)
                throw new ArgumentNullException(nameof(parameterValues));

            if (!parameterValues.Any()) // if no parameters, let's just save ourselves a whole bunch of time
                return;

            // properties and fields must be: public, instance (not static), and have public getter/setter
            var members = parametersObject.GetType().GetPropertiesAndFields();
            if (!members.Any()) // if no applicable members, short circuit
                return;

            foreach (var member in members)
            {
                // check for the attribute
                var attribCmdLineParam = member.GetCustomAttribute<CmdLineParamAttribute>(false);

                // if there is no attribute, or no CmdLineParam value, skip
                if (attribCmdLineParam == null || !parameterValues.ContainsKey(attribCmdLineParam.Name))
                    continue;

                var typedValue = null as object;
                var memberType = (member.MemberType == MemberTypes.Property) ? (member as PropertyInfo).PropertyType
                               : (member.MemberType == MemberTypes.Field) ? (member as FieldInfo).FieldType
                               : null;

                // for performance sake, if the property is a string, assign directly; otherwise, convert to a typed object
                if (memberType == typeof(string))
                    typedValue = parameterValues[attribCmdLineParam.Name];
                else
                    typedValue = System.Convert.ChangeType(parameterValues[attribCmdLineParam.Name], memberType);

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