﻿using Skeleton.Abstraction;
using Skeleton.Abstraction.Repository;

namespace Skeleton.Infrastructure.Repository
{
    public sealed class AsyncCrudRepository<TEntity, TDto> :
            AsyncReadRepository<TEntity, TDto>,
            IAsyncCrudRepository<TEntity, TDto>
        where TEntity : class, IEntity<TEntity>
        where TDto : class
    {
        public AsyncCrudRepository(
            IEntityMapper<TEntity, TDto> mapper,
            IAsyncEntityReader<TEntity> reader,
            IAsyncEntityPersistor<TEntity> persistor)
            : base(mapper, reader)
        {
            Store = persistor;
        }

        public IAsyncEntityPersistor<TEntity> Store { get; }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            Store.Dispose();
        }
    }
}