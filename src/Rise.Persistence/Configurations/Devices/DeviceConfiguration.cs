using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.Devices;

namespace Rise.Persistence.Configurations.Devices;

public class DeviceConfiguration : IEntityTypeConfiguration<Device> {
	public void Configure(EntityTypeBuilder<Device> builder) {
		builder.ToTable("Devices");

		builder.HasKey(x => x.Id);

		builder.Property(x => x.Name)
			.IsRequired()
			.HasMaxLength(256);

		builder.Property(x => x.SerialNumber)
			.IsRequired()
			.HasMaxLength(256);

		builder.Property(x => x.DeviceType)
			.IsRequired();

		builder.Property(x => x.Brand)
			.HasMaxLength(256);

		// Configure optional relationship to MeasurementStation
		builder.HasOne(d => d.MeasurementStation)
			.WithMany()
			.HasForeignKey(d => d.MeasurementStationId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}