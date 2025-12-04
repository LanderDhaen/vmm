using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.Networks;

namespace Rise.Persistence.Configurations.Zones;

/// <summary>
///     Specific configuration for <see cref="Zone" />.
/// </summary>
internal class ZoneConfiguration : EntityConfiguration<Zone> {
	public override void Configure(EntityTypeBuilder<Zone> builder) {
		base.Configure(builder);
		builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
		builder.Property(x => x.Code).IsRequired().HasMaxLength(50);

		// Constraints

		builder.HasIndex(x => x.Code).IsUnique();
	}
}