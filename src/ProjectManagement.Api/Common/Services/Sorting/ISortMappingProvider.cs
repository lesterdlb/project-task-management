namespace ProjectManagement.Api.Common.Services.Sorting;

public interface ISortMappingProvider
{
    SortMapping[] GetMappings<TSource, TDestination>();
    bool ValidateMappings<TSource, TDestination>(string? sort);
}
