using System.Text;

namespace PropGen.WPF.Helpers
{
    public static class LineSortHelper
    {
        public static string SortLines(string input)
        {
            // Split input into lines, handling all line break formats
            var lines = input.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(line => line.Trim())
                             .Where(line => !string.IsNullOrWhiteSpace(line))
                             .ToList();

            var parsedLines = new List<(string line, string sortKey)>();
            var invalidLines = new List<string>();

            foreach (var line in lines)
            {
                if (line.Contains(','))
                {
                    // Handle "Name, Type" format - sort by first part (before comma)
                    var parts = line.Split(new[] { ',' }, 2);
                    if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]))
                    {
                        parsedLines.Add((line, parts[0].Trim()));
                        continue;
                    }
                }
                else
                {
                    // Handle "Type Name" format - sort by last part (after last space)
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        parsedLines.Add((line, parts.Last()));
                        continue;
                    }
                }

                // If we get here, the line didn't match either format
                invalidLines.Add(line);
            }

            // Sort valid lines by their sort key (case-insensitive)
            var sortedLines = parsedLines
                .OrderBy(x => x.sortKey, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.line)
                .ToList();

            // Append invalid lines at the bottom
            sortedLines.AddRange(invalidLines);

            var sb = new StringBuilder();
            int lineCount = sortedLines.Count;
            foreach (var line in sortedLines)
            {
                lineCount--;

                if (lineCount > 0)
                    sb.AppendLine(line);
                else
                    sb.Append(line);
            }           

            return sb.ToString();
        }

        private static bool TryParseTypeNameFormat(string line, out string name)
        {
            name = string.Empty;

            if (string.IsNullOrWhiteSpace(line))
                return false;

            // Check if line matches "Type Name" format
            int lastSpaceIndex = line.LastIndexOf(' ');

            if (lastSpaceIndex > 0 && lastSpaceIndex < line.Length - 1)
            {
                name = line.Substring(lastSpaceIndex + 1).Trim();
                return true;
            }

            return false;
        }

        private static bool TryParseNameTypeFormat(string line, out string name)
        {
            name = string.Empty;

            if (string.IsNullOrWhiteSpace(line))
                return false;

            // Check if line matches "Name, Type" format
            int commaIndex = line.IndexOf(',');

            if (commaIndex > 0 && commaIndex < line.Length - 1)
            {
                name = line.Substring(0, commaIndex).Trim();
                return true;
            }

            return false;
        }
    }
}
