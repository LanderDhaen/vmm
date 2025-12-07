using FluentValidation.Results;

namespace Rise.Shared.Projects;

/// <summary>
///     Represents a static utility class containing request-related structures for projects.
/// </summary>
public static partial class ProjectRequest {
	public class Create {
		public string Name { get; set; }
		public string Code { get; set; }
		public DateTime? StartDate { get; set; } = DateTime.UtcNow;
		public Guid? MeasurementNetworkId { get; set; }

		public class Validator : AbstractValidator<Create> {
			public Validator() {
				RuleFor(x => x.Name)
					.NotEmpty().WithMessage("Naam mag niet leeg zijn.")
					.MaximumLength(250).WithMessage("Naam mag maar max. 250 karakter hebben.");
				RuleFor(x => x.Code)
					.NotEmpty().WithMessage("Code mag niet leeg zijn.")
					.MaximumLength(50).WithMessage("Code maar max. 50 karakter hebben.");
				RuleFor(x => x.StartDate)
					.NotNull().WithMessage("Startdatum mag niet leeg zijn!")
					.GreaterThan(new DateTime(2000, 1, 1)).WithMessage("Startdatum moet boven 1 januari 2000 liggen.")
					.LessThan(DateTime.MaxValue).WithMessage("Startdatum moet onder het maximum zijn.");
				RuleFor(x => x.MeasurementNetworkId)
					.NotNull().WithMessage("Je moet een meetnet selecteren.");
			}

			public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) => {
				ValidationResult? result =
					await ValidateAsync(ValidationContext<Create>.CreateWithOptions((Create)model,
						x => x.IncludeProperties(propertyName)));
				return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
			};
		}
	}
}

public static partial class ProjectResponse {
	public class Create {
		public Guid ProjectId { get; set; }
	}
}