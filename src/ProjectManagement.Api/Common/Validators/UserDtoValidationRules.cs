using FluentValidation;

namespace ProjectManagement.Api.Common.Validators;

public static class UserDtoValidationRules
{
    public static IRuleBuilderOptions<T, string> ValidateUserName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("UserName is required and must not exceed 50 characters.");
    }

    public static IRuleBuilderOptions<T, string> ValidateEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100)
            .WithMessage("Email is required and must be a valid email address.");
    }

    public static IRuleBuilderOptions<T, string> ValidateFullName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("FullName is required and must not exceed 100 characters.");
    }
}
