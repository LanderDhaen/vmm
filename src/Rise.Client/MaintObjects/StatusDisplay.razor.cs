using Microsoft.AspNetCore.Components;

namespace Rise.Client.MaintObjects
{
	public partial class StatusDisplay
	{
		[Parameter]
		public string Value { get; set; }
	}
}