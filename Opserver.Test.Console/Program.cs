﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opserver.Test.Console
{
    using System.Threading;

    using Opserver.Test.Core;

    using Console = System.Console;

    class Program
    {
        static void Main(string[] args)
        {
            var connectionString =
                "Data Source=108.161.134.142,60050;User ID=AutoOrcha;Password=@ut()0Rca;Initial Catalog=AS_Gold_Stage";

            //todo chek how they create nodes;
            var sqlNode = new SqlNode("qa", connectionString);

            PollEngine.TryAdd(sqlNode);

            Thread.Sleep(10000);

            var candidates = sqlNode.Candidates.Data;

            //Thread.Sleep(5000);

            Console.ReadKey();
        }
    }
}
