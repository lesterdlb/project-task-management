namespace ProjectManagement.Api.Domain;

public class ProjectTaskLabel
{
    public Guid ProjectTaskId { get; set; }
    public Guid LabelId { get; set; }

    public ProjectTask ProjectTask { get; set; } = null!;
    public Label Label { get; set; } = null!;
}