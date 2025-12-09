using Rise.Shared.MaintObjects;
using Microsoft.AspNetCore.Components;

namespace Rise.Client.Components.MaintObjects;

public partial class MaintObjStatusChip {
	[Parameter] public required MaintObjStatus Status { get; init; }
}