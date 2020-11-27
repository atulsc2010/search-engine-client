using MediatR;
using System.Collections.Generic;

namespace TestProject.WebAPI.Queries.Accounts
{
    public class ListAccountsQuery : IRequest<IEnumerable<ListAccountsResponse>>
    {

    }
}