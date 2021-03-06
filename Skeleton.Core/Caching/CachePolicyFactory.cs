﻿using Skeleton.Abstraction;
using System;
using System.Runtime.Caching;

namespace Skeleton.Core.Caching
{
    internal sealed class CachePolicyFactory
    {
        private readonly Action<ICacheConfiguration> _defaultCacheConfiguration =
            context => context.SetAbsoluteExpiration(TimeSpan.FromSeconds(300));

        private readonly MemoryCacheConfiguration _cacheContext =
            new MemoryCacheConfiguration
            {
                CreationTime = DateTimeOffset.UtcNow
            };

        internal CacheItemPolicy Create(Action<ICacheConfiguration> configurator)
        {
            if (configurator == null)
                configurator = _defaultCacheConfiguration;

            configurator(_cacheContext);

            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = _cacheContext.AbsoluteExpiration
                    .GetValueOrDefault(DateTimeOffset.MaxValue),
                SlidingExpiration = _cacheContext.SlidingExpiration
                    .GetValueOrDefault(TimeSpan.Zero)
            };

            return policy;
        }
    }
}