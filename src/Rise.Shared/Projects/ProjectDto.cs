namespace Rise.Shared.Projects;

/// <summary>
/// Contains data transfer objects (DTOs) used for project-related operations.
/// </summary>
public static class ProjectDto
{
    /// <summary>
    /// Represents a project index containing minimalistic project details.
    /// </summary>
    public class Index
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        
        public required string Code { get; set; }
        
        public required DateTime StartDate { get; set; }
        
        public required DateTime EndDate { get; set; }
    }
}