namespace Rise.Shared.Locations;

public static partial class LocationRequest {
	public class Create {
		public string Name { get; set; }
		public string? Addressline1 { get; set; }
		public string? Addressline2 { get; set; }
		public string City { get; set; }
		public string PostalCode { get; set; }
		public string CountryIso { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
	}

	public class Validator : AbstractValidator<Create> {
		public Validator() {
			RuleFor(x => x.Name)
				.NotEmpty().WithMessage("Naam mag niet leeg zijn.")
				.MaximumLength(250).WithMessage("Naam mag maar max. 250 karakter hebben.");
			RuleFor(x => x.Addressline1)
				.MaximumLength(250).WithMessage("Adresregel 1 mag maar max. 250 karakter hebben.");
			RuleFor(x => x.Addressline2)
				.MaximumLength(250).WithMessage("Adresregel 2 mag maar max. 250 karakter hebben.");
			RuleFor(x => x.City)
				.NotEmpty().WithMessage("Stad mag niet leeg zijn.");
			RuleFor(x => x.PostalCode)
				.NotEmpty().WithMessage("Postcode mag niet leeg zijn.")
				.MaximumLength(20).WithMessage("Postcode mag maar max. 20 karakter hebben.");
			RuleFor(x => x.CountryIso)
				.NotEmpty().WithMessage("Landcode mag niet leeg zijn.")
				.MaximumLength(2).WithMessage("Landcode mag maar max. 2 karakter hebben.");
			RuleFor(x => x.X)
				.NotEmpty().WithMessage("X-coordinaat mag niet leeg zijn.");
			RuleFor(x => x.Y)
				.NotEmpty().WithMessage("Y-coordinaat mag niet leeg zijn.");
		}
	}
}

public static partial class LocationResponse {
	public class Create {
		public Guid LocationId { get; set; }
	}
}