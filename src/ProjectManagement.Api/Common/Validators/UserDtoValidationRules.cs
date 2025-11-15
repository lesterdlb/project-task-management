using FluentValidation;

namespace ProjectManagement.Api.Common.Validators;

public static class UserDtoValidationRules
{
    extension<T>(IRuleBuilder<T, string> ruleBuilder)
    {
        public void ValidateUserName()
        {
            ruleBuilder
                .NotEmpty()
                .MaximumLength(50)
                .WithMessage("UserName is required and must not exceed 50 characters.");
        }

        public void ValidateEmail()
        {
            ruleBuilder
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(100)
                .WithMessage("Email is required and must be a valid email address.");
        }

        public void ValidateFullName()
        {
            ruleBuilder
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("FullName is required and must not exceed 100 characters.");
        }
    }
}
