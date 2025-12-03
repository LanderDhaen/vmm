using Rise.Shared.Projects;

namespace Rise.Server.Endpoints.Projects {
	public class SelectOptions(IProjectService projectService) : EndpointWithoutRequest<Result<ProjectResponse.SelectOptions>> {
		public override void Configure() {
			Get("/api/projects/select-options");
		}

		public override Task<Result<ProjectResponse.SelectOptions>> ExecuteAsync(CancellationToken ct)
			=> projectService.GetSelectOptionsAsync(ct);
	}
}