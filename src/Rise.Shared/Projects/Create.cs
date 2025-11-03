namespace Rise.Shared.Projects;

/// <summary>
///     Represents a static utility class containing request-related structures for projects.
/// </summary>
public static partial class ProjectRequest {
	public class Create {
		public string Name { get; set; }
		public string Code { get; set; }
		public DateTime? StartDate { get; set; } = DateTime.Now;
		public Guid MeasurementNetworkId { get; set; } = Guid.Empty;

		public class Validator : AbstractValidator<Create> {
			public Validator() {
				RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
				RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
				RuleFor(x => x.StartDate).NotNull().WithMessage("Startdatum mag niet leeg zijn!");
				RuleFor(x => x.MeasurementNetworkId).NotEqual(Guid.Empty)
					.WithMessage("Je moet een meetnet selecteren!");
			}

			public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) => {
				var result =
					await ValidateAsync(ValidationContext<Create>.CreateWithOptions((Create)model,
						x => x.IncludeProperties(propertyName)));
				if (result.IsValid)
					return Array.Empty<string>();
				return result.Errors.Select(e => e.ErrorMessage);
			};
		}
	}
}

public static partial class ProjectResponse {
	public class Create {
		public Guid ProjectId { get; set; }
	}
}