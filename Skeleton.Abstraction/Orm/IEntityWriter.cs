﻿using Skeleton.Abstraction.Domain;
using System;
using System.Collections.Generic;

namespace Skeleton.Abstraction.Orm
{
    public interface IEntityWriter<in TEntity> :
            IDisposable,
            IHideObjectMethods
        where TEntity : class, IEntity<TEntity>, new()
    {
        bool Add(TEntity entity);

        bool Add(IEnumerable<TEntity> entities);

        bool Delete(TEntity entity);

        bool Delete(IEnumerable<TEntity> entities);

        bool Save(TEntity entity);

        bool Save(IEnumerable<TEntity> entities);

        bool Update(TEntity entity);

        bool Update(IEnumerable<TEntity> entities);
    }
}