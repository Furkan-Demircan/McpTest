namespace MCP.Server.Knowledge;

/// <summary>
/// Knowledge klasöründeki frontmatter'lı markdown dosyalarını okur.
/// Frontmatter: id, title, pages ve keywords (son ikisi virgülle ayrılmış).
/// </summary>
public static class MarkdownKnowledgeLoader
{
    public static IReadOnlyList<KnowledgeDocument> LoadFrom(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(directory, "*.md", SearchOption.AllDirectories)
            .Select(Parse)
            .ToList();
    }

    private static KnowledgeDocument Parse(string path)
    {
        var lines = File.ReadAllLines(path);
        var meta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var bodyStart = 0;

        if (lines.Length > 0 && lines[0].Trim() == "---")
        {
            for (var i = 1; i < lines.Length; i++)
            {
                if (lines[i].Trim() == "---")
                {
                    bodyStart = i + 1;
                    break;
                }

                var separatorIndex = lines[i].IndexOf(':');
                if (separatorIndex > 0)
                {
                    meta[lines[i][..separatorIndex].Trim()] =
                        lines[i][(separatorIndex + 1)..].Trim();
                }
            }
        }

        return new KnowledgeDocument
        {
            Id = meta.GetValueOrDefault("id") ?? Path.GetFileNameWithoutExtension(path),
            Title = meta.GetValueOrDefault("title") ?? Path.GetFileNameWithoutExtension(path),
            Pages = SplitList(meta.GetValueOrDefault("pages")),
            Keywords = SplitList(meta.GetValueOrDefault("keywords")),
            Content = string.Join('\n', lines.Skip(bodyStart)).Trim()
        };
    }

    private static string[] SplitList(string? value) =>
        (value ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
