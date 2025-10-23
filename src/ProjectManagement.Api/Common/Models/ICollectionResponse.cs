namespace ProjectManagement.Api.Common.Models;

public interface ICollectionResponse<T>
{
    List<T> Items { get; init; }
}
