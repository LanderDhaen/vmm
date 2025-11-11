using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Temporal.Domain.Projects;
using Temporal.Domain.Common;
using Temporal.Domain.Stations;

namespace Temporal.Persistence.Configurations.Stations;

/// <summary>
///     Specific configuration for <see cref="MeasurementStation" />.
/// </summary>
internal class MeasurementStationConfiguration : EntityConfiguration<MeasurementStation> {
	public override void Configure(EntityTypeBuilder<MeasurementStation> builder) {
		base.Configure(builder);
		builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
		builder.OwnsOne(x => x.Installation, i => {
			i.Property(x => x.InstalledAt).IsRequired().HasColumnName(nameof(Installation.InstalledAt));
			i.Property(x => x.RemovedAt).HasColumnName(nameof(Installation.RemovedAt));
		}).Navigation(x => x.Installation).IsRequired();

		// Constraints

		builder.HasIndex(x => x.Code).IsUnique();
		
		// Relations

		builder.HasOne(x => x.Location).WithMany().IsRequired().OnDelete(DeleteBehavior.Restrict);
		builder.HasMany(x => x.MeasurementStationHistories);
	}
}

internal class ProjectHistoryConfiguration : HistoryEntityConfiguration<MeasurementStationHistory> {
	public override void Configure(EntityTypeBuilder<MeasurementStationHistory> builder) {
		base.Configure(builder);
		builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
		builder.OwnsOne(x => x.Installation, i => {
			i.Property(x => x.InstalledAt).IsRequired().HasColumnName(nameof(Installation.InstalledAt));
			i.Property(x => x.RemovedAt).HasColumnName(nameof(Installation.RemovedAt));
		}).Navigation(x => x.Installation).IsRequired();
		
		// Relations
		
		builder.HasOne(x => x.Location).WithMany().IsRequired().OnDelete(DeleteBehavior.Restrict);
		
		// History
		
		builder.OwnsOne(x => x.ValidityRange, v => {
			v.Property(x => x.ValidFrom).IsRequired().HasColumnName(nameof(ValidityRange.ValidFrom));
			v.Property(x => x.ValidTo).IsRequired().HasColumnName(nameof(ValidityRange.ValidTo));
		}).Navigation(x => x.ValidityRange).IsRequired();
	}
}