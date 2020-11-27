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
using TestProject.WebAPI.Queries.Search;
using Moq;
using System.Net.Http;
using System.Net;
using Moq.Protected;
using TestProject.WebAPI.Integration;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestProject.Tests
{
    public class UnitTests
    {

        [Fact]
        public async Task SearchQueryHandler_returns_cached_result_when_available()
        {

            //Arrange
            var Id1 = Guid.NewGuid();
            var Id2 = Guid.NewGuid();
            SearchEngineResponse response;

            using (var db = MockDbContext())
            {
                db.SearchResults.RemoveRange(db.SearchResults);
                db.SearchResults.Add(new SearchResult { Id = Id1, Request = "google/e-settlements/sympli.com.au", Result = SearchTestData.googleResult, LastUpdated = DateTime.Now.AddMinutes(-5) });
                db.SearchResults.Add(new SearchResult { Id = Id2, Request = "bing/e-settlements/sympli.com.au", Result = SearchTestData.bingResult, LastUpdated = DateTime.Now.AddMinutes(-5) });
                db.SaveChanges();
            }

            using (var db = MockDbContext())
            {
                // run your test here
                var request = new SearchEngineQuery("google", "e-settlements", "sympli.com.au");
                var handler = new SearchEngineQueryHandler(db,null);

                //Act
                response = await handler.Handle(request, CancellationToken.None);
            }

            //Assert
            response.Cached.Should().Be(true);
            response.Occurrences.Should().NotBeEmpty()
                .And.ContainInOrder(new List<int> { 4, 5, 6, 7, 8 });
        }

        [Fact]
        public async Task SearchQueryHandler_does_not_return_cached_result_when_older_than_one_hour()
        {

            //Arrange
            var Id1 = Guid.NewGuid();
            var Id2 = Guid.NewGuid();
            SearchEngineResponse response;

            using (var db = MockDbContext())
            {
                db.SearchResults.RemoveRange(db.SearchResults);
                db.SearchResults.Add(new SearchResult { Id = Id1, Request = "google/e-settlements/sympli.com.au", Result = SearchTestData.googleResult, LastUpdated = DateTime.Now.AddHours(-2) });
                db.SearchResults.Add(new SearchResult { Id = Id2, Request = "bing/e-settlements/sympli.com.au", Result = SearchTestData.bingResult, LastUpdated = DateTime.Now.AddHours(-2) });
                db.SaveChanges();
            }

            using (var db = MockDbContext())
            {
                // run your test here
                var request = new SearchEngineQuery("google", "e-settlements", "sympli.com.au");
                var handler = new SearchEngineQueryHandler(db, null);

                //Act
                response = await handler.Handle(request, CancellationToken.None);
            }

            //Assert
            response.Cached.Should().Be(false);
            response.Occurrences.Should().BeNull(); 
        }



        [Fact]
        public async void SearchQueryHandler_returns_latest_result_when_not_in_cache()
        {
            SearchEngineResponse response;

            //Mock the http Client
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpresponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(""),
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpresponse);

            var httpClient = new HttpClient(handlerMock.Object);
            var client = new BaseHttpClient(httpClient, NullLogger<BaseHttpClient>.Instance);

            //Mock the http Search Engine Client
            var mockSearch = new Mock<SearchEngineClient>(client);
            var searchresult = new SearchResult
            {
                Id = Guid.NewGuid(),
                Request = "google/e-settlements/sympli.com.au",
                Result = SearchTestData.googleResult,
                LastUpdated = DateTime.Now
            };

            mockSearch.Setup(x => x.GetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(searchresult);

            using (var db = MockDbContext())
            {
                // empty the cache
                db.SearchResults.RemoveRange(db.SearchResults);
                db.SaveChanges();
            }

            using (var db = MockDbContext())
            {
                // run your test here
                var request = new SearchEngineQuery("google", "e-settlements", "sympli.com.au");
                var handler = new SearchEngineQueryHandler(db, mockSearch.Object);

                //Act
                response = await handler.Handle(request, CancellationToken.None);
            }

            //Assert 
            response.Cached.Should().Be(false);
            response.Occurrences.Should().NotBeEmpty()
                .And.ContainInOrder(new List<int> { 4, 5, 6, 7, 8 });
        }

        [Fact]
        public async void SearchQueryHandler_returns_latest_result_with_alternate_searchengine()
        {
            SearchEngineResponse response;

            //Mock the http Client
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpresponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(""),
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpresponse);

            var httpClient = new HttpClient(handlerMock.Object);
            var client = new BaseHttpClient(httpClient, NullLogger<BaseHttpClient>.Instance);

            //Mock the http Search Engine Client
            var mockSearch = new Mock<SearchEngineClient>(client);
            var searchresult = new SearchResult
            {
                Id = Guid.NewGuid(),
                Request = "bing/e-settlements/sympli.com.au",
                Result = SearchTestData.bingResult,
                LastUpdated = DateTime.Now
            };

            mockSearch.Setup(x => x.GetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(searchresult);

            using (var db = MockDbContext())
            {
                // empty the cache
                db.SearchResults.RemoveRange(db.SearchResults);
                db.SaveChanges();
            }

            using (var db = MockDbContext())
            {
                // run your test here
                var request = new SearchEngineQuery("bing", "e-settlements", "sympli.com.au");
                var handler = new SearchEngineQueryHandler(db, mockSearch.Object);

                //Act
                response = await handler.Handle(request, CancellationToken.None);
            }

            //Assert 
            response.Cached.Should().Be(false);
            response.Occurrences.Should().NotBeEmpty()
                .And.ContainInOrder(new List<int> { 2 , 3 });
        }


        [Fact]
        public async void SearchQueryHandler_returns_zero_occurence_when_find_url_not_in_search_result()
        {
            SearchEngineResponse response;

            //Mock the http Client
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpresponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(""),
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpresponse);

            var httpClient = new HttpClient(handlerMock.Object);
            var client = new BaseHttpClient(httpClient, NullLogger<BaseHttpClient>.Instance);

            //Mock the http Search Engine Client
            var mockSearch = new Mock<SearchEngineClient>(client);
            var searchresult = new SearchResult
            {
                Id = Guid.NewGuid(),
                Request = "google/music/sympli.com.au",
                Result = SearchTestData.musicResult,
                LastUpdated = DateTime.Now
            };

            mockSearch.Setup(x => x.GetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(searchresult);

            using (var db = MockDbContext())
            {
                // empty the cache
                db.SearchResults.RemoveRange(db.SearchResults);
                db.SaveChanges();
            }

            using (var db = MockDbContext())
            {
                // run your test here
                var request = new SearchEngineQuery("google", "music", "sympli.com.au");
                var handler = new SearchEngineQueryHandler(db, mockSearch.Object);

                //Act
                response = await handler.Handle(request, CancellationToken.None);
            }

            //Assert 
            response.Cached.Should().Be(false);
            response.Occurrences.Should().NotBeEmpty()
                .And.ContainInOrder(new List<int> { 0 });
        }


        [Fact]
        public async void SearchQueryHandler_returns_zero_occurence_when_find_string_not_in_search_result()
        {
            SearchEngineResponse response;

            //Mock the http Client
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpresponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(""),
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpresponse);

            var httpClient = new HttpClient(handlerMock.Object);
            var client = new BaseHttpClient(httpClient, NullLogger<BaseHttpClient>.Instance);

            //Mock the http Search Engine Client
            var mockSearch = new Mock<SearchEngineClient>(client);
            var searchresult = new SearchResult
            {
                Id = Guid.NewGuid(),
                Request = "google/e-settlements/sympli.com.au",
                Result = SearchTestData.bingResult,
                LastUpdated = DateTime.Now
            };

            mockSearch.Setup(x => x.GetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(searchresult);

            using (var db = MockDbContext())
            {
                // empty the cache
                db.SearchResults.RemoveRange(db.SearchResults);
                db.SaveChanges();
            }

            using (var db = MockDbContext())
            {
                // run your test here
                var request = new SearchEngineQuery("google", "music", "sympli.com.au");
                var handler = new SearchEngineQueryHandler(db, mockSearch.Object);

                //Act
                response = await handler.Handle(request, CancellationToken.None);
            }

            //Assert 
            response.Cached.Should().Be(false);
            response.Occurrences.Should().NotBeEmpty()
                .And.ContainInOrder(new List<int> { 0 });
        }



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

    public static class SearchTestData 
    {
        public static string googleResult = @"/url?q=https://www.trisearch.com.au/the-5-things-you-need-to-know-about-electronic-settlements/&amp

            /url?q=https://www.infotrack.com.au/resources/in-the-media/media-releases/a-game-changer-in-e-settlements-is-closer-than-you-think/&amp

            /url?q=https://www.infotrack.com.au/news-and-insights/the-evolution-of-e-settlements/&amp

            /url?q=https://www.sympli.com.au/&amp

            /url?q=https://www.sympli.com.au/e-settlement-services/&amp

            /url?q=https://www.sympli.com.au/our-team/&amp

            /url?q=https://www.sympli.com.au/work-with-us/&amp

            /url?q=https://www.sympli.com.au/contact-us/&amp

            /url?q=https://msanational.com.au/msa-esettlements-via-pexa-digiSettle.htm&amp";


        public static string musicResult = @"
            /url?q=https://music.youtube.com/&amp

            /url?q=https://music.youtube.com/playlist%3Flist%3DPL4fGSI1pDJn5kI81J1fYWK5eZRl1zJ5kM&amp

            /url?q=https://music.youtube.com/playlist%3Flist%3DPL4fGSI1pDJn69On1f-8NAvX_CYlx7QyZc&amp

            /url?q=https://music.youtube.com/playlist%3Flist%3DRDTMAK5uy_lr0LWzGrq6FU9GIxWvFHTRPQD2LHMqlFA&amp

            /url?q=https://music.youtube.com/playlist%3Flist%3DOLAK5uy_k1kEJCk_CbtFpLGyIptQnnozpsTMyhiTA&amp

            /url?q=https://www.youtube.com/watch%3Fv%3DFuXNumBwDOM&amp

            /url?q=https://www.youtube.com/watch%3Fv%3DFuXNumBwDOM&amp

            /url?q=https://www.youtube.com/channel/UC-9-kyTW8ZkZNDHQJ6FgpwQ&amp

            /url?q=https://www.apple.com/au/apple-music/&amp

            /url?q=https://www.theguardian.com/music&amp

            /url?q=https://play.google.com/store/apps/details%3Fid%3Dcom.google.android.music%26hl%3Den%26gl%3DUS&amp

            /url?q=https://en.wikipedia.org/wiki/Music&amp

            /url?q=https://en.wikipedia.org/wiki/Music_genre&amp
            ";


        public static string bingResult = @"
            <cite>https://www.isettlements.com.au

            <cite>https://www.sympli.com.au

            <cite>https://www.sympli.com.au/e-settlement-services

            <cite>https://www.a1conveyancing.com.au/pexa-e-settlement

            <cite>https://www.infotrack.com.au/solutions/e-conveyancing

            <cite>https://www.anz.com.au/personal/home-loans/tips-and-guides/conveyanci...

            <cite>https://www.rba.gov.au/publications/annual-reports/psb/2017/trends-in-payments...

            <cite>https://www.pexa.com.au

            <cite>https://www.infotrack.com.au/news-and-insights/how-will-

            <cite>https://www.infotrack.com.au/solutions/e-conveyancing

            <cite>https://www.anz.com.au/personal/home-loans/tips-and-guides/conveyanci...

            <cite>https://www.msanational.com.au/msa-

            <cite>https://www.pexa.com.au/sites/qld

            <cite>https://www.qld.gov.au/law/housing-and-neighbours/buying-and-selling-a...

            <cite>https://www.pexa.com.au

            <cite>https://www.primepropertylawyers.com.au/is-pexa-safe

            <cite>https://www.trisearch.com.au/the-future-of-

            <cite>https://it.toolbox.com/question/

        ";

    }
}