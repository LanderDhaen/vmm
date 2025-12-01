using EntityFrameworkCore.Triggered;

using Microsoft.EntityFrameworkCore;

using Rise.Domain.Common;

namespace Rise.Persistence.Triggers;

/// <summary>
///     Programmatic trigger, similar to a database trigger but database-agnostic.
///     It works across various databases (e.g., swapping from Microsoft SQL Server to MariaDB).
///     More info: https://github.com/koenbeuk/EntityFrameworkCore.Triggered
/// </summary>
public class EntityBeforeSaveTrigger(ApplicationDbContext dbContext) : IBeforeSaveTrigger<Entity> {
	public Task BeforeSave(ITriggerContext<Entity> context, CancellationToken cancellationToken) {
		Entity entity = context.Entity;
		DateTime currentTime = DateTime.UtcNow;

		switch (context.ChangeType) {
		case ChangeType.Added:
			entity.CreatedAt = currentTime;
			entity.UpdatedAt = currentTime;
			break;
		case ChangeType.Modified:
			entity.UpdatedAt = currentTime;
			break;
		case ChangeType.Deleted:
			entity.IsDeleted = true;
			entity.UpdatedAt = currentTime;
			dbContext.Entry(entity).State = EntityState.Modified;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}

		return Task.CompletedTask;
	}
}