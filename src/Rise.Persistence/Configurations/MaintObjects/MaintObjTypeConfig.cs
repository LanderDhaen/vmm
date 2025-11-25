using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.MaintObjects;

namespace Rise.Persistence.Configurations.MaintObjects;

public class MaintObjTypeConfig : IEntityTypeConfiguration<MaintObjType> {
	public void Configure(EntityTypeBuilder<MaintObjType> builder) {
		builder.HasKey(x => x.Id);

		builder.Property(x => x.Code)
			.IsRequired()
			.HasMaxLength(128);

		builder.HasIndex(x => x.Code).IsUnique();

		builder.Property(x => x.Name)
			.IsRequired()
			.HasMaxLength(256);

		builder.Property(x => x.DefaultMaintenanceIntervalDays);

		builder.HasMany(x => x.MaintObjects).WithOne(m => m.MaintObjType).HasForeignKey("MaintObjTypeId");

		// Seed common types with deterministic GUIDs so migrations can map existing enum ints to these IDs
		builder.HasData(
			new MaintObjType(Guid.Parse("00000000-0000-0000-0000-000000000001"), "Sonde", "Sonde", 14),
			new MaintObjType(Guid.Parse("00000000-0000-0000-0000-000000000002"), "Logger", "Logger", 90),
			new MaintObjType(Guid.Parse("00000000-0000-0000-0000-000000000003"), "Modem", "Modem", 180),
			new MaintObjType(Guid.Parse("00000000-0000-0000-0000-000000000004"), "Sim", "SIM", 365),
			new MaintObjType(Guid.Parse("00000000-0000-0000-0000-000000000005"), "Voeding", "Voeding", 365)
		);
	}
}