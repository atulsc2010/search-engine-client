using System;
using System.Threading;
using System.Threading.Tasks;
using TestProject.WebAPI.Handlers;
using TestProject.WebAPI.Queries.Users;
using FluentAssertions;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TestProject.WebAPI.Models;
using TestProject.WebAPI.Domain;
using System.Collections.Generic;
using TestProject.WebAPI.Commands.Users;
using TestProject.WebAPI.Commands.Accounts;
using TestProject.WebAPI.Queries.Accounts;

namespace TestProject.Tests
{
    public class UnitTests
    {
        [Fact]
        public async Task GetUserHandler_returns_a_user_when_available()
        {

            //Arrange
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            GetUserResponse response;

            using (var db = MockDbContext())
            {
                db.Users.RemoveRange(db.Users);
                db.Users.Add(new User { Id = userId1, Name = "Tester1", Email = "email1@email.com" });
                db.Users.Add(new User { Id = userId2, Name = "Tester2", Email = "email2@email.com" });
                db.SaveChanges();
            }

            using (var db = MockDbContext())
            {
                // run your test here
                var request = new GetUserQuery(userId1);
                var handler = new GetUserQueryHandler(db);

                //Act
                response = await handler.Handle(request, CancellationToken.None);
            }

            //Assert
            response.Id.ToString().Should().Be(userId1.ToString());

        }

        [Fact]
        public async Task GetUserQuery_does_not_return_a_user_when_not_available()
        {

            //Arrange
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            GetUserResponse response;

            using (var db = MockDbContext())
            {
                db.Users.RemoveRange(db.Users);
                db.Users.Add(new User { Id = userId1, Name = "Tester1", Email = "email1@email.com" });
                db.Users.Add(new User { Id = userId2, Name = "Tester2", Email = "email2@email.com" });
                db.SaveChanges();
            }

            using (var db = MockDbContext())
            {
                // run your test here
                var request = new GetUserQuery(Guid.NewGuid());
                var handler = new GetUserQueryHandler(db);

                //Act
                response = await handler.Handle(request, CancellationToken.None);
            }

            //Assert
            response.Should().Be(null);

        }

        [Fact]
        public async Task ListUsersQuery_returns_a_list_of_users_when_available()
        {
            //Arrange
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var userId3 = Guid.NewGuid();
            IEnumerable<ListUsersResponse> response;

            using (var db = MockDbContext())
            {
                db.Users.RemoveRange(db.Users);
                db.Users.Add(new User { Id = userId1, Name = "Tester1", Email = "email1@email.com" });
                db.Users.Add(new User { Id = userId2, Name = "Tester2", Email = "email2@email.com" });
                db.Users.Add(new User { Id = userId3, Name = "Tester3", Email = "email3@email.com" });
                db.SaveChanges();
            }

            using (var db = MockDbContext())
            {
                var request = new ListUsersQuery();
                var handler = new ListUsersQueryHandler(db);

                //Act
                response = await handler.Handle(request, CancellationToken.None);
            }

            //Assert
            response.Count().Should().Be(3);
        }

        [Fact]
        public async Task ListUsersQuery_returns_a_blanklist_when_table_is_empty()
        {
            //Arrange
            IEnumerable<ListUsersResponse> response;

            using (var db = MockDbContext())
            {
                db.Users.RemoveRange(db.Users);
                db.SaveChanges();

                var request = new ListUsersQuery();
                var handler = new ListUsersQueryHandler(db);

                //Act
                response = await handler.Handle(request, CancellationToken.None);
            }

            //Assert
            response.Count().Should().Be(0);
        }

        [Fact]
        public async Task CreateUserCommand_creates_user_when_request_is_valid()
        {
            //Arrange
            var request = new CreateUserRequest { Name = "test", Email = "test@email.com", Salary = 2000, Expenses = 500 };
            var response = new CreateUserResponse();

            using (var db = MockDbContext())
            {
                db.Users.RemoveRange(db.Users);
                db.SaveChanges();

                var command = new CreateUserCommand(request);
                var handler = new CreateUserCommandHandler(db);

                //Act
                response = await handler.Handle(command, CancellationToken.None);
            }

            //Assert
            response.Status.Should().Be("Success");
            response.Id.Should().NotBeEmpty();
        }


