namespace ProjectManagement.Api.Common.Domain.Entities;

public sealed class Label : Entity
{
    public required string Name { get; init; }
    public string Color { get; init; } = string.Empty;

    public Guid ProjectId { get; init; }
    public required Project Project { get; init; }
}