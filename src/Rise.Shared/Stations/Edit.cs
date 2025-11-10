using FluentValidation;

namespace Rise.Shared.Stations;

public static partial class MeasurementStationRequest {
	public class Edit {
		public Guid StationId { get; set; }

		public string Code { get; set; } = default!;
		public string Status { get; set; } = default!;
		public Guid LocationId { get; set; }
		public Guid ProjectId { get; set; }
		public DateTime? InstalledAt { get; set; }
		public DateTime? RemovedAt { get; set; }

		public class Validator : AbstractValidator<Edit> {
			public Validator() {
				RuleFor(x => x.StationId).NotEmpty();
				RuleFor(x => x.Code).NotEmpty().WithMessage("Code mag niet leeg zijn.").MaximumLength(64);
				RuleFor(x => x.Status).NotEmpty();
				RuleFor(x => x.LocationId).NotEmpty();
				RuleFor(x => x.ProjectId).NotEmpty();

				RuleFor(x => x.RemovedAt)
					.Must((m, removed) => removed is null || (m.InstalledAt ?? DateTime.MinValue) <= removed)
					.WithMessage("Einddatum kan niet voor de startdatum liggen.");
			}
		}
	}
}