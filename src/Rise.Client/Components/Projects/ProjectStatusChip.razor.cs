using Microsoft.AspNetCore.Components;

using Rise.Shared.Projects;

namespace Rise.Client.Components.Projects;

public partial class ProjectStatusChip {
	[Parameter] public required Status Status { get; init; }
}