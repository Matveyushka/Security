using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace CyberEnvironment.Security;

public interface ISecureDbContext<SUser>
    where SUser : class, ISecureUser<SUser>
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    DbSet<SUser> User { get; }
    DbSet<SecureObject<SUser>> SecureObject { get; }
    DbSet<SecureRole<SUser>> Role { get; }
    DbSet<SecureRelation<SUser>> Relation { get; }

    void SetDbRelations(ModelBuilder modelBuilder)
    {
        var type = GetType();
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            if (property.PropertyType.IsGenericType &&
                property.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                property.PropertyType.GetGenericArguments()[0].GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IProtectedObject<>)))
            {
                var protectedType = property.PropertyType.GetGenericArguments()[0];

                var foreignKeyName = protectedType.ToString() + "Id";
                var foreignPropertyName = protectedType.ToString();

                modelBuilder.Entity<SecureObject<SUser>>()
                    .Property<Guid?>(foreignKeyName);

                MethodInfo? foreignProperty = modelBuilder
                    .Entity<SecureObject<SUser>>()
                    .GetType()
                    .GetMethod("Property", 1, new[] { typeof(Expression<Func<SecureObject<SUser>, string>>) })?
                    .MakeGenericMethod(new Type[] { protectedType });

                foreignProperty?.Invoke(this, new object[] { foreignPropertyName });

                var hasOneProtectedObject = modelBuilder
                    .Entity<SecureObject<SUser>>()
                    .GetType()
                    .GetMethods()
                    .Where(method =>
                        method.Name == "HasOne" &&
                        method.IsGenericMethod)
                    .ToList()[1]
                    .MakeGenericMethod(new Type[] { protectedType });

                var hasOneResult = hasOneProtectedObject?.Invoke(modelBuilder.Entity<SecureObject<SUser>>(), new object?[] { null });

                var hasOneReturnedType = typeof(Microsoft.EntityFrameworkCore.Metadata.Builders.ReferenceNavigationBuilder<,>)
                    .MakeGenericType(new Type[] { typeof(SecureObject<SUser>), protectedType });

                var withOne = hasOneReturnedType.GetMethod("WithOne", 0, new[] { typeof(string) });

                var withOneResult = withOne?.Invoke(hasOneResult, new object[] { "SecureObject" });

                var withOneReturnedType = typeof(Microsoft.EntityFrameworkCore.Metadata.Builders.ReferenceReferenceBuilder<,>)
                    .MakeGenericType(new Type[] { typeof(SecureObject<SUser>), protectedType });

                var onDelete = withOneReturnedType.GetMethod("OnDelete", 0, new[] { typeof(DeleteBehavior) });

                onDelete?.Invoke(withOneResult, new object[] { DeleteBehavior.Cascade });
            }
        }

        modelBuilder.Entity<SUser>()
            .HasMany(user => user.Relations)
            .WithOne(relation => relation.User)
            .HasForeignKey(relation => relation.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SUser>()
            .HasMany(user => user.Roles)
            .WithMany(role => role.Users)
            .UsingEntity(j => j.ToTable("UserRoles"));

        modelBuilder.Entity<SUser>()
            .HasIndex(user => user.ExternalUserId)
            .IsUnique();

        modelBuilder.Entity<SecureObject<SUser>>()
            .HasMany(secureObject => secureObject.Relations)
            .WithOne(relation => relation.SecureObject)
            .HasForeignKey(relation => relation.SecureObjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SecureRole<SUser>>()
            .HasIndex(role => role.Role)
            .IsUnique();
    }
}