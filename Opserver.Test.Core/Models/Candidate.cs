//ReSharper disable All

using System;

namespace Opserver.Test.Core.Models
{
    public class Candidate
    {
        public long Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
