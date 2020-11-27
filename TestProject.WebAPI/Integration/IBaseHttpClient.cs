using System.Net.Http;
using System.Threading.Tasks;

namespace TestProject.WebAPI.Integration
{
    public interface IBaseHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string uri);
        Task<string> GetStringAsync(string uri);
    }
}