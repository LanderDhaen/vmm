using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.Stations;

namespace Rise.Persistence.Configurations.Stations;

/// <summary>
///     Specific configuration for <see cref="MeasurementStation" />.
///     See https://hogent-web.github.io/csharp/chapters/09/slides/index.html#1 for more information.
/// </summary>
internal class MeasurementStationConfiguration : EntityConfiguration<MeasurementStation> {
	public override void Configure(EntityTypeBuilder<MeasurementStation> builder) {
		// Properties

		base.Configure(builder);
		builder.Property(x => x.Code).IsRequired().HasMaxLength(64);
		builder.OwnsOne(x => x.Installation, b => {
			b.Property(p => p.InstalledAt).IsRequired().HasColumnName(nameof(Installation.InstalledAt));
			b.Property(p => p.RemovedAt).HasColumnName(nameof(Installation.RemovedAt));
		}).Navigation(e => e.Installation).IsRequired();

		// Relations

		builder.HasOne(x => x.Location)
			.WithMany()
			.IsRequired()
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(s => s.Projects)
			.WithMany(p => p.Stations);

		// Constraints

		builder.HasIndex(x => x.Code).IsUnique();
	}
}

internal class MeasurementStationHistoryConfiguration : HistoryEntityConfiguration<MeasurementStationHistory> {
	public override void Configure(EntityTypeBuilder<MeasurementStationHistory> builder) {
		base.Configure(builder);

		// Properties

		builder.OwnsOne(x => x.Installation, i => {
			i.Property(x => x.InstalledAt).IsRequired().HasColumnName(nameof(Installation.InstalledAt));
			i.Property(x => x.RemovedAt).HasColumnName(nameof(Installation.RemovedAt));
		}).Navigation(e => e.Installation).IsRequired();

		// Relations

		builder.HasOne(x => x.Location)
			.WithMany()
			.IsRequired()
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(s => s.MaintObjects)
			.WithOne(m => m.Station)
			.IsRequired(false)
			.OnDelete(DeleteBehavior.SetNull);
	}
}