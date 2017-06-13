﻿using Skeleton.Abstraction;
using Skeleton.Abstraction.Dependency;
using Skeleton.Infrastructure.Dependency.Configuration;
using Skeleton.Infrastructure.Logging;

namespace Skeleton.Infrastructure.Dependency.Plugins
{
    public sealed class LoggerPlugin : IPlugin
    {
        public void Configure(IDependencyContainer container)
        {
            var configuration = new LoggerConfiguration();
            configuration.Configure();
            (container as DependencyContainer).UnityContainer.AddExtension(new LoggerConstructorInjectionExtension());
            container.Register.Instance<ILoggerFactory>(new LoggerFactory());
        }
    }
}