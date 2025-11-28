using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.MaintObjects;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class MaintObjects {
	private IEnumerable<MaintObjDto.Index>? _allMaintObjs;
	private IEnumerable<MaintObjDto.Index>? _maintObjsFromStation;
	private Result? _result;

	private List<Guid> _currentStationMaintObjIds = new();

	[Parameter] public Guid StationId { get; set; }
	[Parameter] public EventCallback<string> TabChanged { get; set; }

	private MeasurementStationRequest.UpdateMaintObjects Model { get; } = new();

	[Inject] private ISnackbar Snackbar { get; set; } = default!;
	[Inject] public required IMaintObjService MaintObjService { get; set; }
	[Inject] public required IMeasurementStationService StationService { get; set; }

	protected override async Task OnInitializedAsync() {
		var maintObjsFromStation = await StationService.GetMaintObjsAsync(StationId);
		var allMaintObjs = await MaintObjService.GetIndexAsync(new QueryRequest.SkipTake());

		if (maintObjsFromStation.IsSuccess && maintObjsFromStation.Value is not null) {
			_maintObjsFromStation = maintObjsFromStation.Value;
		} else {
			_maintObjsFromStation = new List<MaintObjDto.Index>();

			if (!maintObjsFromStation.IsSuccess) {
				string message = maintObjsFromStation.Errors != null
					? string.Join(Environment.NewLine, maintObjsFromStation.Errors)
					: "Kon apparaten niet laden.";
				Snackbar.Add(message, Severity.Warning);
			}
		}

		_currentStationMaintObjIds = _maintObjsFromStation.Select(m => m.Id).ToList();

		if (allMaintObjs.IsSuccess && allMaintObjs.Value is not null) {
			_allMaintObjs = allMaintObjs.Value.MaintObjects;
		} else {
			_allMaintObjs = new List<MaintObjDto.Index>();

			if (!allMaintObjs.IsSuccess) {
				string message = allMaintObjs.Errors != null
					? string.Join(Environment.NewLine, allMaintObjs.Errors)
					: "Kon apparaten niet laden.";
				Snackbar.Add(message, Severity.Warning);
			}
		}

		var allList = (_allMaintObjs ?? Enumerable.Empty<MaintObjDto.Index>()).ToList();
		var stationList = (_maintObjsFromStation ?? Enumerable.Empty<MaintObjDto.Index>()).ToList();

		_allMaintObjs = allList
			.Concat(stationList)
			.GroupBy(m => m.Id)
			.Select(g => g.First())
			.OrderBy(m => m.Name)
			.ToList();

		Model.MaintObjIds = _currentStationMaintObjIds.ToList();
		Model.MeasurementStationId = StationId;

	}

	protected string GetSelectedMaintObjText(List<string?>? selectedValues) {
		if (_allMaintObjs is null || selectedValues is null || selectedValues.Count == 0)
			return string.Empty;

		var ids = selectedValues
			.Select(v => Guid.TryParse(v, out var g) ? g : (Guid?)null)
			.Where(g => g.HasValue)
			.Select(g => g!.Value);

		var names = ids
			.Select(id => _allMaintObjs.FirstOrDefault(x => x.Id == id)?.Name)
			.Where(n => !string.IsNullOrWhiteSpace(n));

		return string.Join(", ", names);
	}

	protected Color GetStatusColor(MaintObjStatus status) {
		return status switch {
			MaintObjStatus.Actief => Color.Success,
			MaintObjStatus.Inactief => Color.Error,
			MaintObjStatus.Defect => Color.Warning,
			_ => Color.Default
		};
	}

	private async Task UpdateMaintObjs() {
		var newIds = Model.MaintObjIds.Distinct().ToList();
		var oldIds = _currentStationMaintObjIds;

		bool selectionChanged = !newIds.SequenceEqual(oldIds);

		if (!selectionChanged) {
			Snackbar.Add("Geen wijzigingen om op te slaan.", Severity.Info);
			return;
		}

		Model.MaintObjIds = newIds;

		_result = await StationService.UpdateMaintObjsAsync(Model);

		if (_result.IsSuccess) {
			Snackbar.Add("Apparaten succesvol bijgewerkt!", Severity.Success);

			var refreshed = await StationService.GetMaintObjsAsync(StationId);
			if (refreshed.IsSuccess && refreshed.Value is not null) {
				_maintObjsFromStation = refreshed.Value;
				_currentStationMaintObjIds = _maintObjsFromStation.Select(m => m.Id).ToList();

				Model.MaintObjIds = _currentStationMaintObjIds.ToList();

				await TabChanged.InvokeAsync($"Apparaten ({_maintObjsFromStation.Count()})");
			}
		} else {
			Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
		}
	}
}