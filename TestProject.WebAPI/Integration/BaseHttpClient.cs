using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TestProject.WebAPI.Integration
{
    public class BaseHttpClient : IBaseHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BaseHttpClient> _logger;

        public BaseHttpClient(
            HttpClient httpClient,
            ILogger<BaseHttpClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

        }

        public async Task<HttpResponseMessage> GetAsync(string uri)
        {
            var response = new HttpResponseMessage();

            try
            {
                response = await _httpClient.GetAsync(uri);
            }
            catch (Exception ex)
            {

                _logger.LogInformation($"{response.ReasonPhrase} {response.StatusCode}");
                throw ex;
            }

            return response;
        }

        public async Task<string> GetStringAsync(string uri)
        {
            string response;

            try
            {
                response = await _httpClient.GetStringAsync(uri);
            }
            catch (Exception ex)
            {                               
                throw ex;
            }

            return response;
        }
    }
}
