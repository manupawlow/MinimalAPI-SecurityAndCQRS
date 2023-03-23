using CQRS.Database;
using CQRS.Model;
using MediatR;

namespace CQRS.CQRS.Commands
{
    //Query/Command
    public record CreateTodoCommand(string Name) : IRequest<CreateTodoResponse>;

    //Response
    public record CreateTodoResponse(int Id);

    //Handler
    public class CreateTodoHandler : IRequestHandler<CreateTodoCommand, CreateTodoResponse>
    {
        private readonly Repository repository;

        public CreateTodoHandler(Repository repository)
        {
            this.repository = repository;
        }

        public async Task<CreateTodoResponse> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
        {
            var id = await repository.AddTodo(new Todo { Name = request.Name, Completed = false });
            return new CreateTodoResponse(id);
        }
    }
}
