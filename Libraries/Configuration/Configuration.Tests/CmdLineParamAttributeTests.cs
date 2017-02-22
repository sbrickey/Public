using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace SBrickey.Libraries.Configuration.Tests
{
    [TestClass]
    public class CmdLineParamAttributeTests
    {
        private class MockObj_Property
        {
            [CmdLineParam("A")]
            public string A { get; set; }
        }
        private class MockObj_Field
        {
            [CmdLineParam("A")]
            public string A;
        }
        private class MockObj_Complex
        {
            [CmdLineParam("A")]
            public string A { get; set; }

            [CmdLineParam("B")]
            public string B;
        }

        [TestMethod]
        public void Load_Property()
        {
            // arrange
            var obj = new MockObj_Property();
            var objParams = new CommandLineParameters("/A=1");

            // act
            CmdLineParamAttribute.Load(obj, objParams);

            // assert
            obj.A.Should().Be("1");
        }

        [TestMethod]
        public void Load_Field()
        {
            // arrange
            var obj = new MockObj_Field();
            var objParams = new CommandLineParameters("/A=1");

            // act
            CmdLineParamAttribute.Load(obj, objParams);

            // assert
            obj.A.Should().Be("1");
        }

        [TestMethod]
        public void Load_Complex()
        {
            // arrange
            var obj = new MockObj_Complex();
            var objParams = new CommandLineParameters("/A=1 /B=2");

            // act
            CmdLineParamAttribute.Load(obj, objParams);

            // assert
            obj.A.Should().Be("1");
            obj.B.Should().Be("2");
        }


    } // class
} // namespace