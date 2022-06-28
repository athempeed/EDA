using EDA.Core.Infrastructure.Messaging.Contracts;
using EDA.Core.Infrastructure.Messaging.Messages;
using EDA.Models;
using Microsoft.AspNetCore.Mvc;

namespace EDA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly ILogger<EmailController> _logger;
        private readonly IPublishMessageToBus _publisher;

        public EmailController(ILogger<EmailController> logger, IPublishMessageToBus publisher)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpPost("/SendEmail")]
        public IActionResult SendMail([FromBody] EmailData message)
        {
            _logger.LogInformation("sendEmail called");
            var email = new EmailMessage
            {
                Bcc = message?.Bcc,
                Body = message?.Body,
                Cc = message?.Cc,
                From = message?.From,
                Subject = message?.Subject,
                To = message?.To
            };
            _publisher.Publish(email);
            return Ok();
            
        }

    }
}
