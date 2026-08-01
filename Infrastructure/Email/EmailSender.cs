using System.Net;
using System.Net.Mail;
using System.Text;
using Application.Features.Auth.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Infrastructure.Email;

public class EmailSender:IEmailSender
{
    private readonly SmtpSettings _smtpSettings;
    private readonly IHostingEnvironment _env;
    public EmailSender(IOptions<SmtpSettings> smtpSettings, IHostingEnvironment env)
    {
        _env = env;
        _smtpSettings = smtpSettings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using (var client = new SmtpClient())
        {
            client.Host = _smtpSettings.Host;
            client.Port = _smtpSettings.Port;
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);

            var mail = new MailMessage();
            mail.From = new MailAddress(_smtpSettings.Username, _smtpSettings.From);
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body=body;
            mail.IsBodyHtml = true;

            try
            {
                await client.SendMailAsync(mail);
            }
            catch (SmtpException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StatusCode);
                Console.WriteLine(ex.InnerException?.Message);

                throw;
            }
        }
        
    }
    public async Task<string> RenderPasswordResetAsync(string code)
    {
        var path = Path.Combine(
            _env.ContentRootPath,
            "Email",
            "Templates",
            "PasswordReset.html"
        );
 

        if (!File.Exists(path))
            throw new FileNotFoundException("PasswordReset email template not found.", path);

        var html = await File.ReadAllTextAsync(path, Encoding.UTF8);

        return html.Replace("{{CODE}}", code);
    }
}