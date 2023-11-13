using System.Text;

public static class FileHandler
{
    public static string ReadFileContent(string filePath, out Encoding encoding)
    {
        using var reader = new StreamReader(filePath, Encoding.Default, true);

        var content = reader.ReadToEnd();
        encoding = reader.CurrentEncoding;

        return content;
    }

    public static void WriteToFile(string filePath, string content, Encoding encoding)
    {
        File.WriteAllText(filePath, content, encoding);
    }
}
