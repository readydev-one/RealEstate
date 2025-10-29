// RealEstate.Infrastructure/Services/EmailService.cs
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using RealEstate.Application.Interfaces;
using System.Text;

namespace RealEstate.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly GmailService _gmailService;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public EmailService(IConfiguration configuration)
    {
        var serviceAccountEmail = configuration["Gmail:ServiceAccountEmail"];
        var serviceAccountKeyPath = configuration["Gmail:ServiceAccountKeyPath"];
        _senderEmail = configuration["Gmail:SenderEmail"] ?? "noreply@realestate.com";
        _senderName = configuration["Gmail:SenderName"] ?? "Real Estate Platform";

        var credential = GoogleCredential.FromFile(serviceAccountKeyPath)
            .CreateScoped(GmailService.Scope.GmailSend);

        _gmailService = new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "RealEstateAPI"
        });
    }

    public async Task SendInvitationEmailAsync(string toEmail, string invitationToken, string propertyAddress, string role)
    {
        var subject = "You're invited to join a Real Estate Transaction";
        var body = $@"
            <html>
            <body>
                <h2>Real Estate Transaction Invitation</h2>
                <p>You have been invited to participate as a <strong>{role}</strong> in a real estate transaction.</p>
                <p><strong>Property:</strong> {propertyAddress}</p>
                <p>Click the link below to register and accept this invitation:</p>
                <p><a href='https://your-app-url.com/register?token={invitationToken}'>Accept Invitation</a></p>
                <p>This invitation will expire in 7 days.</p>
                <p>Best regards,<br/>Real Estate Platform Team</p>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendDocumentApprovedNotificationAsync(string toEmail, string propertyAddress, string documentName)
    {
        var subject = "Document Approved";
        var body = $@"
            <html>
            <body>
                <h2>Document Approved</h2>
                <p>A document has been approved for property: <strong>{propertyAddress}</strong></p>
                <p><strong>Document:</strong> {documentName}</p>
                <p>You can now view this document in your dashboard.</p>
                <p>Best regards,<br/>Real Estate Platform Team</p>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendTaskReminderAsync(string toEmail, string taskTitle, DateTime dueDate)
    {
        var subject = $"Task Reminder: {taskTitle}";
        var body = $@"
            <html>
            <body>
                <h2>Task Reminder</h2>
                <p>You have an upcoming task that requires your attention:</p>
                <p><strong>Task:</strong> {taskTitle}</p>
                <p><strong>Due Date:</strong> {dueDate:MMMM dd, yyyy}</p>
                <p>Please log in to your dashboard to complete this task.</p>
                <p>Best regards,<br/>Real Estate Platform Team</p>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendClosingDateReminderAsync(string toEmail, string propertyAddress, DateTime closingDate)
    {
        var subject = $"Closing Date Reminder: {propertyAddress}";
        var body = $@"
            <html>
            <body>
                <h2>Closing Date Reminder</h2>
                <p>This is a reminder about an upcoming closing date:</p>
                <p><strong>Property:</strong> {propertyAddress}</p>
                <p><strong>Closing Date:</strong> {closingDate:MMMM dd, yyyy}</p>
                <p>Please ensure all required documents and tasks are completed before the closing date.</p>
                <p>Best regards,<br/>Real Estate Platform Team</p>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendCloserApprovalRequestAsync(string adminEmail, string closerName, string closerEmail)
    {
        var subject = "New Closer Approval Request";
        var body = $@"
            <html>
            <body>
                <h2>New Closer Approval Request</h2>
                <p>A new closer has registered and requires your approval:</p>
                <p><strong>Name:</strong> {closerName}</p>
                <p><strong>Email:</strong> {closerEmail}</p>
                <p>Please log in to the admin dashboard to review and approve this request.</p>
                <p>Best regards,<br/>Real Estate Platform Team</p>
            </body>
            </html>
        ";

        await SendEmailAsync(adminEmail, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var message = new Message
        {
            Raw = Base64UrlEncode(CreateMimeMessage(toEmail, subject, htmlBody))
        };

        await _gmailService.Users.Messages.Send(message, "me").ExecuteAsync();
    }

    private string CreateMimeMessage(string to, string subject, string htmlBody)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"To: {to}");
        sb.AppendLine($"From: {_senderName} <{_senderEmail}>");
        sb.AppendLine($"Subject: {subject}");
        sb.AppendLine("Content-Type: text/html; charset=utf-8");
        sb.AppendLine();
        sb.AppendLine(htmlBody);

        return sb.ToString();
    }

    private string Base64UrlEncode(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }
}