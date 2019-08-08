using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;
using System.Text;
using System.Net.Http;
using System.Net;

namespace Aramiz
{
    public static class AramizContactMeApi
    {
        [FunctionName("ContactMe")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [SendGrid(ApiKey = "SendGridApiKey")] IAsyncCollector<SendGridMessage> messageCollector,
            ILogger log)
        {
            try
            {
                log.LogInformation("Sending Email...");

                string requestBody = await new StreamReader(req.Body, Encoding.UTF8).ReadToEndAsync();
                var emailObject = JsonConvert.DeserializeObject<OutgoingEmail>(requestBody);

                var message = new SendGridMessage();
                message.AddTo(emailObject.To);
                message.AddContent("text/html", "From: " 
                    + emailObject.From + "<br/> Phone: " 
                    + emailObject.Phone + "<br/><br/>" 
                    + emailObject.Body);
                message.SetFrom(new EmailAddress(emailObject.From));
                message.SetSubject("From Aramiz.net - " + emailObject.Subject);

                await messageCollector.AddAsync(message);

                return new OkObjectResult("Email Sent Successfully - From: " + emailObject.From);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("Error sending email: " + ex);
            }
        }

        public class OutgoingEmail
        {
            public string To { get;} = "emil@aramiz.net";
            public string From { get; set; }
            public string Subject { get; set; }
            public string Phone { get; set; }
            public string Body { get; set; }
        }
    }
}
