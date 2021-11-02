namespace CMT.Common
{
    public static class StringHelper
    {
        public static string WrapToExport(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            text = text.Replace("\"", "\"\"");
            text = text.Replace("\n", " ");

            return string.Format("\"{0}\"", text);
        }
    }
}
