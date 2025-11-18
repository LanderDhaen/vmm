using Microsoft.AspNetCore.Components;
using Rise.Shared.Projects;

namespace Rise.Client.Components;

public partial class ProjectTimeline
{
	[Parameter] public required ProjectDto.History Project { get; init; }
}