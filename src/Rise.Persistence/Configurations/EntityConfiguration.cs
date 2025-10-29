using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.Common;

namespace Rise.Persistence.Configurations;

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