using FluentValidation;
using FluentValidation.Results;

namespace Rise.Shared.WorkOrders;

public static partial class WorkOrderRequest {
	public class Create {
		public string Name { get; set; }
		public string Description { get; set; }

		public Priority Priority { get; set; } = Priority.Normal;

		public Guid? MaintObjId { get; set; }

		public class Validator : AbstractValidator<Create> {
			public Validator() {
				RuleFor(x => x.Name)
					.NotEmpty().WithMessage("Naam mag niet leeg zijn.")
					.MaximumLength(250).WithMessage("Naam mag maar max. 250 karakters hebben.");

				RuleFor(x => x.Description)
					.NotEmpty().WithMessage("Beschrijving mag niet leeg zijn.")
					.MaximumLength(2000).WithMessage("Beschrijving mag maar max. 2000 karakters hebben.");

				RuleFor(x => x.MaintObjId)
					.NotNull().WithMessage("Je moet een onderhoudsobject selecteren.");
			}

			public Func<object, string, Task<IEnumerable<string>>> ValidateValue =>
				async (model, propertyName) => {
					ValidationResult result =
						await ValidateAsync(
							ValidationContext<Create>.CreateWithOptions(
								(Create)model,
								x => x.IncludeProperties(propertyName)
							)
						);

					return result.IsValid
						? []
						: result.Errors.Select(e => e.ErrorMessage);
				};
		}
	}
}

public static partial class WorkOrderResponse {
	public class Create {
		public Guid WorkOrderId { get; set; }
	}
}