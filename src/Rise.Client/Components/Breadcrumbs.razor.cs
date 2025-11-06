using MudBlazor;
using Microsoft.AspNetCore.Components;

namespace Rise.Client.Components;

public partial class Breadcrumbs {
	
	[Parameter] public required List<BreadcrumbItem> Items  { get; set; }
}