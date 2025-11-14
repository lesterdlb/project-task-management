using FluentValidation;
using ProjectManagement.Api.Common.Services.DataShaping;
using ProjectManagement.Api.Common.Services.Sorting;

namespace ProjectManagement.Api.Common.Validators;

public static class QueryParametersValidationRules
{
    public static void ValidatePage<T>(this IRuleBuilder<T, int?> ruleBuilder)
    {
        ruleBuilder
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");
    }

    public static void ValidatePageSize<T>(this IRuleBuilder<T, int?> ruleBuilder)
    {
        ruleBuilder
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");
    }

    public static void ValidateSort<T, TDto, TEntity>(this IRuleBuilder<T, string?> ruleBuilder,
        ISortMappingProvider sortMappingProvider)
    {
        ruleBuilder
            .Custom((sort, context) =>
            {
                if (!sortMappingProvider.ValidateMappings<TDto, TEntity>(sort))
                {
                    context.AddFailure($"The provided sort parameter isn't valid: '{sort}'");
                }
            });
    }

    public static void ValidateFields<T, TDto>(this IRuleBuilder<T, string?> ruleBuilder,
        IDataShapingService dataShapingService)
    {
        ruleBuilder
            .Custom((fields, context) =>
            {
                if (!dataShapingService.Validate<TDto>(fields))
                {
                    context.AddFailure($"The provided data shaping fields aren't valid: '{fields}'");
                }
            });
    }
}
