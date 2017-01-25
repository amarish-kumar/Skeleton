﻿using Skeleton.Tests.Common;
using Skeleton.Web.Client;

namespace Skeleton.Tests.Web.Mock
{
    public class AsyncCustomersHttpClient : AsyncCrudHttpClient<CustomerDto>
    {
        public AsyncCustomersHttpClient()
            : base("http://localhost:8081/", "api/asynccustomers")
        {
        }
    }
}