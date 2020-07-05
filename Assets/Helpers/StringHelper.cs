using System.Text.RegularExpressions;

public static class StringHelper
{
    public static string WildcardToRegex(string pattern)
    {
        return Regex.Escape(pattern).Replace("*", ".*").Replace("?", ".");
    }

    public static bool FilterMatch(string filter, string name, params string[] options)
    {
        var filterRegex = WildcardToRegex(filter);

        if (Regex.IsMatch(name, filterRegex))
        {
            return true;
        }
        if (options != null)
        {
            foreach (var cat in options)
            {
                if (Regex.IsMatch(cat, filterRegex))
                {
                    return true;
                }
            }
        }

        return false;
    }
}