namespace Rise.Shared.Projects;

public static partial class ProjectRequest {
	public class UpdateStatus {
		public Guid ProjectId { get; set; }
		public Status NewStatus { get; set; }
	}
}

public enum Status {
	Actief,
	Afgesloten,
	Gepauzeerd
}