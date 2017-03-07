//ReSharper disable All

namespace Opserver.Test.Core
{
    using System.Collections.Generic;

    using Opserver.Test.Core.Models;

    class SqlNode : PollNode
    {
        public string ConnectionString { get; set; }

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

        public Cache<IEnumerable<Candidate>> Candidates { get; set; }

        public Cache<IEnumerable<Contact>> Contacts { get; set; }
    }
}