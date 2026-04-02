using ModelContextProtocol.Server;
using TodoAI.Web.Models;
using TodoAI.Web.Services;

namespace TodoAI.Web.Tools
{
    [McpServerToolType]
    public class TodoTools
    {
        private readonly TodoService _service;

        public TodoTools(TodoService service)
        {
            _service = service;
        }

        [McpServerTool]
        public async Task<Todo> create_todo(string title)
        {
            return await _service.Create(title);
        }

        [McpServerTool]
        public async Task<List<Todo>> get_todos()
        {
            return await _service.GetAll();
        }

        [McpServerTool]
        public async Task complete_todo(Guid id)
        {
            await _service.Complete(id);
        }
    }
}