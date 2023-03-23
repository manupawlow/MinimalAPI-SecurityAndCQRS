using CQRS.Database;
using CQRS.Model;
using MediatR;

namespace CQRS.CQRS.Queries
{
    //Query/Command
    public record GetTodoByIdQuery(int Id) : IRequest<Todo>;

    //Response
    //public class GetTodoByIdResponse : Todo { }

    //Handler
    public class GetTodoByIdHandler : IRequestHandler<GetTodoByIdQuery, Todo>
    {
        private readonly Repository repository;

        public GetTodoByIdHandler(Repository repository)
        {
            this.repository = repository;
        }

        public async Task<Todo> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken)
        {
            return await repository.GetTodo(request.Id);
        }
    }
}
