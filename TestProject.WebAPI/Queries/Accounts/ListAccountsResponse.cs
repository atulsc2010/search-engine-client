using System;
using TestProject.WebAPI.Domain;

namespace TestProject.WebAPI.Queries.Accounts
{
    public class ListAccountsResponse
    {
        public string Type => GetType().Name;
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public AccountStatus Status { get; set; }
        public decimal Balance { get; set; }

    }
}
