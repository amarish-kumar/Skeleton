﻿using NUnit.Framework;
using Skeleton.Tests.Common;
using Skeleton.Tests.Web.Mock;
using Skeleton.Web.Client;
using System.Linq;
using System.Threading.Tasks;

namespace Skeleton.Tests.Web
{
    [TestFixture]
    public class HttpClientAsyncEntityWriterTests
    {
        private readonly static AsyncCrudHttpClient<CustomerDto> Client =
            new AsyncCrudHttpClient<CustomerDto>(AppConfiguration.AsyncCustomersUriBuilder);

        private readonly OwinServer _server = new OwinServer();

        [OneTimeSetUp]
        public void Init()
        {
            _server.Start(AppConfiguration.BaseUrl);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _server.Dispose();
        }

        [Test]
        public async Task AsyncEntityWriter_UpdateAsync()
        {
            var data = await Client.PageAsync(1, 1);
            var firstCustomer = data.Results.FirstOrDefault();

            Assert.IsNotNull(firstCustomer);

            var customer = new CustomerDto
            {
                CustomerId = firstCustomer.CustomerId,
                Name = "CustomerUpdated" + firstCustomer.CustomerId,
                CustomerCategoryId = firstCustomer.CustomerCategoryId
            };
            var result = await Client.UpdateAsync(customer);

            Assert.IsTrue(result);
        }

        [Test]
        public async Task AsyncEntityWriter_BatchUpdateAsync()
        {
            var customersPagedResults = await Client.PageAsync(5, 1);
            var customers = customersPagedResults.Results;

            Assert.IsNotNull(customers);

            foreach (var customer in customers)
                customer.Name = "CustomerUpdated" + customer.CustomerId;

            var result = await Client.UpdateAsync(customers);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task AsyncEntityWriter_CreateAsync()
        {
            var customer = new CustomerDto { Name = "Customer" };
            var result = await Client.CreateAsync(customer);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(CustomerDto), result);
        }

        [Test]
        public async Task AsyncEntityWriter_BatchCreateAsync()
        {
            var customers = MemorySeeder.SeedCustomerDtos(5).ToList();
            var results = await Client.CreateAsync(customers);

            Assert.IsNotNull(results);
            Assert.IsInstanceOf(typeof(CustomerDto), results.First());
        }

        [Test]
        public async Task AsyncEntityWriter_DeleteAsync()
        {
            var data = await Client.PageAsync(1, 1);
            var firstCustomer = data.Results.FirstOrDefault();
            Assert.IsNotNull(firstCustomer);
            var result = await Client.DeleteAsync(firstCustomer.CustomerId);

            Assert.IsTrue(result);
        }

        [Test]
        public async Task AsyncEntityWriter_BatchDeleteAsync()
        {
            var data = await Client.PageAsync(5, 1);
            var customers = data.Results;
            Assert.IsNotNull(customers);

            var result = await Client.DeleteAsync(customers);
            Assert.IsTrue(result);
        }
    }
}