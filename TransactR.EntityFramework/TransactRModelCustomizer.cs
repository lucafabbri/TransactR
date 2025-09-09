using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace TransactR.EntityFramework;

public class TransactRModelCustomizer<TDbContext, TStep, TContext> : IModelCustomizer
    where TDbContext : DbContext
    where TStep : notnull, IComparable
    where TContext : class, ITransactionContext<TStep, TContext>, new()
{
    public void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<MementoEntity<TStep, TContext>>(builder =>
        {
            builder.HasKey(e => new { e.TransactionId, e.Step });

            builder.Property(e => e.TransactionId).IsRequired();
            builder.Property(e => e.Step).IsRequired();
            builder.Property(e => e.State).IsRequired();
        });
    }
}
