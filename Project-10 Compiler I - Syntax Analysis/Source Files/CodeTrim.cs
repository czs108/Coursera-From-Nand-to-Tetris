using System.Diagnostics;
using System.Text.RegularExpressions;
using System;

namespace Project10
{
    public static class CodeTrim
    {
        public static string RemoveComment(string content)
        {
            Debug.Assert(content != null);

            content = Regex.Replace(content.Trim(), @"/\*[\s\S]*?\*/", String.Empty);
            content = Regex.Replace(content, @"//[\s\S]*?$", String.Empty, RegexOptions.Multiline);
            return Regex.Replace(content, @"^\s*//[\s\S]*", String.Empty, RegexOptions.Multiline);
        }

        public static string RemoveNewLine(string content)
        {
            Debug.Assert(content != null);

            return Regex.Replace(content.Trim(), @"(\n|\r\n)", String.Empty);
        }

        public static string RemoveMultispace(string content)
        {
            Debug.Assert(content != null);

            return Regex.Replace(content, @"\s{2,}", " ");
        }
    }
}