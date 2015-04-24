using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace DirectoryServices.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void GetAll_Check_Count()
        {
            // Arrange

            // Act
            var Results = SDS.SPConnectors.DirectoryServices.DomainControllers.GetAll();

            // Assert
            Results.Should().NotBeNull();
            Results.Should().HaveCount(1);
        }
        [TestMethod]
        public void GetAll_Check_FirstResult()
        {
            // Arrange

            // Act
            var Result = SDS.SPConnectors.DirectoryServices.DomainControllers.GetAll().FirstOrDefault();

            // Assert
            Result.Should().NotBeNull();
            Result.HostName.ToLower().Should().StartWith("server");
            Result.HostName.ToLower().Should().EndWith("domain.local");
            Result.IPAddress.Should().NotBeEmpty();
            Result.Site.Should().NotBeEmpty();
        }
    }
}
