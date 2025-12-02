using FluentValidation;
using ProjectManagement.Api.Core.Application.Services.DataShaping;
using ProjectManagement.Api.Core.Application.Services.Sorting;

namespace ProjectManagement.Api.Core.Application.Validation.Common;

public static class QueryParametersValidationRules
{
    extension<T>(IRuleBuilder<T, int?> ruleBuilder)
    {
        public void ValidatePage()
        {
            ruleBuilder
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0.");
        }

        public void ValidatePageSize()
        {
            ruleBuilder
                .InclusiveBetween(1, 100)
                .WithMessage("PageSize must be between 1 and 100.");
        }
    }

    extension<T>(IRuleBuilder<T, string?> ruleBuilder)
    {
        public void ValidateSort<TDto, TEntity>(ISortMappingProvider sortMappingProvider)
        {
            ruleBuilder.Custom((sort, context) =>
            {
                if (!sortMappingProvider.ValidateMappings<TDto, TEntity>(sort))
                {
                    context.AddFailure($"The provided sort parameter isn't valid: '{sort}'");
                }
            });
        }

        public void ValidateFields<TDto>(IDataShapingService dataShapingService)
        {
            ruleBuilder.Custom((fields, context) =>
            {
                if (!dataShapingService.Validate<TDto>(fields))
                {
                    context.AddFailure($"The provided data shaping fields aren't valid: '{fields}'");
                }
            });
        }
    }
}
