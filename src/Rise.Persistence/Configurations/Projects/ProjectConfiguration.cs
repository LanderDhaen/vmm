using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.Projects;

namespace Rise.Persistence.Configurations.Projects;

/// <summary>
///     Specific configuration for <see cref="Project" />.
///     See https://hogent-web.github.io/csharp/chapters/09/slides/index.html#1 for more information.
/// </summary>
internal class ProjectConfiguration : EntityConfiguration<Project> {
	public override void Configure(EntityTypeBuilder<Project> builder) {
		base.Configure(builder);
		builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
		builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
		builder.OwnsOne(x => x.DateRange, d => {
			d.Property(x => x.StartDate).IsRequired().HasColumnName(nameof(DateRange.StartDate));
			d.Property(x => x.EndDate).HasColumnName(nameof(DateRange.EndDate));
		}).Navigation(x => x.DateRange).IsRequired();
		builder.HasOne(x => x.MeasurementNetwork)
			.WithMany()
			.OnDelete(DeleteBehavior.Restrict);
		builder.Property(x => x.Status).IsRequired();

		// Constraints
		builder.HasIndex(x => x.Code).IsUnique();


		// Other Project configuration here.
	}
}