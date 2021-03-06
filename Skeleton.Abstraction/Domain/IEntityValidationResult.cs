﻿using System.Collections.Generic;

namespace Skeleton.Abstraction.Domain
{
    public interface IEntityValidationResult : IHideObjectMethods
    {
        IEnumerable<IValidationRule> BrokenRules { get; }

        bool IsValid { get; }
    }
}