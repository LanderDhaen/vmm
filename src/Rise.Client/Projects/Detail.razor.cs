using LeafletForBlazor;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Detail {
	[Parameter] public Guid ProjectId { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }

	private ProjectDto.Detail? _project;
	private string? _remarks;
	// private RealTimeMap.LoadParameters? _parameters;

	protected override async Task OnInitializedAsync() {
		var result = await ProjectService.GetDetailAsync(ProjectId);

		if (result.IsSuccess) {
			_project = result.Value.Project;

			_items.Clear();
			_items.Add(new BreadcrumbItem("Projecten", href: "/projects"));
			_items.Add(new BreadcrumbItem(_project?.Code ?? "Detail", href: null, disabled: true));
		}
	}

	//NAV
	private void NavigateToProjects() {
		Navigation.NavigateTo($"/projects/");
	}

	//BREADCRUMB
	private readonly List<BreadcrumbItem> _items = new();
}