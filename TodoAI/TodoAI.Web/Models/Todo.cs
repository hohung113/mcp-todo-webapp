namespace TodoAI.Web.Models
{
    public class Todo
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public bool Completed { get; set; }
    }
    public class TodoRequest
    {
        public string Title { get; set; }
    }
    public class AgentRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
