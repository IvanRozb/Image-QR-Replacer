public static class FileHandler
{
    public static string ReadFileContent(string filePath)
    {
        // Read and return the content of the specified file
        return File.ReadAllText(filePath);
    }

    public static void WriteToFile(string filePath, string content)
    {
        // Write the provided content to the specified file
        File.WriteAllText(filePath, content);
    }
}
