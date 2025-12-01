using Microsoft.AspNetCore.Components;

namespace Rise.Client.Components;

public partial class SearchField {
	[Parameter] public string? Value { get; set; }
	[Parameter] public EventCallback<string> ValueChanged { get; set; }
	[Parameter] public string Label { get; set; } = "Zoek";

	private async Task HandleIntervalElapsed() =>
		await ValueChanged.InvokeAsync(Value);
}