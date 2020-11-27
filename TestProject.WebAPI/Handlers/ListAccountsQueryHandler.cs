using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestProject.WebAPI.Models;
using TestProject.WebAPI.Queries.Accounts;

namespace TestProject.WebAPI.Handlers
{
    public class ListAccountsQueryHandler : IRequestHandler<ListAccountsQuery, IEnumerable<ListAccountsResponse>>
    {
        private readonly ApiDbContext _db;

        public ListAccountsQueryHandler(ApiDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ListAccountsResponse>> Handle(ListAccountsQuery request, CancellationToken cancellationToken)
        {
            var accounts = await _db.Accounts
                                 .Select(a => new ListAccountsResponse { Id = a.Id, UserId = a.UserId, Name = a.Name, Status = a.Status, Balance = a.Balance })
                                 .ToListAsync();

            if (accounts.Any())
            {
                return accounts;
            }
            else
            {
                return new List<ListAccountsResponse>();
            }
        }

    }
}
