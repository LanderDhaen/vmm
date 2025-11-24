using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.MaintObjects;

namespace Rise.Persistence.Configurations.MaintObjects;

public class MaintObjConfig : IEntityTypeConfiguration<MaintObj> {
	public void Configure(EntityTypeBuilder<MaintObj> builder) {
		builder.HasKey(x => x.Id);

		builder.Property(x => x.Name)
			.IsRequired()
			.HasMaxLength(256);

		builder.Property(x => x.SerialNumber)
			.IsRequired()
			.HasMaxLength(256);

		builder.Property(x => x.MaintObjType)
			.IsRequired();

		builder.Property(x => x.Brand)
			.HasMaxLength(256);

		builder.Property(x => x.Status).IsRequired();

		builder.OwnsOne(x => x.Installation, b => {
			b.Property(p => p.InstalledAt)
				.HasColumnName("InstalledAt")
				.IsRequired();

			b.Property(p => p.RemovedAt)
				.HasColumnName("RemovedAt");

			b.WithOwner();
		});
		builder.Property(x => x.IsInStock).IsRequired();
	}
}