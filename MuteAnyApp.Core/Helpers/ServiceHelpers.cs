using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MuteAnyApp.Core.Helpers
{
    public static class ServiceHelpers
    {
        public const string ScenarioFolderName = "Data";
        public const string ScenarioFileName = "Scenario.json";
        public const string LogFolderName = "Logs";
        public const string LogFileName = "Logs.txt";

        public static string GetAssemblyDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().Location;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        public static void WriteToLog(string message)
        {
            try
            {
                var logsDir = Path.Combine(GetAssemblyDirectory(), LogFolderName);

                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }

                var logFileName = Path.Combine(logsDir, LogFileName);

                if (File.Exists(logFileName))
                {
                    var content = File.ReadAllLines(logFileName).ToList();
                    if (content.Count > 200)
                    {
                        content = content.Take(199).ToList();
                    }
                    content.Insert(0, $"{DateTime.Now} - {message}");
                    File.WriteAllLines(logFileName, content);
                }
                else
                {
                    File.WriteAllLines(logFileName, new string[] { $"{DateTime.Now} - {message}" });
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message, "Ошибка записи лога", MessageBoxButton.OK);
            }
        }
    }
}
