namespace ProjectManagement.Api.Domain.Entities;

public abstract class Entity
{
    public Guid Id { get; init; }
    public Guid CreatedBy { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}