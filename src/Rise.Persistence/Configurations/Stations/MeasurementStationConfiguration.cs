using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Stations;

namespace Rise.Persistence.Configurations.Stations
{
	internal class MeasurementStationConfiguration : EntityConfiguration<MeasurementStation>
	{
		public override void Configure(EntityTypeBuilder<MeasurementStation> builder)
		{
			base.Configure(builder);

			builder.ToTable("MeasurementStations");

			builder.Property(x => x.Code).IsRequired().HasMaxLength(100);
			builder.Property(x => x.Status).IsRequired().HasMaxLength(100);

			builder.HasOne(s => s.Location)
				.WithMany()
				.HasForeignKey(s => s.LocationId)
				.OnDelete(DeleteBehavior.Restrict);
			
			builder.HasOne(s => s.Project)
				.WithMany()                 
				.HasForeignKey(s => s.ProjectId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasIndex(x => x.Code).IsUnique();
		}
	}
}