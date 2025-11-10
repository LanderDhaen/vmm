namespace Rise.Shared.Stations;

public static partial class MeasurementStationRequest {
	public class Create {
		public string Code { get; set; } = default!;
		//public string Status { get; set; } = default!;
		public Guid LocationId { get; set; }
		public Guid ProjectId { get; set; }
		public DateTime? InstalledAt { get; set; }
		
		public class Validator : AbstractValidator<Create> {
			public Validator() {
				RuleFor(x => x.Code)
					.NotEmpty().WithMessage("Code mag niet leeg zijn.")
					.MaximumLength(50).WithMessage("Code maar max. 50 karakter hebben.");
				RuleFor(x => x.LocationId)
					.NotNull().WithMessage("Je moet een locatie selecteren.");
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

public static partial class MeasurementStationResponse {
	public class Create {
		public Guid MeasurementStationId { get; set; }
	}
}