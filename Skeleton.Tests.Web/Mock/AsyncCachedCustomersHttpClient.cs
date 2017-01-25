﻿using Skeleton.Tests.Common;
using Skeleton.Web.Client;

namespace Skeleton.Tests.Web.Mock
{
    public class AsyncCachedCustomersHttpClient : AsyncCrudHttpClient<CustomerDto>
    {
        public AsyncCachedCustomersHttpClient()
            : base("http://localhost:8081/", "api/asynccachedcustomers")
        {
        }
    }
}