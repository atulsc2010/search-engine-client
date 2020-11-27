using MediatR;
using System;

namespace TestProject.WebAPI.Queries.Users
{
    public class GetUserQuery : IRequest<GetUserResponse>
    {
        public Guid Id { get; set; }
        public GetUserQuery(Guid id)
        {
            Id = id;
        }
    }
}
