using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace TransactR.EntityFramework;

public class TransactRModelCustomizer<TDbContext, TState> : IModelCustomizer
    where TDbContext : DbContext
    where TState : class, IState, new()
{
    public void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<MementoEntity<TState>>(builder =>
        {
            builder.HasKey(e => new { e.TransactionId, e.Step });

            builder.Property(e => e.TransactionId).IsRequired();
            builder.Property(e => e.Step).IsRequired();
            builder.Property(e => e.State).IsRequired();
        });
    }
}
