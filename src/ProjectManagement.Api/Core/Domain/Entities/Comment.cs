namespace ProjectManagement.Api.Core.Domain.Entities;

public sealed class Comment : Entity
{
    public required string Content { get; init; }

    public Guid AuthorId { get; init; }
    
    public required User Author { get; init; }
    public Guid ProjectTaskId { get; init; }
    public required ProjectTask ProjectTask { get; init; }
}