using System.Text.RegularExpressions;

namespace CMT.Common
{
    public static class CaseConverter
    {
        public static Regex rgx = new Regex("[a-z][A-Z]|[a-zA-Z][0-9]|[0-9][a-zA-Z]", RegexOptions.Compiled);

        public static string DeCamelCase(string sentence)
        {
            return DeCamelCase(sentence, '_');
        }

        public static string DeCamelCase(string sentence, char separator)
        {
            return rgx.Replace(sentence, delegate (Match match) { return InsertSeparator(match, separator); });
        }

        private static string InsertSeparator(Match match, char separator)
        {
            char[] chars = new char[] { match.Value[0], separator, match.Value[1] };
            return new string(chars);
        }
    }
}
