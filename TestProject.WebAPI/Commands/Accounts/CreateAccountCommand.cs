using MediatR;

namespace TestProject.WebAPI.Commands.Accounts
{
    public class CreateAccountCommand : IRequest<CreateAccountResponse>
    {
        public CreateAccountRequest Request { get; private set; }

        public CreateAccountCommand(CreateAccountRequest request)
        {
            Request = request;
        }

    }
}
