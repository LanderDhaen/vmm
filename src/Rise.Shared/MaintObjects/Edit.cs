using FluentValidation.Results;

namespace Rise.Shared.MaintObjects;

/// <summary>
///     Represents a static utility class containing request-related structures for maintenance objects.
/// </summary>
public static partial class MaintObjRequest {
	public class Edit {
		public required Guid MaintObjId { get; set; }
		public required string Name { get; set; }
		public required string SerialNumber { get; set; }
		public string? Brand { get; set; }
		public DateTime? InstalledAt { get; set; }
		public DateTime? RemovedAt { get; set; }

		public class Validator : AbstractValidator<Edit> {
			public Validator() {
				RuleFor(x => x.Name)
					.NotEmpty().WithMessage("Naam mag niet leeg zijn.")
					.MaximumLength(250).WithMessage("Naam mag maar max. 250 karakter hebben.");
				RuleFor(x => x.SerialNumber)
					.NotEmpty().WithMessage("Serienummer mag niet leeg zijn.");
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