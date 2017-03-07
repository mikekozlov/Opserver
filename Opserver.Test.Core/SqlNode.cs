//ReSharper disable All

namespace Opserver.Test.Core
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    using Opserver.Test.Core.Models;

    class SqlNode : PollNode
    {
        public string ConnectionString { get; set; }

        public const int _refreshInterval = 3;

        //todo mk change to pass setting as a class 
        public SqlNode(string key, string connectionString)
            : base(key)
        {
            ConnectionString = connectionString;
        }

        public override IEnumerable<Cache> DataPollers
        {
            get
            {
                yield return Contacts;
                yield return Candidates;
            }
        }

        public Cache GetSqlCache<T>(string key, string description, Func<DbConnection,Task<T>> get, TimeSpan? cacheDuration) where T : class
        {
            //todo mk add miniprofiler later to monitor how long the sql operation

            return new Cache<T>(key, description,
                       async () =>
                           {
                               using (var conn = await GetConnectionAsync().ConfigureAwait(false))
                               {
                                   return await get(conn).ConfigureAwait(false);
                               }
                           }

                , cacheDuration ?? TimeSpan.FromSeconds(_refreshInterval));
        }

        protected Task<DbConnection> GetConnectionAsync(int timeout = 5000) => Connection.GetOpenAsync(ConnectionString, connectionTimeout: timeout);

        public Cache<IEnumerable<Candidate>> Candidates { get; set; }

        public Cache<IEnumerable<Contact>> Contacts { get; set; }
    }
}