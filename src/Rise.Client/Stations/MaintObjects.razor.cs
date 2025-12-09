using Microsoft.AspNetCore.Components;

using Rise.Shared.MaintObjects;

namespace Rise.Client.Stations;

public partial class MaintObjects {
	[Parameter] public Guid StationId { get; init; }
	[Parameter] public IEnumerable<MaintObjDto.Index>? MaintObjectsFromStation { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; init; }

	private void NavigateToMaintObject(Guid maintObjId) {
		NavigationManager.NavigateTo($"/maintobjects/{maintObjId}");
	}
}