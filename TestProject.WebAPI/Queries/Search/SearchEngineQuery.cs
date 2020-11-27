using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject.WebAPI.Queries.Search
{
    public class SearchEngineQuery : IRequest<SearchEngineResponse>
    {
        public string Engine { get; set; }
        public string Keyword { get; set; }
        public string Find { get; set; }

        public SearchEngineQuery(string engine, string keyword, string find)
        {
            Engine = engine;
            Keyword = keyword;
            Find = find;
        }
    }

}
