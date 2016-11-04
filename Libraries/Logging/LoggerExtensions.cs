namespace SBrickey.Libraries.Logging
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    public static class LoggerExtensions
    {
        /* 
         * These extension methods automatically generate a scope name based on the callers' method signature.
         * 
         * The name is determined by the FormatImplementation method, which currently uses the format:
         *   methodName( {methodParameters}[ = {methodParameterValues}])
         *  where:
         *   methodParameters      is a comma separated list of the parameters, using the string format {Type.FullName ParameterName}
         *   methodParameterValues is a comma separated list of the [Params] values provided in the overloaded extension method
         *   
         *  NOTE:
         *  - In prior applications, I've used the following format:
         *     (assembly) returnType namespace.class.methodName(parameters = values)
         *    which is useful when dealing with namespaces across assemblies, but was unnecessary for the Database logger as
         *    the assembly and namespace are already captured elsewhere. (LogEntity captures Assembly from the EntryAssembly, and
         *    namespace is captured from the LoggerFactory.CreateLogger parameter, which becomes the Loggers' CategoryName).
         */



        [MethodImpl(MethodImplOptions.NoInlining)] // since the code requires the stack trace
        public static IDisposable BeginAutoScope(this ILogger logger)
        {
            // grab the caller's method signature from the call stack
            var method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod(); // frame 0 is this method, frame 1 is calling method
            var methodSig = FormatImplementation(method, null);

            return logger.BeginScope(methodSig);
        }
        [MethodImpl(MethodImplOptions.NoInlining)] // since the code requires the stack trace
        public static IDisposable BeginAutoScope(this ILogger logger, params object[] methodParameters)
        {
            // grab the caller's method signature from the call stack
            var method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod(); // frame 0 is this method, frame 1 is calling method
            var methodSig = FormatImplementation(method, methodParameters);

            return logger.BeginScope(methodSig);
        }




        private static string FormatImplementation(System.Reflection.MethodBase method, object[] ParamData)
        {
            //var methodAssembly = System.IO.Path.GetFileName(method.Module.Assembly.Location);
            //var methodReturn = method is System.Reflection.ConstructorInfo ? "ctor::"
            //                 : (method as System.Reflection.MethodInfo).ReturnType.FullName;
            //var methodNamespace = method.DeclaringType == null ? "" : method.DeclaringType.FullName;
            var methodName = method is System.Reflection.ConstructorInfo ? "::ctor"
                           : method.Name;
            var methodParams = String.Join(",", method.GetParameters().Select(p => string.Format("{0} {1}", p.ParameterType.FullName, p.Name)));
            var methodParamData = ParamData != null && ParamData.Any()
                                ? String.Format(" = {0}", String.Join(", ", ParamData))
                                : String.Empty;

            var methodStr = String.Format(
                format: "{3}( {4}{5} )", // ({0}) {1} {2}.{3}( {4}{5} )  ==>  (assembly.dll) returnType namespace.class.method(paramType paramName)
                args: new object[] {
                        /* 0 */ null, // methodAssembly,
                        /* 1 */ null, // methodReturn,
                        /* 2 */ null, // methodNamespace,
                        /* 3 */ methodName,
                        /* 4 */ methodParams,
                        /* 5 */ methodParamData,
                }
            );
            return methodStr;
        }

    } // class
} // namespace