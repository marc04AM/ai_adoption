using System.Text.Json;
using System.Text.Json.Serialization;

const string FilePath = "todos.json";

var options = new JsonSerializerOptions
{
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter() }
};

var todos = LoadTodos();

Console.WriteLine("=== Todo CLI ===");
Console.WriteLine("Comandi: add, list, done <id>, delete <id>, quit\n");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
    var command = parts[0].ToLower();
    var arg = parts.Length > 1 ? parts[1] : null;

    switch (command)
    {
        case "add":
            Add(arg);
            break;
        case "list":
            ListTodos();
            break;
        case "done":
            SetDone(arg);
            break;
        case "delete":
            Delete(arg);
            break;
        case "quit":
            SaveTodos();
            Console.WriteLine("Bye!");
            return;
        default:
            Console.WriteLine($"Comando sconosciuto: {command}");
            break;
    }
}

void Add(string? title)
{
    if (string.IsNullOrWhiteSpace(title))
    {
        Console.WriteLine("Uso: add <titolo>");
        return;
    }

    var id = todos.Count > 0 ? todos.Max(t => t.Id) + 1 : 1;
    todos.Add(new Todo(id, title.Trim(), Status.Pending));
    SaveTodos();
    Console.WriteLine($"Aggiunto: [{id}] {title.Trim()}");
}

void ListTodos()
{
    if (todos.Count == 0)
    {
        Console.WriteLine("Nessun todo.");
        return;
    }

    foreach (var t in todos)
    {
        var mark = t.Status == Status.Done ? "x" : " ";
        Console.WriteLine($"  [{mark}] {t.Id}. {t.Title}");
    }
}

void SetDone(string? idStr)
{
    if (!TryParseId(idStr, out var todo))
        return;

    if (todo.Status == Status.Done)
    {
        Console.WriteLine($"Todo {todo.Id} già completato.");
        return;
    }

    todo.Status = Status.Done;
    SaveTodos();
    Console.WriteLine($"Completato: {todo.Id}. {todo.Title}");
}

void Delete(string? idStr)
{
    if (!TryParseId(idStr, out var todo))
        return;

    todos.Remove(todo);
    SaveTodos();
    Console.WriteLine($"Eliminato: {todo.Id}. {todo.Title}");
}

bool TryParseId(string? idStr, out Todo todo)
{
    todo = null!;

    if (string.IsNullOrWhiteSpace(idStr) || !int.TryParse(idStr, out var id))
    {
        Console.WriteLine("Uso: <comando> <id> (numero intero)");
        return false;
    }

    var found = todos.FirstOrDefault(t => t.Id == id);
    if (found is null)
    {
        Console.WriteLine($"Todo con id {id} non trovato.");
        return false;
    }

    todo = found;
    return true;
}

List<Todo> LoadTodos()
{
    if (!File.Exists(FilePath))
        return new List<Todo>();

    var json = File.ReadAllText(FilePath);
    return JsonSerializer.Deserialize<List<Todo>>(json, options) ?? new List<Todo>();
}

void SaveTodos()
{
    File.WriteAllText(FilePath, JsonSerializer.Serialize(todos, options));
}

enum Status { Pending, Done }

class Todo
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public Status Status { get; set; }

    public Todo() { }
    public Todo(int id, string title, Status status)
    {
        Id = id;
        Title = title;
        Status = status;
    }
}
