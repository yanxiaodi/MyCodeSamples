using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace Demo6;

[McpServerToolType]
public static class TodoTools
{

    [McpServerTool, Description("Creates a new todo item.")]
    public static async Task<string> CreateTodo(TodoService todoService, string title, bool completed)
    {
        var todo = await todoService.CreateTodoAsync(new Todo
        {
            Title = title,
            Completed = completed
        });
        return JsonSerializer.Serialize(todo);
    }

    [McpServerTool, Description("Gets a todo item by ID.")]
    public static async Task<string> GetTodoById(TodoService todoService, int id)
    {
        var todo = await todoService.GetTodoByIdAsync(id);
        return JsonSerializer.Serialize(todo);
    }

    [McpServerTool, Description("Gets all todo items.")]
    public static async Task<string> GetAllTodos(TodoService todoService)
    {
        var todos = await todoService.GetAllTodosAsync();
        return JsonSerializer.Serialize(todos);
    }

    [McpServerTool, Description("Gets all todo items by a userId.")]
    public static async Task<string> GetTodosByUserId(TodoService todoService, int userId)
    {
        var todos = await todoService.GetAllTodosAsync();
        return JsonSerializer.Serialize(todos.Where(x => x.UserId == userId).ToList());
    }

    [McpServerTool, Description("Updates an existing todo item.")]
    public static async Task<string> UpdateTodo(TodoService todoService, int id, string title, bool completed)
    {
        var updatedTodo = await todoService.UpdateTodoAsync(id, new Todo
        {
            Title = title,
            Completed = completed
        });
        return JsonSerializer.Serialize(updatedTodo);
    }

    [McpServerTool, Description("Deletes a todo item by ID.")]
    public static async Task<string> DeleteTodo(TodoService todoService, int id)
    {
        await todoService.DeleteTodoAsync(id);
        return $"Todo with ID: {id} deleted successfully.";
    }
}
