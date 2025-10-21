using ProjectManagement.Api.Domain.Entities;

namespace ProjectManagement.Api.Domain;

public class Comment : Entity
{
    public Guid ProjectTaskId { get; set; }
    public Guid AuthorId { get; set; }
    public required string Content { get; set; }

    public required ProjectTask ProjectTask { get; set; }
    public required User Author { get; set; }
}