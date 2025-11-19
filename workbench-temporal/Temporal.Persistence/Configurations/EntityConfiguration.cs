using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Temporal.Domain.Common;

namespace Temporal.Persistence.Configurations;

/// <summary>
///     Base configuration for an <see cref="Entity" />
/// </summary>
internal class EntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : Entity {
	public virtual void Configure(EntityTypeBuilder<TEntity> builder) {
		// Table name = class name (singular)
		builder.ToTable(typeof(TEntity).Name);

		// CreatedAt: let SQL Server fill the default timestamp.
		builder.Property(e => e.CreatedAt)
			.HasDefaultValueSql("CURRENT_TIMESTAMP");

		// UpdatedAt: let SQL Server fill the default timestamp.
		builder.Property(e => e.UpdatedAt)
			.HasDefaultValueSql("CURRENT_TIMESTAMP");

		// IsDeleted should be false by default, used for soft delete.
		builder.Property(e => e.IsDeleted)
			.HasDefaultValue(false);
	}
}

/// <summary>
///     Base configuration for an <see cref="HistoryEntity" />
/// </summary>
internal class HistoryEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : HistoryEntity {
	public virtual void Configure(EntityTypeBuilder<TEntity> builder) {
		// Table name = class name (singular)
		builder.ToTable(typeof(TEntity).Name);

		builder.OwnsOne(x => x.ValidityRange, d => {
			d.Property(x => x.ValidFrom).IsRequired().HasColumnName(nameof(ValidityRange.ValidFrom));
			d.Property(x => x.ValidTo).IsRequired().HasColumnName(nameof(ValidityRange.ValidTo));
		}).Navigation(x => x.ValidityRange).IsRequired();
	}
}