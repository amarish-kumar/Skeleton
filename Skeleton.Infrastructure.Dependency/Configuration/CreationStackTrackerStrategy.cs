﻿using Microsoft.Practices.ObjectBuilder2;
using Skeleton.Core;

namespace Skeleton.Infrastructure.Dependency.Configuration
{
    public sealed class CreationStackTrackerStrategy : BuilderStrategy
    {
        public override void PostBuildUp(IBuilderContext context)
        {
            context.ThrowIfNull();

            var policy = context.Policies.Get<ICreationStackTrackerPolicy>(null, true);

            if (policy.TypeStack.Count > 0)
                policy.TypeStack.Pop();

            base.PostBuildUp(context);
        }

        public override void PreBuildUp(IBuilderContext context)
        {
            context.ThrowIfNull();

            var policy = context.Policies.Get<ICreationStackTrackerPolicy>(null, true);

            if (policy == null)
            {
                context.Policies.Set(typeof(ICreationStackTrackerPolicy), new CreationStackTrackerPolicy(), null);
                policy = context.Policies.Get<ICreationStackTrackerPolicy>(null, true);
            }

            policy.TypeStack.Push(context.BuildKey.Type);

            base.PreBuildUp(context);
        }
    }
}