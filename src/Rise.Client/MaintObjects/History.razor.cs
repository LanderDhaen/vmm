using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.MaintObjects;

namespace Rise.Client.MaintObjects;

public partial class History {
	private IEnumerable<MaintObjDto.Modification>? _modifications;
	[Parameter] public Guid MaintObjId { get; set; }
	[Inject] public required IMaintObjService MaintObjService { get; set; }
	
	protected override async Task OnInitializedAsync() {
		Result<MaintObjResponse.History> result =
			await MaintObjService.GetHistoryAsync(MaintObjId);

		if (result.IsSuccess) {
			_modifications = result.Value.Modifications;
		}
	}
}