using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using MimeKit.Utils;

namespace ServiceX.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string otp)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        // تحميل الصورة من wwwroot/images/logo3.jpg
        var builder = new BodyBuilder();

        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logo3.jpg");
        var image = builder.LinkedResources.Add(imagePath);
        image.ContentId = MimeUtils.GenerateMessageId();
        string spacedOtp = string.Join(" ", otp.ToCharArray());
        // HTML Body يحتوي على صورة CID
        builder.HtmlBody = $@"
        <html>
            <body style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px; color: #333;'>
                <div style='text-align: center; margin-bottom: 20px;'>
                    <img src='cid:{image.ContentId}' alt='ServiceX Logo' width='200' style='margin-bottom: 10px;' />
                    <h2>ServiceX</h2>
                </div>
                <p style='font-size: 20px; text-align: center;'>مرحبًا</p>               
                <p style='font-size: 20px; text-align: center;'>شكرًا لاستخدامك خدمة سيرفس اكس</p>
                <p style='font-size: 20px; text-align: center;'>استخدم رمز التحقق التالي لإتمام إجراء تغيير كلمة المرور الخاصة بك</p>    
                <p style='font-size: 20px; text-align: center;'>يرجى عدم مشاركة هذا الرمز مع أي شخص، بما في ذلك موظفي سيرفس اكس، حفاظًا على خصوصيتك وأمان حسابك</p>    
                <p style='font-size: 20px; text-align: center;'>الكود صالح لمدة 10 دقائق</p>
                <p style='font-size: 20px; text-align: center;'> شكرًا لاستخدامك <strong>سيرفس اكس</strong></p>
                <div style='font-size: 30px; font-weight: bold; background-color: #e3f2fd; padding: 10px; text-align: center; margin: 10px 0;'>{spacedOtp}</div>
            </body>
        </html>";

        email.Body = builder.ToMessageBody();

        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
