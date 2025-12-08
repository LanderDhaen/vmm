using FluentValidation.Results;

using Rise.Shared.Locations;

namespace Rise.Shared.Stations;

public static partial class MeasurementStationRequest {
	public class Edit {
		public Guid StationId { get; set; }
		public string Code { get; set; } = default!;

		// Installation
		public DateTime? InstalledAt { get; set; }
		public DateTime? RemovedAt { get; set; }

		public class Validator : AbstractValidator<Edit> {
			public Validator() {
				RuleFor(x => x.Code)
					.NotEmpty().WithMessage("Code mag niet leeg zijn.")
					.MaximumLength(50).WithMessage("Code maar max. 50 karakter hebben.");
				RuleFor(x => x.InstalledAt)
					.NotNull().WithMessage("Installatiedatum mag niet leeg zijn!")
					.GreaterThan(new DateTime(2000, 1, 1)).WithMessage("Installatiedatum moet boven 1 januari 2000 liggen.")
					.LessThan(DateTime.MaxValue).WithMessage("Installatiedatum moet onder het maximum zijn.");
				RuleFor(x => x.RemovedAt)
					.Must((m, removed) => removed is null || (m.InstalledAt ?? DateTime.MinValue) <= removed)
					.WithMessage("Einddatum kan niet voor de startdatum liggen.");
			}

			public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) => {
				ValidationResult? result =
					await ValidateAsync(ValidationContext<Edit>.CreateWithOptions((Edit)model,
						x => x.IncludeProperties(propertyName)));
				return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
			};
		}
	}
}