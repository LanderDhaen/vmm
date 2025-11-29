using Microsoft.AspNetCore.Components;

namespace Rise.Client.Components;

public partial class SearchField {
	[Parameter] public string? Value { get; set; }
	[Parameter] public EventCallback<string?> ValueChanged { get; set; }
	[Parameter] public EventCallback<string?> OnSearch { get; set; }
	[Parameter] public string Label { get; set; } = "Zoek";
	[Parameter] public int Debounce { get; set; } = 300;

	private async Task OnSearchInternal() {
		await ValueChanged.InvokeAsync(Value);
		await OnSearch.InvokeAsync(Value);
	}
}