using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject.WebAPI.Domain
{
    public class SearchResult
    {
        public Guid Id { get; set; }
        public string Request { get; set; }
        public string Result { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
