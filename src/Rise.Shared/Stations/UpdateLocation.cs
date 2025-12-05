using FluentValidation.Results;

using Rise.Shared.Locations;

namespace Rise.Shared.Stations;

public static partial class MeasurementStationRequest {
	public class UpdateLocation {
		public Guid MeasurementStationId { get; set; }
		public Guid? LocationId { get; set; }
		public bool IsNewLocation { get; set; }

		// In case of a new Location
		public LocationRequest.Create? Location { get; set; } = new();

		public class Validator : AbstractValidator<UpdateLocation> {
			public Validator() {
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
					await ValidateAsync(ValidationContext<UpdateLocation>.CreateWithOptions((UpdateLocation)model,
						x => x.IncludeProperties(propertyName)));
				if (result.IsValid) {
					return Array.Empty<string>();
				}

				return result.Errors.Select(e => e.ErrorMessage);
			};
		}
	}
}