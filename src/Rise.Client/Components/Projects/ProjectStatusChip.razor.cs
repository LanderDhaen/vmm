using Rise.Shared.Projects;
using Microsoft.AspNetCore.Components;

namespace Rise.Client.Components.Projects;

public partial class ProjectStatusChip {
	[Parameter] public required Status Status { get; init; }
}