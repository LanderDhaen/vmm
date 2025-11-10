using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Temporal.Domain.Projects;
using Temporal.Domain.Common;

namespace Temporal.Persistence.Configurations.Projects;

/// <summary>
///     Specific configuration for <see cref="Project" />.
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

		// Constraints

		builder.HasIndex(x => x.Code).IsUnique();
		
		// Relations

		builder.HasMany(x => x.ProjectHistories);
	}
}

internal class ProjectHistoryConfiguration : HistoryEntityConfiguration<ProjectHistory> {
	public override void Configure(EntityTypeBuilder<ProjectHistory> builder) {
		base.Configure(builder);
		builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
		builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
		builder.OwnsOne(x => x.DateRange, d => {
			d.Property(x => x.StartDate).IsRequired().HasColumnName(nameof(DateRange.StartDate));
			d.Property(x => x.EndDate).HasColumnName(nameof(DateRange.EndDate));
		}).Navigation(x => x.DateRange).IsRequired();
		
		// History
		
		builder.OwnsOne(x => x.ValidityRange, v => {
			v.Property(x => x.ValidFrom).IsRequired().HasColumnName(nameof(ValidityRange.ValidFrom));
			v.Property(x => x.ValidTo).IsRequired().HasColumnName(nameof(ValidityRange.ValidTo));
		}).Navigation(x => x.ValidityRange).IsRequired();
	}
}