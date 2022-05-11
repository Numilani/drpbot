using System.Text.RegularExpressions;

namespace drpbot.Services;

public static class StaticStringUtils
{
    public static bool isValidUrl(string url)
    {
        var regex = @"^((http[s]?|ftp):\/)?\/?([^:\/\s]+)((\/\w+)*\/)([\w\-\.]+[^#?\s]+)(.*)?(#[\w\-]+)?$";
        return Regex.IsMatch(url, regex);
    }
}