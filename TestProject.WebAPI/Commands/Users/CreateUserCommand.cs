using MediatR;

namespace TestProject.WebAPI.Commands.Users
{
    public class CreateUserCommand : IRequest<CreateUserResponse>
    {
        public CreateUserRequest Request { get; private set; }

        public CreateUserCommand(CreateUserRequest request)
        {
            Request = request;
        }

    }
}
