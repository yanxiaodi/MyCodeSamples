namespace Demo6;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class TodoService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://jsonplaceholder.typicode.com/todos";

    public TodoService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    // Create a new todo
    public async Task<Todo> CreateTodoAsync(Todo newTodo)
    {
        var response = await _httpClient.PostAsJsonAsync(BaseUrl, newTodo);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Todo>();
    }

    // Get a todo by ID
    public async Task<Todo> GetTodoByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Todo>();
    }

    // Get all todos
    public async Task<IEnumerable<Todo>> GetAllTodosAsync()
    {
        var response = await _httpClient.GetAsync(BaseUrl);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Todo>>();
    }

    // Update an existing todo
    public async Task<Todo> UpdateTodoAsync(int id, Todo updatedTodo)
    {
        var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", updatedTodo);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Todo>();
    }

    // Delete a todo by ID
    public async Task DeleteTodoAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
        response.EnsureSuccessStatusCode();
    }
}

// Todo entity model
public class Todo
{
    public int UserId { get; set; }
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool Completed { get; set; }
}

