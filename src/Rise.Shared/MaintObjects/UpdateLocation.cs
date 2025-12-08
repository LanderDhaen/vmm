using FluentValidation.Results;

namespace Rise.Shared.MaintObjects;

public static partial class MaintObjRequest {
	public class UpdateLocation {
		public required Guid MaintObjId { get; set; }
		public Guid? LocationId { get; set; }

		public class Validator : AbstractValidator<UpdateLocation> {
			public Validator() {
				RuleFor(x => x.LocationId)
					.NotNull().WithMessage("Je moet een locatie selecteren.");
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