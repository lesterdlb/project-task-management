namespace ProjectManagement.Api.Core.Application.Models;

public interface ICollectionResponse<T>
{
    List<T> Items { get; init; }
}
