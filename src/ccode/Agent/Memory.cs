using System.Text.Json;

namespace ccode.Agent;

public class Memory(string persistencePath = "memory.json")
{
    private readonly List<string> _items = [];

    public void Add(string fact)
    {
        if (!_items.Contains(fact))
            _items.Add(fact);
    }

    private IReadOnlyList<string> GetRecent(int n) =>
        _items.TakeLast(n).ToList();

    public IReadOnlyList<string> Search(string query) =>
        _items.Where(i => i.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

    public void Clear() => _items.Clear();

    public void Load()
    {
        if (!File.Exists(persistencePath)) return;
        var json = File.ReadAllText(persistencePath);
        var items = JsonSerializer.Deserialize<List<string>>(json);
        if (items is null) return;
        _items.Clear();
        _items.AddRange(items);
    }

    public void Save() =>
        File.WriteAllText(persistencePath, JsonSerializer.Serialize(_items));

    public string GetContext() =>
        _items.Count == 0
            ? "No memory yet."
            : string.Join("\n", GetRecent(10).Select(i => $"- {i}"));
}
