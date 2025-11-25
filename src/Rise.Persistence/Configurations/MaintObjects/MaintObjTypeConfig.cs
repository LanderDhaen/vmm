using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.MaintObjects;

namespace Rise.Persistence.Configurations.MaintObjects;

internal class MaintObjTypeConfig : EntityConfiguration<MaintObjType> {
	public override void Configure(EntityTypeBuilder<MaintObjType> builder) {
		base.Configure(builder);
		builder.Property(x => x.Code)
			.IsRequired()
			.HasMaxLength(128);

		builder.HasIndex(x => x.Code).IsUnique();

		builder.Property(x => x.Name)
			.IsRequired()
			.HasMaxLength(256);

		builder.Property(x => x.MaintenanceIntervalDays);
		builder.Property(x => x.CalibrationIntervalDays);


		builder.HasMany(x => x.MaintObjects).WithOne(m => m.MaintObjType).HasForeignKey("MaintObjTypeId");

		// Seed common types with deterministic GUIDs so migrations can map existing enum ints to these IDs
		builder.HasData(
			new MaintObjType(Guid.Parse("00000000-0000-0000-0000-000000000001"), "Sonde", "Sonde", 14, 21),
			new MaintObjType(Guid.Parse("00000000-0000-0000-0000-000000000002"), "Logger", "Logger", 90, 97),
			new MaintObjType(Guid.Parse("00000000-0000-0000-0000-000000000003"), "Modem", "Modem", 180, 187),
			new MaintObjType(Guid.Parse("00000000-0000-0000-0000-000000000004"), "Sim", "SIM", 365, 372),
			new MaintObjType(Guid.Parse("00000000-0000-0000-0000-000000000005"), "Voeding", "Voeding", 365, 372)
		);
	}
}