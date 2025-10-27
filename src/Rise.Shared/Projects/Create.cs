namespace Rise.Shared.Projects;

/// <summary>
///     Represents a static utility class containing request-related structures for projects.
/// </summary>
public static partial class ProjectRequest {
	public class Create {
		public string Name { get; set; }
		public string Code { get; set; }
		public DateOnly StartDate { get; set; }

		public class Validator : AbstractValidator<Create> {
			public Validator() {
				RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
				RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
				RuleFor(x => x.StartDate).NotNull();
			}
		}
	}
}

public static partial class ProjectResponse {
	public class Create {
		public Guid ProjectId { get; set; }
	}
}