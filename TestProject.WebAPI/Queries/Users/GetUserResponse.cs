using System;
using System.Collections.Generic;
using TestProject.WebAPI.Domain;

namespace TestProject.WebAPI.Queries.Users
{
    public class GetUserResponse
    {
        public string Type => GetType().Name;
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal Salary { get; set; }
        public decimal Expenses { get; set; }
        public IEnumerable<Account> Accounts { get; set; }
    }
}