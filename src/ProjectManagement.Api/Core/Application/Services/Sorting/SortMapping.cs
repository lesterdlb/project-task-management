namespace ProjectManagement.Api.Core.Application.Services.Sorting;

public sealed record SortMapping(string SortField, string PropertyName, bool Reverse = false);
