using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rise.Domain.Locations;

namespace Rise.Persistence.Configurations.Locations;

/// <summary>
///     Specific configuration for <see cref="Location" />.
/// </summary>
internal class LocationConfiguration : EntityConfiguration<Location> {
	public override void Configure(EntityTypeBuilder<Location> builder) {
		base.Configure(builder);
		builder.Property(x => x.Name).IsRequired().HasMaxLength(250);

		// Value Object Mapping and rename of columns
		builder.OwnsOne(x => x.Address, a => {
			a.Property(x => x.Addressline1).HasMaxLength(250)
				.HasColumnName(nameof(Address.Addressline1));
			a.Property(x => x.Addressline2).HasMaxLength(250)
				.HasColumnName(nameof(Address.Addressline2));
			a.Property(x => x.City).IsRequired().HasMaxLength(50).HasColumnName(nameof(Address.City));
			a.Property(x => x.PostalCode).IsRequired().HasMaxLength(20).HasColumnName(nameof(Address.PostalCode));
			a.Property(x => x.CountryIso).IsRequired().HasMaxLength(2).HasColumnName(nameof(Address.CountryIso));
		}).Navigation(x => x.Address);

		builder.OwnsOne(x => x.Coordinate, c => {
			c.Property(x => x.X).IsRequired().HasColumnName(nameof(Coordinate.X)).HasPrecision(18, 6);
			c.Property(x => x.Y).IsRequired().HasColumnName(nameof(Coordinate.Y)).HasPrecision(18, 6);
		}).Navigation(x => x.Coordinate).IsRequired();

		builder.Property(x => x.IsStockLocation).IsRequired();

		// See /// See https://hogent-web.github.io/csharp/chapters/09/slides/index.html#1 for more information.
		// Other Project configuration here.
	}
}