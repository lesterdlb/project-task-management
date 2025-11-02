namespace ProjectManagement.Api.Common.Services.Email;

public sealed class EmailService(ILogger<EmailService> logger) : IEmailService
{
    public Task SendEmailConfirmationAsync(string email, string userName, string confirmationLink)
    {
        logger.LogInformation("Email Confirmation for {Email} ({UserName})\nConfirmation Link: {ConfirmationLink}",
            email, userName, confirmationLink);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string userName, string resetLink)
    {
        logger.LogInformation("Password Reset for {Email} ({UserName})\nReset Link: {ResetLink}",
            email, userName, resetLink);

        return Task.CompletedTask;
    }
}
