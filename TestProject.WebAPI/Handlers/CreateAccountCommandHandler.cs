using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using TestProject.WebAPI.Commands.Accounts;
using TestProject.WebAPI.Domain;
using TestProject.WebAPI.Models;

namespace TestProject.WebAPI.Handlers
{
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, CreateAccountResponse>
    {
        private readonly ApiDbContext _db;

        public CreateAccountCommandHandler(ApiDbContext db)
        {
            _db = db;
        }

        public async Task<CreateAccountResponse> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = request.Request.UserId;
                var account = new Account { };

                if (userId != null || userId != Guid.Empty)
                {
                    var user = await _db.Users.FindAsync(userId);

                    if (user == null)
                    {
                        return new CreateAccountResponse { Id = Guid.Empty, Status = "User not found, Account could not be created" };
                    }

                    if (user.IsIncomeThresholdMet())
                    {
                        account = new Account
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            Balance = 0,
                            Name = $"Wallet {DateTime.Now:fffffff}",
                            Status = AccountStatus.Active
                        };
                    }
                    else
                    {
                        return new CreateAccountResponse { Id = Guid.Empty, Status = "Income Threshold not met, Account could not be created" };
                    }

                }

                _db.Accounts.Add(account);
                await _db.SaveChangesAsync();

                return await Task.FromResult(new CreateAccountResponse { Id = account.Id, Status = "Success" });

            }
            catch (Exception e)
            {
                throw e;
            }

        }
    }
}
