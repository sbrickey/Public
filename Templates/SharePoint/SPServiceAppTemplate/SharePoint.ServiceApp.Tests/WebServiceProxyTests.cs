using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using SBrickey.SPServiceAppTemplate.SharePoint.ServiceApp.Tests.Helper;

namespace SBrickey.SPServiceAppTemplate.SharePoint.ServiceApp.Tests
{
    [TestClass]
    public class WebServiceProxyTests
    {
        [TestMethod]
        public void Should_Have_Well_Defined_SupportedServiceApplicationAttribute()
        {
            // location of this test assembly
            var testAssembly_PathAndFile = this.GetType().Assembly.Location;
            var testAssembly_Path = System.IO.Path.GetDirectoryName(testAssembly_PathAndFile);

            // scan directory for assemblies
            var assembliesToScan = System.IO.Directory.GetFiles(testAssembly_Path, "*.dll");

            // prep the appdomain's ReflectionOnlyLoadFrom reference resolution...
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (sender, e) => { return System.Reflection.Assembly.ReflectionOnlyLoad(e.Name); };

            foreach (var assemblyToScan in assembliesToScan)
            {
                var assembly = System.Reflection.Assembly.ReflectionOnlyLoadFrom(assemblyToScan);

                // check assembly for classes : SPWebServiceProxy
                var WebServiceProxies = assembly.GetTypes()
                                                .Where(t => t.InheritsFrom(typeof(Microsoft.SharePoint.Administration.SPIisWebServiceProxy)))
                                                .ToList(); // materialize

                foreach (var WebServiceProxy in WebServiceProxies)
                {
                    var WebServiceProxy_Attributes = System.Reflection
                                                           .CustomAttributeData
                                                           .GetCustomAttributes(WebServiceProxy)
                                                           .Where(a => a.Constructor
                                                                        .DeclaringType
                                                                        .TypeEquals(typeof(Microsoft.SharePoint.Administration.SupportedServiceApplicationAttribute))
                                                           )
                                                           .ToList(); // materialize

                    // check that the attribute exists (once)
                    WebServiceProxy_Attributes.Count.Should().Be(
                        1,
                        "SPIisWebServiceProxy implementation {0} should define its SupportedServiceApplication",
                        WebServiceProxy.FullName
                    );

                    // reference to the single attribute, for simplicity
                    var WebServiceProxy_Attribute = WebServiceProxy_Attributes.DefaultIfEmpty(null)
                                                                              .SingleOrDefault();

                    if (WebServiceProxy_Attribute != null)
                    {
                        #region Check SupportedServiceApp.GUID
                        var SupportedServiceApp_GUID = WebServiceProxy_Attribute.ConstructorArguments[0].Value as string;

                        // check the assembly for types
                        //    with a GUID attribute
                        //         with the same GUID as the SupportedServiceApp
                        var ClassesWithGuidMatchingSupportedServiceApp_GUID =
                            assembly.GetTypes()
                                    .Where(t => System.Reflection
                                                      .CustomAttributeData
                                                      .GetCustomAttributes(t)
                                                      .Where(a => a.Constructor
                                                                   .DeclaringType
                                                                   .TypeEquals(typeof(System.Runtime.InteropServices.GuidAttribute))
                                                               && a.ConstructorArguments[0].Value as string == SupportedServiceApp_GUID
                                                      )
                                                      .Any()
                                          )
                                    .ToList(); // materialize
                        
                        ClassesWithGuidMatchingSupportedServiceApp_GUID.Count.Should().Be(
                            1,
                            "GUID ({0}) in SupportedServiceApplication from {1} should match the GUID of a class in the assembly",
                            SupportedServiceApp_GUID,
                            WebServiceProxy.FullName
                        );

                        // check that the class is a WebServiceApp
                        var WebServiceApp = ClassesWithGuidMatchingSupportedServiceApp_GUID.DefaultIfEmpty(null).SingleOrDefault();
                        if (WebServiceApp != null)
                        {
                            var IsWebServiceApp = WebServiceApp.InheritsFrom(typeof(Microsoft.SharePoint.Administration.SPIisWebServiceApplication));
                            IsWebServiceApp.Should().BeTrue(
                                "SPIisWebServiceProxy implementation {0} has SupportedServiceApplication with GUID ({1}) which matches class {2}, but that class should be an implementation of SPIisWebServiceApplication",
                                WebServiceProxy.FullName,
                                SupportedServiceApp_GUID,
                                WebServiceApp.FullName
                            );
                        }
                        #endregion

                        #region Check SupportedServiceApp.Version

                        var SupportedServiceApp_Version = WebServiceProxy_Attribute.ConstructorArguments[1].Value as string;

                        #endregion

                        #region Check SupportedServiceApp.ProxyType

                        var SupportedServiceApp_ProxyType = WebServiceProxy_Attribute.ConstructorArguments[2].Value as Type;

                        // check the Proxy Type
                        var IsWebServiceAppProxy = SupportedServiceApp_ProxyType.InheritsFrom(typeof(Microsoft.SharePoint.Administration.SPIisWebServiceApplicationProxy));
                        IsWebServiceAppProxy.Should().BeTrue(
                            "SPIisWebServiceProxy implementation {0} has SupportedServiceApplication with proxyType ({1}), but that class should be an implementation of SPIisWebServiceApplicationProxy",
                            WebServiceProxy.FullName,
                            SupportedServiceApp_ProxyType.FullName
                        );

                        #endregion

                    } // WebServiceProxy_attribute != null

                } // foreach WebServiceProxy

            } // foreach assemblyToScan

            // check for SupportedServiceApplicationAttribute

            // check assembly for classes with:
            //    GuidAttribute from SupportedServiceApplicationAttribute
            //    : SPWebServiceAppProxy
        }
    } // class
} // namespace
