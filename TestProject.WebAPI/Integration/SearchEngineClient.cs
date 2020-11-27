using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using TestProject.WebAPI.Domain;

namespace TestProject.WebAPI.Integration
{
    public class SearchEngineClient
    {
        private readonly IBaseHttpClient _baseHttpClient;

        public SearchEngineClient(IBaseHttpClient baseHttpClient)
        {
            _baseHttpClient = baseHttpClient;
        }


        public virtual async Task<SearchResult> GetStringAsync(string engine, string keyWord, int pages=10)
        {
            var engineUri = "https://www.google.com";
            string queryString;
            int nextPage = -10;
            string response = string.Empty;
             

            for (var i = 0; i <= pages; i++) 
            {
                nextPage += 10;

                if (engine.ToLower().Contains("google"))
                {
                    
                    engineUri = $"https://www.google.com.au";
                    queryString = $"search?q={keyWord}&start={nextPage}";
                }
                else if (engine.ToLower().Contains("bing"))
                {

                    engineUri = $"https://www.bing.com";
                    queryString = $"search?q={keyWord}&first={nextPage}";
                }
                else
                {
                    queryString = $"search?q={keyWord}&start={nextPage}";
                }

                string searchUri = $"{engineUri}/{queryString}";
                Console.WriteLine($"Searching : { searchUri}");

                string page = await _baseHttpClient.GetStringAsync(searchUri);
                response = string.Join(string.Empty, response, page);
            }

            return new SearchResult
            {
                Id = Guid.NewGuid(),
                Result = response,
                LastUpdated = DateTime.Now
            };
        }

        public async Task<SearchResult> GetAsync(string engine, string keyWord)
        {
            var engineUri = "https://www.google.com";
            string queryString;

            if (engine.ToLower().Contains("google"))
            {
                engineUri = $"https://www.google.com.au";
                queryString = $"search?q={keyWord}";
            }
            else
            {
                queryString = $"search?q={keyWord}";
            }

            string searchUri = $"{engineUri}/{queryString}";

            var response = await _baseHttpClient.GetAsync(searchUri);
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<SearchResult>(content);
        }
    }
}
