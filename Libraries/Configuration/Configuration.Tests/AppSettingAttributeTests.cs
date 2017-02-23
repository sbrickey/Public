using System;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace SBrickey.Libraries.Configuration.Tests
{
    [TestClass]
    public class AppSettingAttributeTests
    {
        private class MockObj_Property
        {
            [AppSetting("A")]
            public string A { get; set; }

        }
        private class MockObj_Field
        {
            [AppSetting("A")]
            public string A;
        }
        private class MockObj_Complex
        {
            [AppSetting("A")]
            public string A { get; set; }

            [AppSetting("B")]
            public string B;
        }

        [TestMethod]
        public void Load_Property()
        {
            using (ShimsContext.Create())
            {
                // arrange
                var obj = new MockObj_Property();
                var settings = new NameValueCollection() { { "A", "1" } };

                // act
                AppSettingAttribute.Load(obj, settings);

                // assert
                obj.A.Should().Be("1");
            } // using Fakes
        } // TestMethod


        [TestMethod]
        public void Load_Field()
        {
            using (ShimsContext.Create())
            {
                // arrange
                var obj = new MockObj_Field();
                var settings = new NameValueCollection() { { "A", "1" } };

                // act
                AppSettingAttribute.Load(obj, settings);

                // assert
                obj.A.Should().Be("1");
            } // using Fakes
        } // Load_Field


        [TestMethod]
        public void Load_Complex()
        {
            using (ShimsContext.Create())
            {
                // arrange
                var obj = new MockObj_Complex();
                var settings = new NameValueCollection() { { "A", "1" }, { "B", "2" } };

                // act
                AppSettingAttribute.Load(obj, settings);

                // assert
                obj.A.Should().Be("1");
            } // using Fakes
        } // Load_Complex


    } // class
} // namespace