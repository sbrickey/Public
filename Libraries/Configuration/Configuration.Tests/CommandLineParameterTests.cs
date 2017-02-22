using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace SBrickey.Libraries.Configuration.Tests
{
    [TestClass]
    public class CommandLineParameterTests
    {

        [TestMethod]
        public void String_DefaultSettings_SlashPrefix_EqualsSeparator()
        {
            // arrange
            var inp = "/a=1";

            // act
            var values = new Configuration.CommandLineParameters(inp);

            // assert
            values.Should().Contain("a", "1");
        }

        [TestMethod]
        public void String_DefaultSettings_HyphenPrefix_EqualsSeparator()
        {
            // arrange
            var inp = "-a=1";

            // act
            var values = new Configuration.CommandLineParameters(inp);

            // assert
            values.Should().Contain("a", "1");
        }

        [TestMethod]
        public void String_DefaultSettings_DoubleHyphenPrefix_EqualsSeparator()
        {
            // arrange
            var inp = "--a=1";

            // act
            var values = new Configuration.CommandLineParameters(inp);

            // assert
            values.Should().Contain("a", "1");
        }

        [TestMethod]
        public void String_DefaultSettings_SlashPrefix_ColonSeparator()
        {
            // arrange
            var inp = "/a:1";

            // act
            var values = new Configuration.CommandLineParameters(inp);

            // assert
            values.Should().Contain("a", "1");
        }

        [TestMethod]
        public void String_DefaultSettings_Switch()
        {
            // arrange
            var inp = "/a";

            // act
            var values = new Configuration.CommandLineParameters(inp);

            // assert
            values.Should().Contain("a", Boolean.TrueString);
        }

        [TestMethod]
        public void String_ComplexCombination()
        {
            // arrange
            var inp = "/a=1 /b=2 -c --d:z"; // various prefixes (/, -, --) and value separators (=, :), as well as a value-less switch (c)

            // act
            var values = new Configuration.CommandLineParameters(inp);

            // assert
            values.Should().Contain("a", "1");
            values.Should().Contain("b", "2");
            values.Should().Contain("c", Boolean.TrueString);
            values.Should().Contain("d", "z");
        }



        [TestMethod]
        public void StringArray_ComplexCombination()
        {
            // arrange
            var inp = new string[] { "/a=1", "/b=2", "-c", "--d:z" }; // various prefixes (/, -, --) and value separators (=, :), as well as a value-less switch (c)

            // act
            var values = new Configuration.CommandLineParameters(inp);

            // assert
            values.Should().Contain("a", "1");
            values.Should().Contain("b", "2");
            values.Should().Contain("c", Boolean.TrueString);
            values.Should().Contain("d", "z");
        }


        
    } // class
} // namespace