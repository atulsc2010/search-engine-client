using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TestProject.WebAPI.Commands.Accounts;
using TestProject.WebAPI.Queries.Accounts;
using TestProject.WebAPI.Queries.Search;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TestProject.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : BaseController
    {
        [HttpGet("{engine}/{keyword}/{find}")]
        public async Task<IActionResult> Get([FromRoute] string engine, [FromRoute] string keyword, [FromRoute] string find)
        {
            var query = new SearchEngineQuery(engine,keyword,find);
            var response = await Mediator.Send(query);

            return Ok(response);
        }

        //[HttpPost("create")]
        //public async Task<IActionResult> Create([FromBody] CreateAccountRequest accountRequest)
        //{
        //    var command = new CreateAccountCommand(accountRequest);
        //    var response = await Mediator.Send(command);

        //    if (response.Status == "Success")
        //    {
        //        return Created("/accounts/create", response);
        //    }
        //    else
        //    {
        //        return BadRequest(response);
        //    }

        //}

    }
}
