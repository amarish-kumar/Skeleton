﻿using Skeleton.Abstraction.Dependency;
using Skeleton.Core;
using System.Diagnostics;

namespace Skeleton.Infrastructure.Dependency.Configuration
{
    [DebuggerDisplay("DatabaseName = {Name")]
    public sealed class DatabaseConfiguration :
        HideObjectMethodsBase,
        IDatabaseConfiguration
    {
        private const int DefaultTimeout = 300;
        private const int DefaultRetryCount = 10;
        private const int DefaultRetryInterval = 1;
        private const string DefaultProvider = "System.Data.SqlClient";
        private const string DefaultName = "Default";

        private int _commandTimeout;
        private int _retryCount;
        private int _retryInterval;
        private string _providerName;
        private string _name;

        public int CommandTimeout
        {
            get { return _commandTimeout; }
            set { _commandTimeout = value == 0 ? DefaultTimeout : value; }
        }

        public string ConnectionString { get; set; }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name.IsNullOrEmpty() || value.IsNullOrEmpty())
                    _name = DefaultName;
                else
                    _name = value;
            }
        }

        public string ProviderName
        {
            get
            {
                return _providerName;
            }
            set
            {
                if (_providerName.IsNullOrEmpty() || value.IsNullOrEmpty())
                    _providerName = DefaultProvider;
                else
                    _providerName = value;
            }
        }

        public int RetryCount
        {
            get { return _retryCount; }
            set { _retryCount = value == 0 ? DefaultRetryCount : value; }
        }

        public int RetryInterval
        {
            get { return _retryInterval; }
            set { _retryInterval = value == 0 ? DefaultRetryInterval : value; }
        }
    }
}