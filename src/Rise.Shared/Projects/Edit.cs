using FluentValidation.Results;

namespace Rise.Shared.Projects;

public static partial class ProjectRequest {
	public class Edit {
		/// <summary>
		///     The unique identifier of the project.
		/// </summary>
		public required Guid ProjectId { get; set; }

		/// <summary>
		///     The name of the project.
		/// </summary>
		public required string Name { get; set; }

		public required string Code { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public Guid? MeasurementNetworkId { get; set; }

		public class Validator : AbstractValidator<Edit> {
			public Validator() {
				RuleFor(x => x.Name)
					.NotEmpty().WithMessage("Naam mag niet leeg zijn.")
					.MaximumLength(250).WithMessage("Naam mag maar max. 250 karakter hebben.");
				RuleFor(x => x.Code)
					.NotEmpty().WithMessage("Code mag niet leeg zijn.")
					.MaximumLength(50).WithMessage("Code mag maar max. 50 karakter hebben.");
				RuleFor(x => x.StartDate)
					.NotNull().WithMessage("Startdatum mag niet leeg zijn!")
					.GreaterThan(new DateTime(2000, 1, 1)).WithMessage("Startdatum moet boven 1 januari 2000 liggen.")
					.LessThan(DateTime.MaxValue).WithMessage("Startdatum moet onder het maximum zijn.");
				RuleFor(x => x.EndDate)
					.Must((model, endDate) => !endDate.HasValue || endDate.Value > model.StartDate)
					.WithMessage("Einddatum moet na de startdatum liggen.");
				RuleFor(x => x.MeasurementNetworkId)
					.NotNull().WithMessage("Je moet een meetnet selecteren.");
			}

			public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) => {
				ValidationResult? result =
					await ValidateAsync(ValidationContext<Edit>.CreateWithOptions((Edit)model,
						x => x.IncludeProperties(propertyName)));
				if (result.IsValid) {
					return Array.Empty<string>();
				}

				return result.Errors.Select(e => e.ErrorMessage);
			};
		}
	}
}