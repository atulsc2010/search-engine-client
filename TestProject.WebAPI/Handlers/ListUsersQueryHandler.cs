using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestProject.WebAPI.Models;
using TestProject.WebAPI.Queries.Users;

namespace TestProject.WebAPI.Handlers
{
    public class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, IEnumerable<ListUsersResponse>>
    {
        private readonly ApiDbContext _db;

        public ListUsersQueryHandler(ApiDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ListUsersResponse>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _db.Users
                                 .Select(u => new ListUsersResponse { Id = u.Id, Name = u.Name, Email = u.Email })
                                 .ToListAsync();

            if (users.Any())
            {
                return users;
            }
            else
            {
                return new List<ListUsersResponse>();
            }
        }
    }
}
