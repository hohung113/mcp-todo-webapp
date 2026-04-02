using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.AspNetCore;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;
using TodoAI.Web.Data;
using TodoAI.Web.Models;
using TodoAI.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 45))
    ));

builder.Services.AddScoped<TodoService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();
app.UseCors();

app.MapPost("/api/todo", async (TodoService service, TodoRequest req) =>
{
    await service.Create(req.Title);
    return Results.Ok();
});

app.MapGet("/api/todo", async (TodoService service) =>
{
    return Results.Ok(await service.GetAll());
});

app.MapDelete("/api/todo/{id:guid}", async (TodoService service, Guid id) =>
{
    await service.Delete(id);
    return Results.Ok();
});

app.MapPatch("/api/todo/{id:guid}/complete", async (TodoService service, Guid id) =>
{
    await service.Complete(id);
    return Results.Ok();
});
app.MapPost("/api/agent", async (TodoService service, AgentRequest req) =>
{
    var options = new OpenAIClientOptions
    {
        Endpoint = new Uri("https://api.groq.com/openai/v1")
    };
    var client = new OpenAIClient(
     new ApiKeyCredential(app.Configuration["Groq:ApiKey"]!),
     options
    );
    var chat = client.GetChatClient("llama-3.3-70b-versatile");

    var tools = new List<ChatTool>
    {
        ChatTool.CreateFunctionTool(
            "create_todo",
            "Create a new todo item",
            BinaryData.FromString("""
            {
                "type": "object",
                "properties": {
                    "title": { "type": "string" }
                },
                "required": ["title"]
            }
            """)
        ),
        ChatTool.CreateFunctionTool(
            "delete_todo",
            "Delete a todo item by id",
            BinaryData.FromString("""
            {
                "type": "object",
                "properties": {
                    "id": { "type": "string" }
                },
                "required": ["id"]
            }
            """)
        ),
        ChatTool.CreateFunctionTool(
            "complete_todo",
            "Mark a todo item as completed by id",
            BinaryData.FromString("""
            {
                "type": "object",
                "properties": {
                    "id": { "type": "string" }
                },
                "required": ["id"]
            }
            """)
        ),
        ChatTool.CreateFunctionTool(
            "get_todos",
            "Get all todo items",
            BinaryData.FromString("""
            {
                "type": "object",
                "properties": {}
            }
            """)
        )
    };

    var chatOptions = new ChatCompletionOptions();
    foreach (var tool in tools)
        chatOptions.Tools.Add(tool);

    chatOptions.ToolChoice = ChatToolChoice.CreateAutoChoice();

    var existingTodos = await service.GetAll();
    var todosContext = existingTodos.Any()
        ? string.Join("\n", existingTodos.Select(t => $"- id: {t.Id}, title: {t.Title}, completed: {t.Completed}"))
        : "(no todos yet)";

    var userMessage = $"""
    Current todo list:
    {todosContext}

    User request: {req.Message}
    """;

    ClientResult<ChatCompletion> completion;
    try
    {
        completion = await chat.CompleteChatAsync(
            new List<ChatMessage>
            {
                new SystemChatMessage("You are a todo assistant. Only use the provided tools: create_todo, delete_todo, complete_todo, get_todos. If the request is not related to todos, reply normally in plain text. Never call any tool outside the provided list."),
                new UserChatMessage(userMessage)
            },
            chatOptions
        );
    }
    catch (Exception)
    {
        return Results.Ok(new { action = "none", message = "I could not process that request. Please try again." });
    }

    if (completion.Value.FinishReason == ChatFinishReason.ToolCalls)
    {
        var results = new List<object>();

        foreach (var toolCall in completion.Value.ToolCalls)
        {
            switch (toolCall.FunctionName)
            {
                case "create_todo":
                    {
                        using var createArgs = JsonDocument.Parse(toolCall.FunctionArguments);
                        var title = createArgs.RootElement.GetProperty("title").GetString();
                        await service.Create(title!);
                        results.Add(new { action = "created", title });
                        break;
                    }
                case "delete_todo":
                    {
                        using var deleteArgs = JsonDocument.Parse(toolCall.FunctionArguments);
                        var idStr = deleteArgs.RootElement.GetProperty("id").GetString()!;
                        if (!Guid.TryParse(idStr, out var id))
                            break;
                        await service.Delete(id);
                        results.Add(new { action = "deleted", id });
                        break;
                    }
                case "complete_todo":
                    {
                        using var completeArgs = JsonDocument.Parse(toolCall.FunctionArguments);
                        var idStr = completeArgs.RootElement.GetProperty("id").GetString()!;
                        if (!Guid.TryParse(idStr, out var id))
                            break;
                        await service.Complete(id);
                        results.Add(new { action = "completed", id });
                        break;
                    }
                case "get_todos":
                    {
                        var todos = await service.GetAll();
                        results.Add(new { action = "listed", todos });
                        break;
                    }
            }
        }

        if (results.Any())
            return Results.Ok(new { action = "multi", results });
    }


    return Results.Ok(new { action = "none", message = completion.Value.Content[0].Text });

});
app.MapMcp("/mcp");
app.Run();