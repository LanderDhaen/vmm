using Microsoft.AspNetCore.Components;

using Rise.Shared.WorkOrders;

namespace Rise.Client.Components.WorkOrders;

public partial class WorkOrderStatusChip {
	[Parameter] public required WorkOrderStatus Status { get; init; }
}