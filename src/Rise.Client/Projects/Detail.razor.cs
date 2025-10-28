using LeafletForBlazor;

using Microsoft.AspNetCore.Components;

using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Detail {
	[Parameter] public Guid ProjectId { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }

	private ProjectDto.Detail? _project;
	private string? _remarks;
	private RealTimeMap.LoadParameters? _parameters;

	protected override async Task OnInitializedAsync() {
		var result = await ProjectService.GetDetailAsync(ProjectId);

		if (result.IsSuccess) {
			_project = result.Value.Project;
			_parameters = new RealTimeMap.LoadParameters {
				location = new RealTimeMap.Location {
					latitude = _project.Location.Coordinate.Y,
					longitude = _project.Location.Coordinate.X
				},
				zoomLevel = 18,

				// 👇 meerdere basemap-types instellen
				basemap = new RealTimeMap.Basemap {
					basemapLayers = new List<RealTimeMap.BasemapLayer> {
						new RealTimeMap.BasemapLayer {
							url = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
							attribution = "©OpenStreetMap",
							title = "OpenStreetMap",
							detectRetina = true
						},

						new RealTimeMap.BasemapLayer {
							url =
								"http://services.arcgisonline.com/arcgis/rest/services/World_Topo_Map/MapServer/tile/{z}/{y}/{x}",
							attribution = "©World Topo Map",
							title = "World Topo Map",
							detectRetina = true
						},
						new RealTimeMap.BasemapLayer {
							url =
								"http://services.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer/tile/{z}/{y}/{x}",
							attribution = "©World Street Map",
							title = "World Street Map",
							detectRetina = true
						},
						new RealTimeMap.BasemapLayer {
							url =
								"http://services.arcgisonline.com/arcgis/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}",
							attribution = "©World Imagery",
							title = "World Imagery",
							detectRetina = true
						},
					}
				}
			};
		}
	}

	private void NavigateToHistory() {
		Navigation.NavigateTo("/projects/history");
	}
}