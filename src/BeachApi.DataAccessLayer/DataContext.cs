using System.Reflection;
using BeachApi.Contracts;
using BeachApi.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore;

namespace BeachApi.DataAccessLayer;

public class DataContext : DbContext, IDataContext
{
    private static readonly MethodInfo setQueryFilterOnDeletableEntity = typeof(DataContext)
        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilterOnDeletableEntity));

    private static readonly MethodInfo setQueryFilterOnTenantEntity = typeof(DataContext)
        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilterOnTenantEntity));

    private readonly Guid tenantId;

    public DataContext(DbContextOptions<DataContext> options, IUserService userService) : base(options)
    {
        tenantId = userService.GetTenantId();
    }

    public void Delete<T>(T entity) where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        Set<T>().Remove(entity);
    }

    public void Delete<T>(IEnumerable<T> entities) where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));
        Set<T>().RemoveRange(entities);
    }

    public async ValueTask<T> GetAsync<T>(params object[] keyValues) where T : BaseEntity
    {
        var entity = await Set<T>().FindAsync(keyValues);
        return entity;
    }

    public IQueryable<T> Get<T>(bool ignoreQueryFilters = false, bool trackingChanges = false) where T : BaseEntity
    {
        var set = Set<T>().AsQueryable();

        if (ignoreQueryFilters)
        {
            set = set.IgnoreQueryFilters();
        }

        return trackingChanges ? set.AsTracking() : set.AsNoTrackingWithIdentityResolution();
    }

    public void Insert<T>(T entity) where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        Set<T>().Add(entity);
    }

    public async Task SaveAsync()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.Entity.GetType()))
            .ToList();

        foreach (var entry in entries.Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            var baseEntity = entry.Entity as BaseEntity;
            if (entry.State is EntityState.Added)
            {
                if (baseEntity is TenantEntity tenantEntity)
                {
                    tenantEntity.TenantId = tenantId;
                }
            }

            if (entry.State is EntityState.Modified)
            {
                if (baseEntity is TenantEntity tenantEntity)
                {
                    tenantEntity.TenantId = tenantId;
                }

                if (baseEntity is DeletableEntity deletableEntity)
                {
                    deletableEntity.IsDeleted = false;
                    deletableEntity.DeletedDate = null;
                }

                baseEntity.LastModifiedDate = DateTime.UtcNow;
            }

            if (entry.State is EntityState.Deleted)
            {
                if (baseEntity is DeletableEntity deletableEntity)
                {
                    deletableEntity.IsDeleted = true;
                    deletableEntity.DeletedDate = DateTime.UtcNow;
                    entry.State = EntityState.Modified;
                }
            }
        }

        await SaveChangesAsync();
    }

    public async Task ExecuteTransactionAsync(Func<Task> action)
    {
        var strategy = Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await Database.BeginTransactionAsync();
            await action.Invoke();
            await transaction.CommitAsync();
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        var entities = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)).ToList();

        foreach (var type in entities.Select(t => t.ClrType))
        {
            var methods = SetGlobalQueryFiltersMethod(type);
            foreach (var method in methods)
            {
                var genericMethod = method.MakeGenericMethod(type);
                genericMethod.Invoke(this, new object[] { modelBuilder });
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private static IEnumerable<MethodInfo> SetGlobalQueryFiltersMethod(Type type)
    {
        var result = new List<MethodInfo>();

        if (typeof(TenantEntity).IsAssignableFrom(type))
        {
            result.Add(setQueryFilterOnTenantEntity);
        }

        if (typeof(DeletableEntity).IsAssignableFrom(type))
        {
            result.Add(setQueryFilterOnDeletableEntity);
        }

        return result;
    }

    private void SetQueryFilterOnTenantEntity<T>(ModelBuilder builder) where T : TenantEntity
    {
        builder.Entity<T>().HasQueryFilter(t => t.TenantId == tenantId);
    }

    private void SetQueryFilterOnDeletableEntity<T>(ModelBuilder builder) where T : DeletableEntity
    {
        builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted && e.DeletedDate == null && e.TenantId == tenantId);
    }
}