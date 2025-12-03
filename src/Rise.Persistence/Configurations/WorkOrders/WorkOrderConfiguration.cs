using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.Projects;
using Rise.Domain.WorkOrders;

namespace Rise.Persistence.Configurations.Workorders;

/// <summary>
///     Specific configuration for <see cref="WorkOrder" />.
///     See https://hogent-web.github.io/csharp/chapters/09/slides/index.html#1 for more information.
/// </summary>
internal class WorkOrderConfiguration : EntityConfiguration<WorkOrder> {
	public override void Configure(EntityTypeBuilder<WorkOrder> builder) {
		// Properties

		base.Configure(builder);
		builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
		builder.Property(x => x.Description).IsRequired().HasMaxLength(2000);
		builder.Property(x => x.Status).IsRequired();
		builder.Property(x => x.Priority).IsRequired();
		builder.OwnsOne(x => x.DateRange, d => {
			d.Property(x => x.StartDate).IsRequired().HasColumnName(nameof(DateRange.StartDate));
			d.Property(x => x.EndDate).HasColumnName(nameof(DateRange.EndDate));
		}).Navigation(x => x.DateRange);

		// Relations

		builder.HasOne(x => x.MaintObj)
			.WithMany()
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(x => x.Technician)
			.WithMany()
			.OnDelete(DeleteBehavior.Restrict);
	}
}