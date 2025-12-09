using FluentValidation.Results;

using Rise.Shared.Locations;

namespace Rise.Shared.Stations;

public static partial class MeasurementStationRequest {
	public class Create {
		public string Code { get; set; } = default!;
		public Guid? LocationId { get; set; }
		public bool IsNewLocation { get; set; }

		// In case of a new Location
		public LocationRequest.Create? Location { get; set; } = new();

		// Installation
		public DateTime? InstalledAt { get; set; }

		public class Validator : AbstractValidator<Create> {
			public Validator() {
				RuleFor(x => x.Code)
					.NotEmpty().WithMessage("Code mag niet leeg zijn.")
					.MaximumLength(50).WithMessage("Code maar max. 50 karakter hebben.");
				RuleFor(x => x)
					.Must(x => x.LocationId.HasValue || x.IsNewLocation)
					.WithMessage("Je moet een bestaande locatie selecteren of een nieuwe locatie invullen.")
					.OverridePropertyName("LocationId");
				RuleFor(x => x.Location)
					.SetValidator(new LocationRequest.Validator()!)
					.When(x => x.IsNewLocation);
			}

			public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) => {
				ValidationResult? result =
					await ValidateAsync(ValidationContext<Create>.CreateWithOptions((Create)model,
						x => x.IncludeProperties(propertyName)));
				if (result.IsValid) {
					return Array.Empty<string>();
				}

				return result.Errors.Select(e => e.ErrorMessage);
			};
		}
	}
}

public static partial class MeasurementStationResponse {
	public class Create {
		public Guid MeasurementStationId { get; set; }
	}
}