        [Fact]
        public async Task CreateUserCommand_DoesNot_create_user_when_Salary_income_is_less_than_zero()
        {
            //Arrange
            var request = new CreateUserRequest { Name = "test", Email = "test@email.com", Salary = -2000, Expenses = 2000 };
            var response = new CreateUserResponse();

            using (var db = MockDbContext())
            {
                db.Users.RemoveRange(db.Users);
                db.SaveChanges();

                var command = new CreateUserCommand(request);
                var handler = new CreateUserCommandHandler(db);

                //Act
                response = await handler.Handle(command, CancellationToken.None);
            }

            //Assert
            response.Status.Should().Contain("User cannot be created");
            response.Id.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateUserCommand_DoesNot_create_user_when_name_or_email_not_provided()
        {
            //Arrange
            var request = new CreateUserRequest { Name = "test", Email = "", Salary = 2000, Expenses = 2000 };
            var response = new CreateUserResponse();

            using (var db = MockDbContext())
            {
                db.Users.RemoveRange(db.Users);
                db.SaveChanges();

                var command = new CreateUserCommand(request);
                var handler = new CreateUserCommandHandler(db);

                //Act
                response = await handler.Handle(command, CancellationToken.None);
            }

            //Assert
            response.Status.Should().Contain("User cannot be created");
            response.Id.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateUserCommand_DoesNot_create_user_when_email_exists_in_users_db()
        {
            //Arrange
            var request = new CreateUserRequest { Name = "test", Email = "test@email.com", Salary = 2000, Expenses = 2000 };
            var response = new CreateUserResponse();

            using (var db = MockDbContext())
            {
                db.Users.RemoveRange(db.Users);
                db.SaveChanges();

                var command = new CreateUserCommand(request);
                var handler = new CreateUserCommandHandler(db);

                //Act
                response = await handler.Handle(command, CancellationToken.None);
            }

            //Assert
            response.Status.Should().Contain("Success");
            response.Id.Should().NotBeEmpty();

            //Arrange 2
            request = new CreateUserRequest { Name = "new user", Email = "test@email.com", Salary = 3000, Expenses = 500 };
            response = new CreateUserResponse();

            using (var db = MockDbContext())
            {
                var command = new CreateUserCommand(request);
                var handler = new CreateUserCommandHandler(db);

                //Act
                response = await handler.Handle(command, CancellationToken.None);
            }

            //Assert
            response.Status.Should().Contain("User cannot be created");
            response.Id.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateAccountCommand_creates_account_when_request_is_valid_and_User_exists()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var request = new CreateAccountRequest { UserId = userId };
            var response = new CreateAccountResponse();

            using (var db = MockDbContext())
            {
                db.Users.RemoveRange(db.Users);
                db.SaveChanges();

                db.Users.Add(new User { Id = userId, Name = "Tester1", Email = "email1@email.com", Salary = 2000 , Expenses = 1000 });
                db.SaveChanges();


                var command = new CreateAccountCommand(request);
                var handler = new CreateAccountCommandHandler(db);

                //Act
                response = await handler.Handle(command, CancellationToken.None);
            }

            //Assert
            response.Status.Should().Be("Success");
            response.Id.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CreateAccountCommand_DoesNot_create_account_when_User_notfound()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var request = new CreateAccountRequest { UserId = userId };
            var response = new CreateAccountResponse();

            using (var db = MockDbContext())
            {
                db.Users.RemoveRange(db.Users);
                db.SaveChanges();

                var command = new CreateAccountCommand(request);
                var handler = new CreateAccountCommandHandler(db);

                //Act
                response = await handler.Handle(command, CancellationToken.None);
            }

            //Assert
            response.Status.Should().Contain("not be created");
            response.Id.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateAccountCommand_DoesNot_create_account_when_Income_threshold_is_not_met()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var request = new CreateAccountRequest { UserId = userId };
            var response = new CreateAccountResponse();

            using (var db = MockDbContext())
            {
                db.Users.RemoveRange(db.Users);
                db.SaveChanges();

                db.Users.Add(new User { Id = userId, Name = "Tester1", Email = "email1@email.com", Salary = 2000, Expenses = 1500 });
                db.SaveChanges();

                var command = new CreateAccountCommand(request);
                var handler = new CreateAccountCommandHandler(db);

                //Act
                response = await handler.Handle(command, CancellationToken.None);
            }

            //Assert
            response.Status.Should().Contain("Income");
            response.Id.Should().BeEmpty();
        }


        [Fact]
        public async Task ListAccountsQuery_returns_a_list_of_accounts_when_available()
        {
            //Arrange

            IEnumerable<ListAccountsResponse> response;

            using (var db = MockDbContext())
            {
                db.Accounts.RemoveRange(db.Accounts);
                db.Accounts.Add(new Account { Id = Guid.NewGuid(), Name = "account1", Status = AccountStatus.Active });
                db.Accounts.Add(new Account { Id = Guid.NewGuid(), Name = "account2", Status = AccountStatus.Active });
                db.Accounts.Add(new Account { Id = Guid.NewGuid(), Name = "account3", Status = AccountStatus.Active });
                db.SaveChanges();
            }

            using (var db = MockDbContext())
            {
                var request = new ListAccountsQuery();
                var handler = new ListAccountsQueryHandler(db);

                //Act
                response = await handler.Handle(request, CancellationToken.None);
            }

            //Assert
            response.Count().Should().Be(3);
        }


        private static ApiDbContext MockDbContext()
        {

            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemory")
                .Options;

            return new ApiDbContext(options);

        }
    }
}