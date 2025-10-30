using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.Projects;

namespace Rise.Persistence.Configurations.Projects;

/// <summary>
///     Specific configuration for <see cref="Zone" />.
/// </summary>
internal class ZoneConfiguration : EntityConfiguration<Zone> {
	public override void Configure(EntityTypeBuilder<Zone> builder) {
		base.Configure(builder);
		builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
	}
}