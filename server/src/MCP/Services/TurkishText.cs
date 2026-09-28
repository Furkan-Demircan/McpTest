using System.Globalization;
using System.Text;

namespace MCP.Server;

/// <summary>
/// Basit Türkçe metin eşleştirme: küçük harfe çevirir, Türkçe karakterleri sadeleştirir
/// ve kelimeleri ortak önek üzerinden eşler ("öğrenciyi" ~ "öğrenci", "eklerim" ~ "ekle").
/// Bilgi tabanı ve sayfa araması bunu kullanır; RAG'e geçişte yerini embedding alır.
/// </summary>
public static class TurkishText
{
    private const int MinPrefixLength = 4;

    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    // Soru kalıplarında geçen ama konuyu belirlemeyen kelimeler (sadeleştirilmiş halleriyle)
    private static readonly HashSet<string> StopWords =
    [
        "ben", "sen", "bu", "su", "bir", "ve", "ile", "icin", "ama", "nasil",
        "ne", "neden", "nerede", "mi", "mu", "misin", "musun", "bilmiyorum",
        "yapabilirim", "yapilir", "olur", "olacak", "olacagini", "istiyorum",
        "lazim", "gerek", "yardim", "sistemde", "sisteme", "yeni", "bana", "beni"
    ];

    /// <summary>Sorguyu anlamlı kelimelere ayırır (stop word'ler hariç).</summary>
    public static List<string> QueryTokens(string query) =>
        Tokenize(query).Where(token => !StopWords.Contains(token)).ToList();

    public static bool IsMatch(string queryToken, string documentToken)
    {
        if (queryToken == documentToken)
        {
            return true;
        }

        var prefixLength = Math.Min(queryToken.Length, documentToken.Length);
        return prefixLength >= MinPrefixLength &&
               string.CompareOrdinal(queryToken, 0, documentToken, 0, prefixLength) == 0;
    }

    public static HashSet<string> Tokenize(string text)
    {
        var normalized = new StringBuilder(text.Length);

        foreach (var ch in text.ToLower(Turkish))
        {
            normalized.Append(ch switch
            {
                'ç' => 'c',
                'ğ' => 'g',
                'ı' => 'i',
                'ö' => 'o',
                'ş' => 's',
                'ü' => 'u',
                _ => char.IsLetterOrDigit(ch) ? ch : ' '
            });
        }

        return normalized
            .ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(token => token.Length > 1)
            .ToHashSet();
    }
}
