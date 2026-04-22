using EHub.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace EHub.EntityFrameworkCore.Interceptors;

public sealed class PreventSoftDeleteIfReferencedInterceptor : SaveChangesInterceptor, ITransientDependency
{
    private readonly IStringLocalizer<EHubResource> _L;

    public PreventSoftDeleteIfReferencedInterceptor(IStringLocalizer<EHubResource> l)
    {
        _L = l;
    }

    private static readonly MethodInfo DbContextSetMethod =
        typeof(DbContext).GetMethods()
            .Single(m => m.Name == nameof(DbContext.Set)
                         && m.IsGenericMethodDefinition
                         && m.GetParameters().Length == 0);

    private static readonly MethodInfo QueryableWhereMethod =
        typeof(Queryable).GetMethods()
            .Where(m => m.Name == nameof(Queryable.Where)
                        && m.IsGenericMethodDefinition
                        && m.GetParameters().Length == 2)
            .Single(m =>
            {
                var p1 = m.GetParameters()[1].ParameterType;
                if (!p1.IsGenericType || p1.GetGenericTypeDefinition() != typeof(Expression<>))
                    return false;

                var func = p1.GetGenericArguments()[0];
                return func.IsGenericType && func.GetGenericTypeDefinition() == typeof(Func<,>);
            });

    private static readonly MethodInfo AnyAsyncMethod =
        typeof(EntityFrameworkQueryableExtensions).GetMethods()
            .Where(m => m.Name == nameof(EntityFrameworkQueryableExtensions.AnyAsync)
                        && m.IsGenericMethodDefinition)
            .Single(m =>
            {
                var ps = m.GetParameters();
                return ps.Length == 2 && ps[1].ParameterType == typeof(CancellationToken);
            });

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var db = eventData.Context;
        if (db is null)
            return result;

        var deleting = db.ChangeTracker.Entries()
            .Where(e => e.Entity is ISoftDelete && (
                e.State == EntityState.Deleted ||
                (e.State == EntityState.Modified &&
                 e.Property(nameof(ISoftDelete.IsDeleted)).IsModified &&
                 e.Property(nameof(ISoftDelete.IsDeleted)).CurrentValue is bool b && b)))
            .ToList();

        if (deleting.Count == 0)
            return result;

        foreach (var principalEntry in deleting)
        {
            var principalType = principalEntry.Metadata;

            var pk = principalType.FindPrimaryKey();
            if (pk is null || pk.Properties.Count != 1)
                continue;

            var pkProp = pk.Properties[0];
            var pkValue = principalEntry.Property(pkProp.Name).CurrentValue;
            if (pkValue is null)
                continue;

            foreach (var fk in principalType.GetReferencingForeignKeys())
            {
                if (fk.DeclaringEntityType.IsOwned())
                    continue;

                if (fk.PrincipalKey != pk || fk.Properties.Count != 1)
                    continue;

                var dependentClr = fk.DeclaringEntityType.ClrType;
                var dependentFkProp = fk.Properties[0];

                var anyActiveDependent = await AnyDependentAsync(
                    db,
                    dependentClr,
                    dependentFkProp,
                    pkValue,
                    cancellationToken
                );

                if (anyActiveDependent)
                {
                    if (HasDependentPendingDelete(db, dependentClr, dependentFkProp, pkValue))
                        continue;

                    throw new BusinessException("EHub:DeleteRestricted")
                        .WithData("Entity", _L[principalType.ClrType.Name ?? ""])
                        .WithData("ReferencedBy", _L[dependentClr.Name ?? ""]);
                }
            }
        }

        return result;
    }

    private static Task<bool> AnyDependentAsync(
        DbContext db,
        Type dependentClr,
        IProperty dependentFkProp,
        object pkValue,
        CancellationToken ct)
    {
        var param = Expression.Parameter(dependentClr, "d");

        var left = Expression.Call(
            typeof(EF),
            nameof(EF.Property),
            new[] { dependentFkProp.ClrType },
            param,
            Expression.Constant(dependentFkProp.Name));

        var right = Expression.Convert(Expression.Constant(pkValue), dependentFkProp.ClrType);

        var body = Expression.Equal(left, right);
        var lambda = Expression.Lambda(body, param);

        var set = GetQueryableSet(db, dependentClr);

        var whereGeneric = QueryableWhereMethod.MakeGenericMethod(dependentClr);
        var filtered = (IQueryable)whereGeneric.Invoke(null, new object[] { set, lambda })!;

        var anyGeneric = AnyAsyncMethod.MakeGenericMethod(dependentClr);
        return (Task<bool>)anyGeneric.Invoke(null, new object[] { filtered, ct })!;
    }

    private static bool HasDependentPendingDelete(
        DbContext db,
        Type dependentClr,
        IProperty dependentFkProp,
        object pkValue)
    {
        return db.ChangeTracker.Entries()
            .Any(e =>
                e.Metadata.ClrType == dependentClr &&
                (
                    e.State == EntityState.Deleted ||
                    (e.State == EntityState.Modified &&
                     e.Entity is ISoftDelete &&
                     e.Property(nameof(ISoftDelete.IsDeleted)).IsModified &&
                     e.Property(nameof(ISoftDelete.IsDeleted)).CurrentValue is bool b && b)
                ) &&
                Equals(e.Property(dependentFkProp.Name).CurrentValue, pkValue)
            );
    }

    private static IQueryable GetQueryableSet(DbContext db, Type entityClrType)
    {
        var generic = DbContextSetMethod.MakeGenericMethod(entityClrType);
        var setObj = generic.Invoke(db, null)!;
        return (IQueryable)setObj;
    }
}