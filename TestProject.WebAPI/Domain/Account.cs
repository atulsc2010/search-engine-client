using System;

namespace TestProject.WebAPI.Domain
{
    public class Account
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public AccountStatus Status { get; set; }
        public decimal Balance { get; set; }
        public Guid UserId { get; set; }
    }
}
