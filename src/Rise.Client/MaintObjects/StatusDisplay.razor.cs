using Microsoft.AspNetCore.Components;

namespace Rise.Client.MaintObjects
{
	public partial class StatusDisplay
	{
		[Parameter]
		public required string Value { get; init; }
	}
}