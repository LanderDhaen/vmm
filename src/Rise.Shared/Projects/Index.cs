namespace Rise.Shared.Projects;

public static class ProjectResponse
{
    public class Index
    {
        public required IEnumerable<ProjectDto.Index> Projects { get; set; }
        public required int TotalCount  { get; set; }
    }
}
