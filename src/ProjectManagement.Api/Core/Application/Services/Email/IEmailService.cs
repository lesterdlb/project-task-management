namespace ProjectManagement.Api.Core.Application.Services.Email;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string email, string userName, string confirmationLink);
    Task SendPasswordResetAsync(string email, string userName, string resetLink);
}
