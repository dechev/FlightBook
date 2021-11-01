using FlightBook.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace FlightBook.ServiceInterfaces
{
    public interface IEntityRepository<TEntity>
        where TEntity : BaseEntity
    {
        Task<TEntity> GetByIdAsync(long id, CancellationToken token = default);

        Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter = null, CancellationToken token = default);
    }
}