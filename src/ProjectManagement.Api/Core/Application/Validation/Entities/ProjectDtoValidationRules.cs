using FluentValidation;
using ProjectManagement.Api.Core.Domain.Enums;

namespace ProjectManagement.Api.Core.Application.Validation.Entities;

public static class ProjectDtoValidationRules
{
    extension<T>(IRuleBuilder<T, string> ruleBuilder)
    {
        public void ValidateProjectName()
        {
            ruleBuilder
                .NotEmpty()
                .WithMessage("Project name is required.")
                .MaximumLength(200)
                .WithMessage("Project name must not exceed 200 characters.");
        }

        public void ValidateProjectDescription()
        {
            ruleBuilder
                .MaximumLength(5000)
                .WithMessage("Description must not exceed 5000 characters.");
        }
    }

    extension<T>(IRuleBuilder<T, DateTime> ruleBuilder)
    {
        public void ValidateProjectStartDate()
        {
            ruleBuilder
                .NotEmpty()
                .GreaterThanOrEqualTo(DateTime.Now)
                .WithMessage("Start date is required and must be in the future.");
        }
    }

    extension<T>(IRuleBuilder<T, DateTime?> ruleBuilder)
    {
        public void ValidateProjectEndDate(Func<T, DateTime> startDateSelector)
        {
            ruleBuilder
                .GreaterThan(x => startDateSelector(x))
                .WithMessage("End date must be after start date.");
        }
    }

    extension<T>(IRuleBuilder<T, ProjectStatus> ruleBuilder)
    {
        public void ValidateProjectStatus()
        {
            ruleBuilder
                .IsInEnum()
                .WithMessage("Status is required and must be a valid value.");
        }
    }

    extension<T>(IRuleBuilder<T, Priority> ruleBuilder)
    {
        public void ValidateProjectPriority()
        {
            ruleBuilder
                .IsInEnum()
                .WithMessage("Priority is required and must be a valid value.");
        }
    }
}
