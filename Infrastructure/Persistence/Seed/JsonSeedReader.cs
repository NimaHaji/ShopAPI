using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seed;

public class JsonSeedReader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly ILogger<JsonSeedReader> _logger;

    public JsonSeedReader(ILogger<JsonSeedReader> logger)
    {
        _logger = logger;
    }

    public async Task<List<T>> ReadListAsync<T>(string fileName, bool required = false)
    {
        var path = ResolvePath(fileName);

        if (!File.Exists(path))
        {
            var message = $"Seed file not found: {path}";
            if (required)
                throw new FileNotFoundException(message);

            _logger.LogWarning(message);
            return new List<T>();
        }

        await using var stream = File.OpenRead(path);
        var result = await JsonSerializer.DeserializeAsync<List<T>>(stream, Options);
        var items = result ?? new List<T>();

        if (required && items.Count == 0)
            throw new InvalidOperationException($"Seed file is empty: {path}");

        _logger.LogInformation("Loaded {Count} items from {File}", items.Count, fileName);
        return items;
    }

    private static string ResolvePath(string fileName)
    {
        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "Persistence", "Seed", "Data", fileName);

        if (File.Exists(path))
            return path;

        var assemblyDir = Path.GetDirectoryName(typeof(JsonSeedReader).Assembly.Location)!;
        return Path.Combine(assemblyDir, "Persistence", "Seed", "Data", fileName);
    }
}
