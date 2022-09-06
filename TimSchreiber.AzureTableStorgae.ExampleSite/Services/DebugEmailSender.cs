using Microsoft.AspNetCore.Identity.UI.Services;
using System.Diagnostics;

namespace TimSchreiber.AzureTableStorgae.ExampleSite.Services
{
    public class DebugEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Debug.WriteLine("########################################");
            Debug.WriteLine($"To:      {email}");
            Debug.WriteLine($"Subject: {subject}");
            Debug.WriteLine("----------------------------------------");
            Debug.WriteLine(htmlMessage);
            Debug.WriteLine("########################################");
            return Task.CompletedTask;
        }
    }
}
