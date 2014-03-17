using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace SmtpSimulator.Core.Tests
{
    [TestClass]
    public class SmtpSimulatorTests
    {
        Core.SmtpSimulator server = null;

        [TestInitialize]
        public void Initialize()
        {
            // use port 8025 to avoid elevated privileges required for ports <= 1024
            server = new Core.SmtpSimulator(System.Net.IPAddress.Any, 8025);
            server.Start();
        }
        [TestCleanup]
        public void Cleanup()
        {
            server.Stop();
            server = null;
        }


        [TestMethod]
        public void SendMailMessage()
        {
            // Arrange
            var smtpClient = new System.Net.Mail.SmtpClient(
                                host: "localhost",
                                port: 8025
            );
            var mailMessage = new System.Net.Mail.MailMessage(
                                from: "fromaddress@domain.com",
                                to: "toaddress@domain.com",
                                subject: "subject",
                                body: "body"
            );

            // Act
            smtpClient.Send(mailMessage);

            // Assert
            SmtpSimulator.MailQueue.Count.Should().Be(1);
            
            var msg = SmtpSimulator.MailQueue[0];
            msg.From.Should().Be("fromaddress@domain.com");
            msg.Recipients.Count.Should().Be(1);
            msg.Recipients[0].Should().Be("toaddress@domain.com");
            msg.Message.Should().Contain("subject");
            msg.Message.Should().Contain("body");
        }

    
    } // class
} // namespace