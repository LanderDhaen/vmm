using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.Workorders;

namespace Rise.Persistence.Configurations.Workorders;

/// <summary>
///     Specific configuration for <see cref="Workorder" />.
///     See https://hogent-web.github.io/csharp/chapters/09/slides/index.html#1 for more information.
/// </summary>
internal class WorkorderConfiguration : EntityConfiguration<Workorder> {
	public override void Configure(EntityTypeBuilder<Workorder> builder) {
		// Properties

		base.Configure(builder);
		builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
		builder.Property(x => x.Description).IsRequired().HasMaxLength(2000);
		builder.Property(x => x.Status).IsRequired();
		builder.Property(x => x.Prioriteit).IsRequired();

		// Relations

		builder.HasOne(x => x.MaintObj)
			.WithMany()
			.OnDelete(DeleteBehavior.Restrict);
		
		builder.HasOne(x => x.Technician)
			.WithMany()
			.OnDelete(DeleteBehavior.Restrict);
	}
}