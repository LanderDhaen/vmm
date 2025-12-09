using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.MaintObjects;

namespace Rise.Persistence.Configurations.MaintObjects;

internal class MaintObjPlanConfiguration : EntityConfiguration<MaintObjPlan> {
	public override void Configure(EntityTypeBuilder<MaintObjPlan> builder) {
		base.Configure(builder);

		// Properties

		builder.Property(x => x.Name).IsRequired().HasMaxLength(256);
		builder.Property(x => x.MaintenanceIntervalDays).IsRequired();
		builder.Property(x => x.CalibrationIntervalDays).IsRequired();
	}
}