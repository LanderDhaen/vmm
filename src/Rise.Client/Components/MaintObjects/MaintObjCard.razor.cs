using Microsoft.AspNetCore.Components;

using Rise.Shared.MaintObjects;

namespace Rise.Client.Components.MaintObjects;

public partial class MaintObjCard {
	[Parameter] public required MaintObjDto.Index MaintObj { get; init; }

	[Inject] public required NavigationManager NavigationManager { get; init; }

	private void NavigateToMaintObj(Guid id) {
		NavigationManager.NavigateTo($"maintobjects/{id}");
	}
}