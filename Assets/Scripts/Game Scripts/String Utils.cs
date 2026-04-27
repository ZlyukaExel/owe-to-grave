using System.Text;

public static class StringUtils
{
    public static string RepeatStr(string text, int count)
    {
        StringBuilder stringBuilder = new();
        for (int i = 0; i < count; i++)
        {
            stringBuilder.Append(text);
        }
        return stringBuilder.ToString();
    }
}