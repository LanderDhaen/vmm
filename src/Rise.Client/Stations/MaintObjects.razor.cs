using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.MaintObjects;

namespace Rise.Client.Stations;

public partial class MaintObjects {
	[Parameter] public Guid StationId { get; init; }
	[Parameter] public IEnumerable<MaintObjDto.Index>? MaintObjectsFromStation { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; init; }

	private void NavigateToMaintObject(Guid maintObjId) {
		NavigationManager.NavigateTo($"/maintobjects/{maintObjId}");
	}

	private Color GetStatusColor(MaintObjStatus status) {
		return status switch {
			MaintObjStatus.Active => Color.Success,
			MaintObjStatus.Inactive => Color.Error,
			MaintObjStatus.Broken => Color.Warning,
			_ => Color.Default
		};
	}
}