using FluentValidation;
using ProjectManagement.Api.Common.Domain.Enums;

namespace ProjectManagement.Api.Common.Validators;

public static class ProjectDtoValidationRules
{
    public static IRuleBuilderOptions<T, string> ValidateProjectName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Project name is required.")
            .MaximumLength(200)
            .WithMessage("Project name must not exceed 200 characters.");
    }

    public static IRuleBuilderOptions<T, string> ValidateProjectDescription<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(5000)
            .WithMessage("Description must not exceed 5000 characters.");
    }

    public static IRuleBuilderOptions<T, DateTime> ValidateProjectStartDate<T>(
        this IRuleBuilder<T, DateTime> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .GreaterThanOrEqualTo(DateTime.Now)
            .WithMessage("Start date is required and must be in the future.");
    }

    public static IRuleBuilderOptions<T, DateTime?> ValidateProjectEndDate<T>(
        this IRuleBuilder<T, DateTime?> ruleBuilder,
        Func<T, DateTime> startDateSelector)
    {
        return ruleBuilder
            .GreaterThan(x => startDateSelector(x))
            .WithMessage("End date must be after start date.");
    }

    public static IRuleBuilderOptions<T, ProjectStatus> ValidateProjectStatus<T>(
        this IRuleBuilder<T, ProjectStatus> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum()
            .WithMessage("Status is required and must be a valid value.");
    }

    public static IRuleBuilderOptions<T, Priority> ValidateProjectPriority<T>(
        this IRuleBuilder<T, Priority> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum()
            .WithMessage("Priority is required and must be a valid value.");
    }
}
