using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.Common;
using Rise.Domain.MaintObjects;

namespace Rise.Persistence.Configurations.MaintObjects;

internal class MaintObjConfiguration : EntityConfiguration<MaintObj> {
	public override void Configure(EntityTypeBuilder<MaintObj> builder) {
		base.Configure(builder);
		// Properties

		builder.Property(x => x.Name).IsRequired().HasMaxLength(256);
		builder.Property(x => x.SerialNumber).IsRequired().HasMaxLength(256);
		builder.Property(x => x.Brand).HasMaxLength(256);
		builder.Property(x => x.MaintObjType).IsRequired();
		builder.Property(x => x.MaintObjStatus).IsRequired();
		builder.OwnsOne(x => x.Installation, b => {
			b.Property(p => p.InstalledAt).IsRequired().HasColumnName(nameof(Installation.InstalledAt));
			b.Property(p => p.RemovedAt).HasColumnName(nameof(Installation.RemovedAt));
		}).Navigation(e => e.Installation);

		// Relations

		builder.HasOne(x => x.MaintObjPlan)
			.WithMany()
			.IsRequired()
			.OnDelete(DeleteBehavior.Cascade);


		builder.HasOne(x => x.Location)
			.WithMany()
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(x => x.LastScannedBy)
			.WithMany()
			.OnDelete(DeleteBehavior.Restrict);
	}
}

internal class MaintObjHistoryConfiguration : HistoryEntityConfiguration<MaintObjHistory> {
	public override void Configure(EntityTypeBuilder<MaintObjHistory> builder) {
		base.Configure(builder);

		// Properties

		builder.Property(x => x.MaintObjStatus).IsRequired();
		builder.OwnsOne(x => x.Installation, b => {
			b.Property(p => p.InstalledAt).IsRequired().HasColumnName(nameof(Installation.InstalledAt));
			b.Property(p => p.RemovedAt).HasColumnName(nameof(Installation.RemovedAt));
		}).Navigation(e => e.Installation);

		// Relations

		builder.HasOne(x => x.MaintObjPlan)
			.WithMany()
			.IsRequired()
			.OnDelete(DeleteBehavior.Cascade);


		builder.HasOne(x => x.Location)
			.WithMany()
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(x => x.LastScannedBy)
			.WithMany()
			.OnDelete(DeleteBehavior.Restrict);
	}
}