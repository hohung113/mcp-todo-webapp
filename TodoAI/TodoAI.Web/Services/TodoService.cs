using Microsoft.EntityFrameworkCore;
using TodoAI.Web.Data;
using TodoAI.Web.Models;

namespace TodoAI.Web.Services
{
    public class TodoService
    {
        private readonly AppDbContext _db;

        public TodoService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Todo> Create(string title)
        {
            var todo = new Todo
            {
                Id = Guid.NewGuid(),
                Title = title
            };

            _db.Todos.Add(todo);

            await _db.SaveChangesAsync();

            return todo;
        }

        public async Task<List<Todo>> GetAll()
        {
            return await _db.Todos.ToListAsync();
        }

        public async Task Complete(Guid id)
        {
            var todo = await _db.Todos.FindAsync(id);

            if (todo == null)
                return;

            todo.Completed = true;

            await _db.SaveChangesAsync();
        }
        public async Task Delete(Guid id)
        {
            var todo = await _db.Todos.FindAsync(id);

            if (todo == null)
                return;

            _db.Todos.Remove(todo);

            await _db.SaveChangesAsync();
        }

    }
}
