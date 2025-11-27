using FluentValidation.Results;

namespace Rise.Shared.MaintObjects;

/// <summary>
///     Represents a static utility class containing request-related structures for maintenance objects.
/// </summary>
public static partial class MaintObjRequest {
	public class Create {
		public string Name { get; set; }
		public string SerialNumber { get; set; }
		public MaintObjType MaintObjType { get; set; }
		public string? Brand { get; set; }
		public Guid? LocationId { get; set; }

		public class Validator : AbstractValidator<Create> {
			public Validator() {
				RuleFor(x => x.Name)
					.NotEmpty().WithMessage("Naam mag niet leeg zijn.")
					.MaximumLength(250).WithMessage("Naam mag maar max. 250 karakter hebben.");
				RuleFor(x => x.SerialNumber)
					.NotEmpty().WithMessage("Serienummer mag niet leeg zijn.");
				RuleFor(x => x.MaintObjType)
					.NotNull().WithMessage("Je moet een type selecteren.");
				RuleFor(x => x.LocationId)
					.NotNull().WithMessage("Je moet een stock locatie selecteren.");
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

public static partial class MaintObjResponse {
	public class Create {
		public Guid MaintObjId { get; set; }
	}
}