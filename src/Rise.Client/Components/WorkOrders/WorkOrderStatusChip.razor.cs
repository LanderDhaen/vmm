using Rise.Shared.WorkOrders;
using Microsoft.AspNetCore.Components;

namespace Rise.Client.Components.WorkOrders;

public partial class WorkOrderStatusChip {
	[Parameter] public required WorkOrderStatus Status { get; init; }
}