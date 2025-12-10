using Microsoft.AspNetCore.Components;

namespace Rise.Client.Projects
{
	public partial class StatusDisplay
	{
		[Parameter]
		public required string Value { get; init; }
	}
}