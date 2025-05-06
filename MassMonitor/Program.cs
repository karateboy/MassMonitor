using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using MassMonitor;
using Microsoft.Extensions.Configuration;

Task BroadcastLine(string token, string message)
{
    var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    var content = new
    {
        messages = new[]
        {
            new
            {
                type = "text",
                text = message
            }
        }
    };
    
    return client.PostAsync("https://api.line.me/v2/bot/message/broadcast", 
        new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json"));
}

void SendMail(MailSetting setting, string message)
{
    var mailMessage = new MailMessage();
    mailMessage.From = new MailAddress(setting.From);
    mailMessage.To.Add(setting.To);
    mailMessage.Subject = "停機警報";
    mailMessage.Body = message;

    // Create a new SmtpClient object
    SmtpClient smtpClient = new SmtpClient(setting.Host);
    smtpClient.Port = setting.Port; // or the port your SMTP server uses
    smtpClient.Credentials = new NetworkCredential(setting.Username, setting.Password);
    smtpClient.EnableSsl = true; // Enable SSL if required by your SMTP server

    try
    {
        // Send the email
        smtpClient.Send(mailMessage);
        Console.WriteLine("Email sent successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error sending email: " + ex.Message);
    }
}

Task SendSms(SmsSetting setting, string message)
{
    // Send SMS
    var client = new HttpClient();
    var smsContent = new SmsContent
    {
        UID = [setting.Account],
        PWD = [setting.Password],
        SB = ["Mass停機警報"],
        MSG = [message],
        DEST = setting.Phone.Split(",")
    };


    try
    {
        var jsonContent = JsonSerializer.Serialize(smsContent);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        return client.PostAsync("https://api.e8d.tw/API21/HTTP/sendSMS.ashx", content);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error sending SMS: " + ex.Message);
    }

    return Task.CompletedTask;
}

// See https://aka.ms/new-console-template for more information
var configRoot = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("setting.json", false)
    .Build();

var model = configRoot.Get<SettingModel>();
Debug.Assert(model is not null);
model.MailSetting = configRoot.GetSection("Mail").Get<MailSetting>();
model.SmsSetting = configRoot.GetSection("Sms").Get<SmsSetting>();


Console.WriteLine("Start Checking YL GC Mass log");
Console.WriteLine($"Log Path {model.LogPath}");
var lines = File.ReadAllLines(model.LogPath);
var firstLogAt = lines.Reverse().First(line => line.Contains("Logged at"));
var logTime = DateTime.Parse(firstLogAt.Split("Logged at")[1].Trim());
Console.WriteLine($"First log at {logTime}");
var now = DateTime.Now;
if (logTime.AddHours(1) < now)
{
    var message = $"現在時間:{now} Mass停機警報:{logTime}";
    // 停機警報
    if (model.LineToken is not null)
    {
        await BroadcastLine(model.LineToken, message);
    }

    if (model.MailSetting is not null)
    {
        Console.WriteLine("Sending Email");
        SendMail(model.MailSetting, message);
    }

    if (model.SmsSetting is not null)
    {
        Console.WriteLine("Sending SMS");
        await SendSms(model.SmsSetting, message);
    }
}
