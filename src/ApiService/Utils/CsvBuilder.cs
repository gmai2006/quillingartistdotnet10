using System.Text;

namespace ApiService.Utils;

public static class CsvBuilder
{
    public static string Build<T>(string header, IEnumerable<T> rows, Func<T, object?[]> columns)
    {
        var csv = new StringBuilder();
        csv.Append(header).Append('\n');
        foreach (var row in rows)
        {
            csv.AppendJoin(',', columns(row).Select(JsonHelper.CsvValue)).Append('\n');
        }
        return csv.ToString();
    }
}
