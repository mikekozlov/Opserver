//ReSharper disable All

namespace Opserver.Test.Core.Models
{
    using System;

    public class Contact
    {
        public long Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public DateTime DateCreated { get; set; }
    }
}