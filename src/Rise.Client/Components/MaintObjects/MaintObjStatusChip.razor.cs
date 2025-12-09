using Microsoft.AspNetCore.Components;

using Rise.Shared.MaintObjects;

namespace Rise.Client.Components.MaintObjects;

public partial class MaintObjStatusChip {
	[Parameter] public required MaintObjStatus Status { get; init; }
}