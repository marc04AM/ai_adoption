// crea un'app todo CLI in c# che: salva e carica to-do da todos.js, 
// Comandi: add, list, done ID, delete ID, quit, Usa System.Text.Json, 
// validazioni input, 
// Pattern pulito con List<Todo>, enum Status       

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

const string FilePath = "todos.json";

var options = new JsonSerializerOptions
{
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter() }
};

var todos = LoadTodos();

Console.WriteLine("Welcome to the To-Do CLI App!");
Console.WriteLine("Available commands: add, list, done ID, delete ID, quit");

while (true)
{
    Console.WriteLine(">");
    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(input)) continue;

    var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
    var command = parts[0].ToLower();
    var arg = parts.Length > 1 ? parts[1] : string.Empty;

    switch (command)
    {
        case "add":
            AddTodo(arg);
            break;
        case "list":
            ListTodos();
            break;
        case "done":
            MarkDone(arg);
            break;
        case "delete":
            DeleteTodo(arg);
            break;
        case "quit":
            SaveTodos();
            Console.WriteLine("Goodbye!");
            return;
        default:
            Console.WriteLine("Unknown command. Available commands: add, list, done ID, delete ID, quit");
            break;
    }
}

void AddTodo(string description)
{
    if (string.IsNullOrWhiteSpace(description))
    {
        Console.WriteLine("Description cannot be empty.");
        return;
    }

    var todo = new Todo
    {
        Id = Guid.NewGuid(),
        Description = description,
        Status = Status.Pending
    };
    todos.Add(todo);
    SaveTodos();
    Console.WriteLine($"Added: {description}");
}

void ListTodos()
{
    if (todos.Count == 0)
    {
        Console.WriteLine("No to-dos found.");
        return;
    }

    foreach (var todo in todos)
    {
        Console.WriteLine($"{todo.Id} - [{todo.Status}] {todo.Description}");
    }
}

void MarkDone(string id)
{
    if (!Guid.TryParse(id, out var guid))
    {
        Console.WriteLine("Invalid ID format.");
        return;
    }

    var todo = todos.Find(t => t.Id == guid);
    if (todo == null)
    {
        Console.WriteLine("To-do not found.");
        return;
    }

    todo.Status = Status.Done;
    SaveTodos();
    Console.WriteLine($"Marked as done: {todo.Description}");
}

void DeleteTodo(string id)
{
    if (!Guid.TryParse(id, out var guid))
    {
        Console.WriteLine("Invalid ID format.");
        return;
    }

    var todo = todos.Find(t => t.Id == guid);
    if (todo == null)
    {
        Console.WriteLine("To-do not found.");
        return;
    }

    todos.Remove(todo);
    SaveTodos();
    Console.WriteLine($"Deleted: {todo.Description}");
}

void SaveTodos()
{
    var json = JsonSerializer.Serialize(todos, options);
    File.WriteAllText(FilePath, json);
}

List<Todo> LoadTodos()
{
    if (!File.Exists(FilePath))
    {
        return new List<Todo>();
    }

    var json = File.ReadAllText(FilePath);
    return JsonSerializer.Deserialize<List<Todo>>(json, options) ?? new List<Todo>();
}

public class Todo
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public Status Status { get; set; }
}

public enum Status
{
    Pending,
    Done
}