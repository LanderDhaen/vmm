using Rise.Shared.WorkOrders;
using Microsoft.AspNetCore.Components;

namespace Rise.Client.Components.WorkOrders;

public partial class WorkOrderPriorityChip {
	[Parameter] public required Priority Priority { get; init; }
}