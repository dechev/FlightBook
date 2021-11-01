using FlightBook.DomainModel;
using FlightBook.ServiceInterfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace FlightBook.Persistence.EFCore
{
    public class EntityRepository<TEntity> : IEntityRepository<TEntity>
        where TEntity : BaseEntity
    {
        private readonly FlightBookContext _context;

        public EntityRepository(
            FlightBookContext context
            )
        {
            _context = context;
        }

        public async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter = null, CancellationToken token = default)
        {
            var dbSet = _context.Set<TEntity>().AsNoTracking();
            if (filter != null) dbSet = dbSet.Where(filter);
            return await dbSet.ToArrayAsync(token).ConfigureAwait(false);
        }

        public Task<TEntity> GetByIdAsync(long id, CancellationToken token = default) =>
            _context.Set<TEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == id, token);

        public async Task<int> InsertAsync(IEnumerable<TEntity> entities, CancellationToken token = default)
        {
            await _context.Set<TEntity>().AddRangeAsync(entities, token).ConfigureAwait(false);
            return await _context.SaveChangesAsync(token).ConfigureAwait(false);
        }
    }
}