using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.MaintObjects;

namespace Rise.Persistence.Configurations.MaintObjects;

internal class MaintObjConfig : EntityConfiguration<MaintObj> {
	public override void Configure(EntityTypeBuilder<MaintObj> builder) {
		base.Configure(builder);

		builder.Property(x => x.Name)
			.IsRequired()
			.HasMaxLength(256);

		builder.Property(x => x.SerialNumber)
			.IsRequired()
			.HasMaxLength(256);

		builder.HasOne(x => x.MaintObjType)
			.WithMany(t => t.MaintObjects)
			.IsRequired();

		builder.Property(x => x.Brand)
			.HasMaxLength(256);

		builder.Property(x => x.MaintObjStatus).IsRequired();

		builder.OwnsOne(x => x.Installation, b => {
			b.Property(p => p.InstalledAt)
				.HasColumnName("InstalledAt")
				.IsRequired();

			b.Property(p => p.RemovedAt)
				.HasColumnName("RemovedAt");

			b.WithOwner();
		});
		builder.Property(x => x.IsInStock).IsRequired();

		builder.HasOne(x => x.Location)
			.WithMany()
			.OnDelete(DeleteBehavior.Restrict);
	}
}