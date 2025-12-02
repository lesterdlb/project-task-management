using System.Dynamic;
using ProjectManagement.Api.Core.Application.Models;

namespace ProjectManagement.Api.Core.Application.Services.DataShaping;

public interface IDataShapingService
{
    ExpandoObject ShapeData<T>(T entity, string? fields);

    List<ExpandoObject> ShapeCollectionData<T>(
        IEnumerable<T> entities,
        string? fields,
        Func<T, List<LinkDto>>? linksFactory = null);

    bool Validate<T>(string? fields);
}
