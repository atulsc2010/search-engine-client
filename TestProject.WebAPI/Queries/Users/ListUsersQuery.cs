using MediatR;
using System.Collections.Generic;

namespace TestProject.WebAPI.Queries.Users
{
    public class ListUsersQuery : IRequest<IEnumerable<ListUsersResponse>>
    {

    }
}
