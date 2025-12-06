using Rise.Shared.MaintObjects;

namespace Rise.Shared.WorkOrders;

/// <summary>
///     Contains data transfer objects (DTOs) used for work-order-related operations.
/// </summary>
public static class WorkOrderDto {
	/// <summary>
	///     Represents a work order index containing minimalistic work order details.
	/// </summary>
	public class Index {
		public required Guid Id { get; init; }
		public required Priority Priority { get; init; }
		public required string Name { get; init; }
		public required string MaintObj { get; init; }

		public required MaintObjType MaintObjType { get; init; }
		public required string? Technician { get; init; }
		public required WorkOrderStatus Status { get; init; }
		public required DateOnly? StartDate { get; init; }
		public required DateOnly? EndDate { get; init; }
	}

	public class Detail {
		public required Guid Id { get; init; }
		public required string Name { get; init; }
		public required string Description { get; init; }
		public required Priority Priority { get; init; }
		public required WorkOrderStatus Status { get; init; }
		public required DateOnly? StartDate { get; init; }
		public required DateOnly? EndDate { get; init; }
		public required string? Technician { get; init; }

		public required MaintObjDto.Index MaintObj { get; init; }
	}
}