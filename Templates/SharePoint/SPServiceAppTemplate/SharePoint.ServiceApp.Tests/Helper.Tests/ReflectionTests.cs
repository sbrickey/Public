using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions; // CAS policy to run MSTests from UNC
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using SBrickey.SPServiceAppTemplate.SharePoint.ServiceApp.Tests.Helper;

namespace SBrickey.SPServiceAppTemplate.SharePoint.ServiceApp.Tests.Helper.Tests
{
    [TestClass]
    public class ReflectionTests
    {
        #region Generic Inheritance Testing : Classes inheriting from Classes

        [TestMethod]
        [TestCategory("Reflection : Primary Usage")]
        public void ArgumentException_Does_Inherit_Directly_From_SystemException()
        {
            var derivedClass = typeof(ArgumentException);
            var baseClass = typeof(SystemException);

            var result = derivedClass.InheritsFrom(baseClass);

            result.Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("Reflection : Primary Usage")]
        public void ArgumentException_Does_Inherit_Recursively_From_Exception()
        {
            var derivedClass = typeof(ArgumentException);
            var baseClass = typeof(Exception);

            var result = derivedClass.InheritsFrom(baseClass);

            result.Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("Reflection : Primary Usage")]
        public void Exception_DoesNot_Inherit_From_String()
        {
            var derivedClass = typeof(Exception);
            var baseClass = typeof(String);

            var result = derivedClass.InheritsFrom(baseClass);

            result.Should().BeFalse();
        }

        #endregion

        #region Special Case Testing : Interfaces

        [TestMethod]
        [TestCategory("Reflection : Special Case : Interface")]
        public void IDisposable_DoesNot_Inherit_From_Anything()
        {
            var derivedClass = typeof(IFormattable);
            Type baseClass = null; // not necessary for comparison

            var result = derivedClass.InheritsFrom(baseClass);

            result.Should().BeFalse();
        }

        #endregion

        #region Special Case Testing : System.Object

        [TestMethod]
        [TestCategory("Reflection : Special Case : Object")]
        public void Exception_Does_Inherit_Directly_From_Object()
        {
            var derivedClass = typeof(Exception);
            var baseClass = typeof(System.Object);

            var result = derivedClass.InheritsFrom(baseClass);

            result.Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("Reflection : Special Case : Object")]
        public void ArgumentException_Does_Inherit_Recursively_From_Object()
        {
            var derivedClass = typeof(ArgumentException);
            var baseClass = typeof(System.Object);

            var result = derivedClass.InheritsFrom(baseClass);

            result.Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("Reflection : Special Case : Object")]
        public void Byte_DoesNot_Inherit_From_Object()
        {
            var derivedClass = typeof(Byte);
            var baseClass = typeof(Object);

            var result = derivedClass.InheritsFrom(baseClass);

            result.Should().BeFalse();
        }

        #endregion

        #region Special Case Testing : System.ValueType

        [TestMethod]
        [TestCategory("Reflection : Special Case : ValueType")]
        public void Byte_Does_Inherit_From_ValueType()
        {
            var derivedClass = typeof(Byte);
            var baseClass = typeof(ValueType);

            var result = derivedClass.InheritsFrom(baseClass);

            result.Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("Reflection : Special Case : ValueType")]
        public void String_DoesNot_Inherit_From_ValueType()
        {
            var derivedClass = typeof(String);
            var baseClass = typeof(ValueType);

            var result = derivedClass.InheritsFrom(baseClass);

            result.Should().BeFalse();
        }

        #endregion





        //private interface iMyDisposable : IDisposable { }
        //private interface iMyDisposable2 : iMyDisposable { }

        //[TestMethod]
        //[TestCategory("Reflection : Special Case : Interface")]
        //public void iMyDisposable_Does_Implement_IDisposable_Directly()
        //{
        //    var derivedClass = typeof(iMyDisposable);
        //    Type baseClass = typeof(IDisposable);

        //    var result = derivedClass.Implements(baseClass);

        //    result.Should().BeTrue();
        //}

        //[TestMethod]
        //[TestCategory("Reflection : Special Case : Interface")]
        //public void iMyDisposable_Does_Implement_IDisposable_Recursively()
        //{
        //    var derivedClass = typeof(iMyDisposable2);
        //    Type baseClass = typeof(IDisposable);

        //    var result = derivedClass.Implements(baseClass);

        //    result.Should().BeTrue();
        //}



    } // namespace
} // class
