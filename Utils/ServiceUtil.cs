public class ServiceUtil
{
    public static void WriteToFile(string message)
    {
        try
        {
            string basePath = Directory.GetCurrentDirectory();

            // Ensure logs directory exists
            string logDir = Path.Combine(basePath, "logs");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            // Determine today's log filename
            string today = DateTime.Now.ToString("yyyy_MM_dd");
            string logFile = Path.Combine(logDir, $"ServiceLog_{today}.txt");

            using (StreamWriter writer = File.AppendText(logFile))
            {
                writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} :: {message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Logging failed: {ex.Message}");
        }
    }
}