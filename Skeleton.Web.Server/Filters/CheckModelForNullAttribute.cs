﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Skeleton.Common;

namespace Skeleton.Web.Server.Filters
{
        public sealed class CheckModelForNullAttribute : ActionFilterAttribute
    {
        private const string Error = "The argument cannot be null";
        private readonly Func<Dictionary<string, object>, bool> _checkCondition;

        public CheckModelForNullAttribute()
            : this(arguments => arguments.ContainsValue(null))
        {
        }

        public CheckModelForNullAttribute(Func<Dictionary<string, object>, bool> checkCondition)
        {
            _checkCondition = checkCondition;
        }

        public Func<Dictionary<string, object>, bool> CheckCondition
        {
            get { return _checkCondition; }
        }


        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            actionContext.ThrowIfNull(() => actionContext);

            if (CheckCondition(actionContext.ActionArguments))
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, Error);
        }
    }
}