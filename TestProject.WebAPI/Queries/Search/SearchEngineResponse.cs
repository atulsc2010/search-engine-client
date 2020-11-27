using System;
using System.Collections.Generic;

namespace TestProject.WebAPI.Queries.Search
{
    public class SearchEngineResponse
    {
        public List<int> Occurrences { get; set; }
        public List<string> Results { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool Cached { get; set; }
    }
}