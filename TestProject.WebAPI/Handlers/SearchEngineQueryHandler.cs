using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using TestProject.WebAPI.Integration;
using TestProject.WebAPI.Models;
using TestProject.WebAPI.Queries.Search;
using TestProject.WebAPI.Domain;

namespace TestProject.WebAPI.Handlers
{
    public class SearchEngineQueryHandler : IRequestHandler<SearchEngineQuery, SearchEngineResponse>
    {
        private readonly ApiDbContext _db;
        private readonly SearchEngineClient _searchEngine;


        public SearchEngineQueryHandler(ApiDbContext db, SearchEngineClient searchEngine)
        {
            _db = db;
            _searchEngine = searchEngine;
        }
        public async Task<SearchEngineResponse> Handle(SearchEngineQuery request, CancellationToken cancellationToken)
        {

            var cachedResult = await GetCachedResult(request);

            if (cachedResult == null)
            {
                try
                {
                    var response = await _searchEngine.GetStringAsync(request.Engine, request.Keyword);
                    var result = ParseResponseHtml(request, response);
                    
                    await AddCachedResult(request, response);
                    return result;
                }
                catch (Exception ex)
                {
                    return new SearchEngineResponse
                    {
                        Results = new List<string> { ex.Message }
                    };
                }
            }
            else
            {
                var result = ParseResponseHtml(request, cachedResult);
                result.Cached = true;
                return result;
            }
        }

        private List<string> FindUris(SearchEngineQuery request, string response)
        {
            string pattern = @"https ?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)"; 

            if (request.Engine.ToLower().Contains("google")) 
            {
                pattern = $@"\/url\?q={pattern}";
            }
            else if (request.Engine.ToLower().Contains("bing"))
            {
                pattern = $@"<cite>{pattern}";
            }
            var uris = new List<string>();

            var m = Regex.Matches(response, pattern, RegexOptions.Multiline | RegexOptions.ECMAScript);

            Console.WriteLine("{0} matches found in: ", m.Count);

            foreach (Match s in m)
            {
                if (s.Success) 
                {
                    Console.WriteLine(s.Value);
                    uris.Add(s.Value);
                }
            }

            return uris;
        }

        private List<int> FindOccurrences(SearchEngineQuery request, List<string> uris) 
        {
            string find = request.Find;
            var occcurs = new List<int>();
            var index=0;

            foreach (var uri in uris)
            {
                index++;
                if (uri.ToLower().Contains(find.ToLower()))
                    occcurs.Add(index);
            }

            if (!occcurs.Any())
            {
                occcurs.Add(0);
            } 
            
            return occcurs;
        }

        private async Task<SearchResult> GetCachedResult(SearchEngineQuery request) 
        {
            try
            {
                var cache = await _db.SearchResults.FirstOrDefaultAsync(r => r.Request == $"{request.Engine}/{request.Keyword}/{request.Find}" && r.LastUpdated >= DateTime.Now.AddHours(-1));
                return cache;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        private async Task AddCachedResult(SearchEngineQuery request, SearchResult response)
        {
            try
            {
                response.Request = $"{request.Engine}/{request.Keyword}/{request.Find}";

                _db.SearchResults.Add(response);
                await _db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private SearchEngineResponse ParseResponseHtml(SearchEngineQuery request, SearchResult response)
        {
            var uris = FindUris(request, response.Result);
            var occurs = FindOccurrences(request, uris);

            return new SearchEngineResponse
            {
                Occurrences = occurs,
                Results = uris,
                LastUpdated = response.LastUpdated,
                Cached = false
            };

        }

    }

}


    
