using EDA.Core.Infrastructure.Messaging.Messages;
using EmailService.Handlers;
using EmailService.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit.Abstractions;

namespace EDA.EmailService.Test.Handlers
{
    public class EmailMessageHandlerTest
    {
        Mock<ILogger<EmailMessageHandler>> loggerMock;
        private ILogger<EmailMessageHandler> _logger;

        Mock<IGraphClient> mockg;
        private IGraphClient _graphClient;
        private EmailMessageHandler messageHandler;

        public EmailMessageHandlerTest()
        {
            loggerMock = new Mock<ILogger<EmailMessageHandler>>();
            _logger = loggerMock.Object;
            mockg = new Mock<IGraphClient>();
            mockg.Setup(x => x.SendMail(It.IsAny<EmailMessage>()));
            _graphClient = mockg.Object;
            messageHandler = new EmailMessageHandler(_logger, _graphClient);
        }
        [Fact]
        public void Should_throw_exception_when_GraphClient_is_null()
        {
            //Act & assert
            Should.Throw<ArgumentNullException>(() => new EmailMessageHandler(_logger,null));
        }

        [Fact]
        public void Should_throw_exception_when_logger_is_null()
        {

            //Act & assert
            Should.Throw<ArgumentNullException>(() => new EmailMessageHandler(null, _graphClient));
        }


        [Fact]
        public void Should_Log_Error_If_Message_Is_Null()
        {
            //Arrange
            var msg = new EmailMessage
            {
                To = "test@gmail.com",
                Body = "this is test.",
                From = "TEST"
            };

            //Act
            messageHandler.Handle(null);

            //Assert
            loggerMock.Verify(
                    x => x.Log(
                        It.IsAny<LogLevel>(),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()== "Message is null."),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }

        [Fact]
        public void Should_Send_Message_Success()
        {
            //Arrange
            var msg = new EmailMessage
            {
                To = "test@gmail.com",
                Body = "this is test.",
                From = "TEST"
            };

            //Act
            messageHandler.Handle(msg);

            //Assert
            loggerMock.Verify(
                    x => x.Log(
                        It.IsAny<LogLevel>(),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("email sent successfully to")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }
    }
}
