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
                            "SELECT distinct top 10\r\n\tCandidateId as Id, \r\n\tEmail, \r\n\tLastName as FullName\r\n FROM AS_Gold_Stage.JobDiva.Candidate WHERE CandidateId > 3490");
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
                Thread.Sleep(3000);
                var c = sqlNode.Candidates.Data.First();

                Console.WriteLine($"Global Pollers Candidate: {c.FullName} [{c.Id}]");

                var candidatesF = sqlNode.GetSqlCacheExt(
                    nameof(Candidate) + "other key",
                    "Get candidates with filters",
                    candidatesFiltersFunc,
                    10.Seconds(),
                    15);

                var c2 = candidatesF.Data.First();
                Console.WriteLine($"Cache Pollers Candidate: {c2.FullName} [{c2.Id}]");
                Console.WriteLine($"Cache Pollers Candidate Hit Count: {candidatesF.HitCount}");
            }
        }
    }
}
