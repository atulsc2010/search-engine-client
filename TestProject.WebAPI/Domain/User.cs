using System;
using System.Collections.Generic;

namespace TestProject.WebAPI.Domain
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal Salary { get; set; }
        public decimal Expenses { get; set; }
        public IEnumerable<Account> Accounts { get; set; }

        public bool IsIncomeThresholdMet()
        {
            return Salary - Expenses >= 1000;
        }

    }
}
