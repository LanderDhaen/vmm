using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Stations;
using Rise.Domain.Projects;

namespace Rise.Persistence.Configurations.Stations;

public class MeasurementStationConfiguration : IEntityTypeConfiguration<MeasurementStation>
{
	public void Configure(EntityTypeBuilder<MeasurementStation> builder)
	{
		builder.ToTable("MeasurementStations");

		builder.HasKey(x => x.Id);

		builder.Property(x => x.Code)
			.IsRequired()
			.HasMaxLength(64);

		builder.Property(x => x.Status)
			.IsRequired()
			.HasMaxLength(32);
		
		builder.HasOne(x => x.Location)
			.WithMany()
			.IsRequired()
			.OnDelete(DeleteBehavior.Restrict);
		
		builder.OwnsOne(x => x.Installation, b =>
		{
			b.Property(p => p.InstalledAt)
				.HasColumnName("InstalledAt")
				.IsRequired();

			b.Property(p => p.RemovedAt)
				.HasColumnName("RemovedAt");

			b.WithOwner();
		});

		builder.HasMany(s => s.Projects)
			.WithMany(p => p.Stations)
			.UsingEntity<Dictionary<string, object>>(
				"ProjectStations",
				right => right.HasOne<Project>()
					.WithMany()
					.HasForeignKey("ProjectId")
					.OnDelete(DeleteBehavior.Cascade),
				left  => left.HasOne<MeasurementStation>()
					.WithMany()
					.HasForeignKey("StationId")
					.OnDelete(DeleteBehavior.Cascade),
				join =>
				{
					join.ToTable("ProjectStations");
					join.HasKey("ProjectId", "StationId");
					join.HasIndex("StationId");
				});

	}
}