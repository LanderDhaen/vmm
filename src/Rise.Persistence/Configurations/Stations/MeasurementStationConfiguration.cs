using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.Stations;

namespace Rise.Persistence.Configurations.Stations;

/// <summary>
///     Specific configuration for <see cref="MeasurementStation" />.
/// </summary>
internal class MeasurementStationConfiguration : EntityConfiguration<MeasurementStation> {
	public override void Configure(EntityTypeBuilder<MeasurementStation> builder) {
		base.Configure(builder);

		// Columns
		builder.Property(x => x.Code).IsRequired().HasMaxLength(100);
		builder.Property(x => x.Status).IsRequired().HasMaxLength(50);

		builder.Property(x => x.InstalledAt);
		builder.Property(x => x.RemovedAt);

		// Relations
		builder.HasOne(x => x.Location); // optional/required depending on your seeding

		// Indexes
		builder.HasIndex(x => x.Code).IsUnique();
	}
}