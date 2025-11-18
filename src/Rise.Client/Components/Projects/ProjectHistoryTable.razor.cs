using Microsoft.AspNetCore.Components;
using Rise.Shared.Projects;

namespace Rise.Client.Components.Projects;

public partial class ProjectHistoryTable
{
	[Parameter] public required ProjectDto.History Project { get; init; }
}