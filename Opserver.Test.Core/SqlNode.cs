//ReSharper disable All

namespace Opserver.Test.Core
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    using Dapper;

    using Opserver.Test.Core.Models;

    using StackExchange.Opserver.Helpers;

    public class SqlNode : PollNode
    {
        private Cache<IEnumerable<Contact>> _contacts;

        private Cache<IEnumerable<Candidate>> _candidates;


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

        public Cache<T> GetSqlCache<T>(string key, string description, Func<DbConnection,Task<T>> get, TimeSpan? cacheDuration) where T : class
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

        public Cache<IEnumerable<Candidate>> Candidates
            => _candidates?? ( _candidates = GetSqlCache(
                   nameof(Candidate),
                   "Fetch Candidates",
                   async connection =>
                       {
                           IEnumerable<Candidate> candidates =
                               await
                                   SqlMapper.QueryAsync<Candidate>(
                                       connection,
                                       "SELECT distinct top 10\r\n\tContactId as Id, \r\n\tEmail, \r\n\tLastName as FullName\r\n FROM AS_Gold_Stage.JobDiva.Contact");
                           return candidates;
                       },
                   ExtensionMethods.Seconds(5)));

        public Cache<IEnumerable<Contact>> Contacts
            => _contacts?? ( _contacts = GetSqlCache(
                   nameof(Contact),
                   "Fetch Contacts",
                   async connection =>
                   {
                       IEnumerable<Contact> contacts =
                           await
                               SqlMapper.QueryAsync<Contact>(
                                   connection,
                                   "SELECT distinct top 10\r\n\tCandidateId as Id, \r\n\tEmail, \r\n\tLastName as FullName\r\n FROM AS_Gold_Stage.JobDiva.Candidate");
                       return contacts;
                   },
                   ExtensionMethods.Seconds(5)));
    }
}