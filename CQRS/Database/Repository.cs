using CQRS.Model;

namespace CQRS.Database
{
    public class Repository
    {
        public List<Todo> Todos { get; set; }
        public List<User> Users { get; set; }

        public Repository()
        {
            Todos = new List<Todo>
            {
                new Todo{ Id = 1, Name = "Cook dinner", Completed = false },
                new Todo{ Id = 2, Name = "Make Youtube video", Completed = true },
                new Todo{ Id = 3, Name = "Wash car", Completed = false },
                new Todo{ Id = 4, Name = "Practice programming", Completed = true },
                new Todo{ Id = 5, Name = "Take out garbage", Completed = false },
            };

            Users = new List<User>
            {
                new User{ Id = 1, Username = "mpaw", Password = "1234", Role = 1 },
                new User{ Id = 2, Username = "spaw", Password = "2345", Role = 2 },
                new User{ Id = 3, Username = "jpaw", Password = "3456", Role = 1 },
            };
        }

        public async Task<Todo> GetTodo(int id)
        {
            //await Task.Delay(5000);
            return Todos.FirstOrDefault(x => x.Id == id);
        }

        public async Task<int> AddTodo(Todo record)
        {
            //await Task.Delay(5000);
            record.Id = Todos.Max(x => x.Id) + 1;
            Todos.Add(record);
            return record.Id;
        }

    }
}
