using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using TestProject.WebAPI.Models;
using TestProject.WebAPI.Queries.Users;

namespace TestProject.WebAPI.Handlers
{
    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, GetUserResponse>
    {
        private readonly ApiDbContext _db;

        public GetUserQueryHandler(ApiDbContext db)
        {
            _db = db;
        }

        public async Task<GetUserResponse> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _db.Users.Include(u => u.Accounts).FirstOrDefaultAsync(u => u.Id == request.Id);

            if (user != null)
            {
                return new GetUserResponse
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Salary = user.Salary,
                    Expenses = user.Expenses,
                    Accounts = user.Accounts
                };
            }
            else
            {
                return null;
            }

        }
    }
}
