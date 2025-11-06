using System.Dynamic;
using ProjectManagement.Api.Common.Models;

namespace ProjectManagement.Api.Common.Services.DataShaping;

public interface IDataShapingService
{
    ExpandoObject ShapeData<T>(T entity, string? fields);

    List<ExpandoObject> ShapeCollectionData<T>(
        IEnumerable<T> entities,
        string? fields,
        Func<T, List<LinkDto>>? linksFactory = null);

    bool Validate<T>(string? fields);
}
