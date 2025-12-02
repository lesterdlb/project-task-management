using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;
using System.Text.Json;
using ProjectManagement.Api.Core.Application.Models;

namespace ProjectManagement.Api.Core.Application.Services.DataShaping;

public sealed class DataShapingService : IDataShapingService
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertiesCache = new();

    public ExpandoObject ShapeData<T>(T entity, string? fields)
    {
        var fieldsSet = fields?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        var propertyInfos = PropertiesCache.GetOrAdd(
            typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        if (fieldsSet.Any())
        {
            propertyInfos = [.. propertyInfos.Where(p => fieldsSet.Contains(p.Name))];
        }

        IDictionary<string, object?> shapedObject = new ExpandoObject();

        foreach (var propertyInfo in propertyInfos)
        {
            var camelCaseName = JsonNamingPolicy.CamelCase.ConvertName(propertyInfo.Name);
            shapedObject[camelCaseName] = propertyInfo.GetValue(entity);
        }

        return (ExpandoObject)shapedObject;
    }

    public List<ExpandoObject> ShapeCollectionData<T>(
        IEnumerable<T> entities,
        string? fields,
        Func<T, List<LinkDto>>? linksFactory = null)
    {
        var fieldsSet = fields?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        var propertyInfos = PropertiesCache.GetOrAdd(
            typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        if (fieldsSet.Any())
        {
            propertyInfos = [.. propertyInfos.Where(p => fieldsSet.Contains(p.Name))];
        }

        List<ExpandoObject> shapedObjects = [];
        foreach (var entity in entities)
        {
            IDictionary<string, object?> shapedObject = new ExpandoObject();

            foreach (var propertyInfo in propertyInfos)
            {
                var camelCaseName = JsonNamingPolicy.CamelCase.ConvertName(propertyInfo.Name);
                shapedObject[camelCaseName] = propertyInfo.GetValue(entity);
            }

            if (linksFactory is not null)
            {
                shapedObject["links"] = linksFactory(entity);
            }

            shapedObjects.Add((ExpandoObject)shapedObject);
        }

        return shapedObjects;
    }

    public bool Validate<T>(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return true;
        }

        var fieldsSet = fields
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var propertyInfos = PropertiesCache.GetOrAdd(
            typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        return fieldsSet.All(f => propertyInfos.Any(p => p.Name.Equals(f, StringComparison.OrdinalIgnoreCase)));
    }
}
