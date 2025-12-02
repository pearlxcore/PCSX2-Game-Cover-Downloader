using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public static class SerialExtractor
{
    // Regex: 4 letters, dash, 5 letters/digits
    private static readonly Regex SerialRegex =
        new Regex(@"\b([A-Za-z]{4}-[0-9A-Za-z]{5})\b", RegexOptions.Compiled);

    private static readonly Regex PrefixAlpha =
        new Regex(@"^[A-Z]{4}$", RegexOptions.Compiled);

    /// <summary>
    /// Reads serials from a gamelist.cache file.
    /// </summary>
    public static List<string> FromFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Cache file not found", path);

        byte[] bytes = File.ReadAllBytes(path);
        return FromBytes(bytes);
    }

    /// <summary>
    /// Reads serials from a binary buffer.
    /// </summary>
    public static List<string> FromBytes(byte[] data)
    {
        // Latin1 = preserves 00–FF bytes, serials stay readable
        string text = Encoding.Latin1.GetString(data);

        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match m in SerialRegex.Matches(text))
        {
            string serial = m.Groups[1].Value.ToUpperInvariant();

            // Validate prefix letters (SLUS, SCUS, SLES, SLPM, etc)
            if (PrefixAlpha.IsMatch(serial.Substring(0, 4)))
                results.Add(serial);
        }

        return new List<string>(results);
    }
}
