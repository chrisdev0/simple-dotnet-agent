using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;

namespace ccode.Shared;

public static class JsonHelper
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };

    public static string GetSchema<T>()
    {
        var schema = Options.GetJsonSchemaAsNode(typeof(T));
        return schema.ToJsonString(Options);
    }

    public static string? ExtractJson(string text)
    {
        // Strip markdown code fences
        var start = text.IndexOf("```", StringComparison.Ordinal);
        if (start != -1)
        {
            var contentStart = text.IndexOf('\n', start) + 1;
            var end = text.IndexOf("```", contentStart, StringComparison.Ordinal);
            if (end != -1)
                text = text[contentStart..end].Trim();
        }

        // Find first { or [
        var objStart = text.IndexOfAny(['{', '[']);
        if (objStart == -1) return null;

        var openChar = text[objStart];
        var closeChar = openChar == '{' ? '}' : ']';
        var depth = 0;

        for (var i = objStart; i < text.Length; i++)
        {
            if (text[i] == openChar) depth++;
            else if (text[i] == closeChar) depth--;

            if (depth == 0)
                return text[objStart..(i + 1)];
        }

        return null;
    }

    public static T? TryDeserialize<T>(string text)
    {
        var json = ExtractJson(text);
        if (json is null) return default;

        try { return JsonSerializer.Deserialize<T>(json, Options); }
        catch { return default; }
    }
}
