namespace Rise.Shared.Projects;

public static class ProjectResponse
{
    public class Index
    {
        public required IEnumerable<ProjectDto.Index> Projects { get; set; }
        public required int TotalCount  { get; set; }
    }
}

public static class ProjectDto
{
    public class Index
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}