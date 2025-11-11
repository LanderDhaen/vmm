using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.Projects;

namespace Rise.Persistence.Configurations.Projects;

/// <summary>
///     Specific configuration for <see cref="MeasurementNetwork" />.
/// </summary>
internal class MeasurementNetworkConfiguration : EntityConfiguration<MeasurementNetwork> {
	public override void Configure(EntityTypeBuilder<MeasurementNetwork> builder) {
		base.Configure(builder);
		builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
		builder.HasOne(x => x.Zone)
			.WithMany()
			.OnDelete(DeleteBehavior.Restrict);


		// See /// See https://hogent-web.github.io/csharp/chapters/09/slides/index.html#1 for more information.
		// Other MeasurementNetwork configuration here.
	}
}