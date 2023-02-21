using System;

public static class Logger
{
    private static string PATH_TO_LOGS = AppDomain.CurrentDomain.BaseDirectory + "/Logs/";
    private static string LOG_FILE_NAME = "log.txt";

    public static void LogInfo(string data)
    {
        Initialize();
        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(PATH_TO_LOGS + LOG_FILE_NAME, true))
        {
            sw.WriteLine(DateTime.Now + "\n" + data);
            Console.WriteLine(DateTime.Now + "\n" + data);
        }
    }


    private static void Initialize()
    {
        if (!System.IO.Directory.Exists(PATH_TO_LOGS))
        {
            System.IO.Directory.CreateDirectory(PATH_TO_LOGS);
        }
        if (!System.IO.File.Exists(PATH_TO_LOGS + LOG_FILE_NAME))
        {
            System.IO.File.Create(PATH_TO_LOGS + LOG_FILE_NAME);
        }
    }
}
