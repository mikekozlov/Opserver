using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opserver.Test.Console
{
    using System.Collections;
    using System.Data.Common;
    using System.Threading;

    using Opserver.Test.Core;
    using Opserver.Test.Core.Models;

    using StackExchange.Opserver.Helpers;

    using Console = System.Console;

    class Program
    {
        private static Func<DbConnection, Task<IEnumerable<Candidate>>> candidatesFiltersFunc = async connection =>
            {
                IEnumerable<Candidate> candidates =
                    await
                        connection.QueryAsync<Candidate>(
                            "SELECT distinct top 10\r\n\tCandidateId as Id, \r\n\tEmail, \r\n\tLastName as FullName\r\n FROM AS_Gold_Stage.JobDiva.Candidate WHERE CandidateId > 3490 and CandidateId <=8336");
                return candidates;
            };

        static void Main(string[] args)
        {
            var connectionString =
                "Data Source=108.161.134.142,60050;User ID=AutoOrcha;Password=@ut()0Rca;Initial Catalog=AS_Gold_Stage";

            //todo chek how they create nodes;
            var sqlNode = new SqlNode("qa", connectionString);

            PollEngine.TryAdd(sqlNode);

            while (true)
            {
                Thread.Sleep(4000);

                // initial call will go to db directly.
                var c = sqlNode.Candidates.Data.First();
                Console.WriteLine($"Global Pollers Candidate: {c.FullName} [{c.Id}]");

                var cache = sqlNode.AddCachePoller(
                    nameof(Candidate) + "other key A",
                    "Get candidates with filters",
                    candidatesFiltersFunc,
                    10.Seconds(),
                    15);

                var d = cache.Data.First().FullName;

                Console.WriteLine($"Cache Pollers Candidate {cache.Key} {d}  Hit Count: {cache.HitCount}");

                var cache2 = sqlNode.AddCachePoller(
                    nameof(Candidate) + "other key AA",
                    "Get candidates with filters",
                    candidatesFiltersFunc,
                    20.Seconds(),
                    15);

                var d2 = cache2.Data.First().FullName;

                Console.WriteLine($"Cache Pollers Candidate 2 {cache2.Key} {d2} Hit Count: {cache2.HitCount}");

                // duration and hitcount can be moved as default in override method 
                // because of smart algo in cache item itself, it is fine to declare duration, hitcount with default values
                // wiht cqs approach call to sqlNode.GetSqlCacheExt will be done in query; the sql statement will be in query itself
                // business logic will be done in query

                //var cache3 = sqlNode.GetSqlCacheExt(
                //            nameof(Candidate) + "other key AAA",
                //            "Get candidates with filters",
                //            // here some manipulations , mapping and multiple queries can be done.
                //            async connection => await
                //                                    connection.QueryAsync<Candidate>(
                //                                        "SELECT distinct top 10\r\n\tCandidateId as Id, \r\n\tEmail, \r\n\tLastName as FullName\r\n FROM AS_Gold_Stage.JobDiva.Candidate WHERE CandidateId > 3490"),
                //            20.Seconds(),
                //            15);

                //var d3 = cache3.Data.Last().FullName;

                //Console.WriteLine($"Cache Pollers Candidate 2 {cache3.Key} {d3} Hit Count: {cache3.HitCount}");

                for (int i = 0; i < 100; i++)
                {
                    sqlNode.AddCachePoller(
                nameof(Candidate) + "A " + i,
                "Get candidates with filters",
                async connection => await
                                        connection.QueryAsync<Candidate>(
                                            "SELECT distinct top 10\r\n\tCandidateId as Id, \r\n\tEmail, \r\n\tLastName as FullName\r\n FROM AS_Gold_Stage.JobDiva.Candidate WHERE CandidateId > 3490"),
                            20.Seconds(),
                            15);
                }
            }
        }
    }
}
