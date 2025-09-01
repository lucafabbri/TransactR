using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;

namespace TransactR.EntityFramework
{
    /// <summary>
    /// Rappresenta un memento persistente in un database tramite Entity Framework.
    /// </summary>
    /// <typeparam name="TState">Il tipo dello stato del memento.</typeparam>
    /// <typeparam name="TStep">Il tipo dell'identificatore del passo.</typeparam>
    public class MementoEntity<TState, TStep>
        where TState : class, new()
        where TStep : notnull, IComparable
    {
        /// <summary>
        /// L'identificatore della transazione.
        /// </summary>
        [Key]
        public string? TransactionId { get; set; }

        /// <summary>
        /// L'identificatore del passo all'interno della transazione.
        /// </summary>
        [Key]
        public TStep? Step { get; set; }

        /// <summary>
        /// Lo stato del memento, serializzato in formato JSON.
        /// </summary>
        public string? State { get; set; }
    }
}
