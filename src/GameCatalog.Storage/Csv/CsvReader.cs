using System.Text;

namespace GameCatalog.Storage.Csv;

/// <summary>Minimal RFC 4180 reader: quoted fields may contain delimiters, doubled quotes and line breaks.</summary>
public static class CsvReader
{
    public static IEnumerable<string[]> ReadRecords(TextReader reader, char delimiter)
    {
        var fields = new List<string>();
        var field = new StringBuilder();
        bool inQuotes = false, any = false;
        int c;
        while ((c = reader.Read()) != -1)
        {
            char ch = (char)c;
            any = true;
            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (reader.Peek() == '"') { field.Append('"'); reader.Read(); }
                    else inQuotes = false;
                }
                else field.Append(ch);
                continue;
            }
            if (ch == '"' && field.Length == 0) { inQuotes = true; continue; }
            if (ch == delimiter) { fields.Add(field.ToString()); field.Clear(); continue; }
            if (ch == '\r') continue;
            if (ch == '\n')
            {
                fields.Add(field.ToString());
                field.Clear();
                yield return fields.ToArray();
                fields.Clear();
                any = false;
                continue;
            }
            field.Append(ch);
        }
        if (any)
        {
            fields.Add(field.ToString());
            yield return fields.ToArray();
        }
    }
}
