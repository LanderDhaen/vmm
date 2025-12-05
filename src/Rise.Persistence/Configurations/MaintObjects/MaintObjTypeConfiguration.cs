using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.MaintObjects;

namespace Rise.Persistence.Configurations.MaintObjects;

internal class MaintObjTypeConfiguration : EntityConfiguration<MaintObjType> {
	public override void Configure(EntityTypeBuilder<MaintObjType> builder) {
		base.Configure(builder);
		
		// Properties
		
		builder.Property(x => x.Code).IsRequired().HasMaxLength(128);
		builder.Property(x => x.Name).IsRequired().HasMaxLength(256);
		builder.Property(x => x.MaintenanceIntervalDays).IsRequired();
		builder.Property(x => x.CalibrationIntervalDays).IsRequired();
		
		// Constraints
		
		builder.HasIndex(x => x.Code).IsUnique();
	}
}