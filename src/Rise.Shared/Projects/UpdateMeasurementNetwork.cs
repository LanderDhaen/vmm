using FluentValidation.Results;

namespace Rise.Shared.Projects;

public static partial class ProjectRequest {
	public class UpdateMeasurementNetwork {
		/// <summary>
		///     The unique identifier of the project.
		/// </summary>
		public required Guid ProjectId { get; set; }
		public Guid? MeasurementNetworkId { get; set; }

		public class Validator : AbstractValidator<UpdateMeasurementNetwork> {
			public Validator() {
				RuleFor(x => x.MeasurementNetworkId)
					.NotNull().WithMessage("Je moet een meetnet selecteren.");
			}

			public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) => {
				ValidationResult? result =
					await ValidateAsync(ValidationContext<UpdateMeasurementNetwork>.CreateWithOptions((UpdateMeasurementNetwork)model,
						x => x.IncludeProperties(propertyName)));
				if (result.IsValid) {
					return Array.Empty<string>();
				}

				return result.Errors.Select(e => e.ErrorMessage);
			};
		}
	}
}