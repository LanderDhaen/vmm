using Microsoft.AspNetCore.Components;

using Rise.Shared.WorkOrders;

namespace Rise.Client.Components.WorkOrders;

public partial class WorkOrderPriorityChip {
	[Parameter] public required Priority Priority { get; init; }
}