using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.MaintObjects;

namespace Rise.Client.MaintObjects;

public partial class History {
	private IEnumerable<MaintObjDto.Modification>? _modifications;
	[Parameter] public Guid MaintObjId { get; set; }
	[Inject] public required IMaintObjService MaintObjService { get; set; }

	private TimelinePosition _timelinePosition;


	protected override async Task OnInitializedAsync() {
		Result<MaintObjResponse.History> result =
			await MaintObjService.GetHistoryAsync(MaintObjId);

		if (result.IsSuccess) {
			_modifications = result.Value.Modifications;
		}
	}

	private void OnBreakPointChange(Breakpoint breakpoint) => _timelinePosition = breakpoint == Breakpoint.Xs ? TimelinePosition.Left : TimelinePosition.Alternate;


	private static string DefineTitle(MaintObjModificationType modificationType) {
		return modificationType switch {
			MaintObjModificationType.StatusChanged => "Status gewijzigd",
			MaintObjModificationType.MovedToStock => "Verplaatst naar stock",
			MaintObjModificationType.AssignedToStation => "Geplaatst in meetstation",
			MaintObjModificationType.Scanned => "Gescand",
			_ => "Wijziging"
		};
	}

}