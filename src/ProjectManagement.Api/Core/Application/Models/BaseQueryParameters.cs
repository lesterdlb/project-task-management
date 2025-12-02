using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using ProjectManagement.Api.Constants;

namespace ProjectManagement.Api.Core.Application.Models;

public class BaseQueryParameters
{
    public string? Fields { get; init; }

    [FromHeader(Name = "Accept")]
    public string? Accept { get; init; }

    public bool IncludeLinks =>
        MediaTypeHeaderValue.TryParse(Accept, out var mediaType) &&
        mediaType.SubTypeWithoutSuffix.HasValue &&
        mediaType.SubTypeWithoutSuffix.Value.Contains(CustomMediaTypeNames.Application.HateoasSubType);
}
