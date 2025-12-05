using Microsoft.AspNetCore.Components;

namespace Rise.Client.Components;

public partial class TimeLineItem {
	[Parameter] public required string Title { get; init; }
	[Parameter] public required DateTime ModifiedAt { get; init; }
	[Parameter] public required string PreviousValue { get; init; }
	[Parameter] public required string CurrentValue { get; init; }
}