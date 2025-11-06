using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace Rise.Client.Components;

public partial class Breadcrumbs {

	[Parameter] public required List<BreadcrumbItem> Items { get; set; }
}