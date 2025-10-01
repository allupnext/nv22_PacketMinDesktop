using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NV22SpectralInteg
{
    public static class Logger
    {
        private static readonly string logDirectory;
        private static readonly string errorLogDirectory;
        private static readonly string machineAvailabilityDirectory;
        private static readonly object logLock = new object();

        static Logger()
        {
            try
            {
                string basePath = "c:/PocketMint/";

                logDirectory = Path.Combine(basePath, "Logs");
                errorLogDirectory = Path.Combine(basePath, "ErrorLogs");
                machineAvailabilityDirectory = Path.Combine(basePath, "MachineAvailabilityLogs");

                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                if (!Directory.Exists(errorLogDirectory))
                    Directory.CreateDirectory(errorLogDirectory);

                if (!Directory.Exists(machineAvailabilityDirectory))
                    Directory.CreateDirectory(machineAvailabilityDirectory);

            }
            catch
            {
                // Silent fail
            }
        }

        private static string GetLogFilePath() =>
            Path.Combine(logDirectory, $"app_log_{DateTime.Now:yyyy-MM-dd}.txt");

        private static string GetErrorLogFilePath() =>
            Path.Combine(errorLogDirectory, $"error_log_{DateTime.Now:yyyy-MM-dd}.txt");
        private static string GetmachineAvailabilityLogFilePath() =>
            Path.Combine(machineAvailabilityDirectory, $"machineAvailability_log_{DateTime.Now:yyyy-MM-dd}.txt");

        public static void Log(string message)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]   {message}";
                lock (logLock)
                {
                    File.AppendAllText(GetLogFilePath(), logEntry + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Log write failed: " + ex.Message);
            }
        }
        
        public static void MachineLog(string message)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]   {message}";
                lock (logLock)
                {
                    File.AppendAllText(GetmachineAvailabilityLogFilePath(), logEntry + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Log write failed: " + ex.Message);
            }
        }

        public static void LogException(Exception ex, string context = "")
        {
            Log($"[EXCEPTION] {context}: {ex.Message}\n{ex.StackTrace}");
        }


        public static void LogError(
            string message,
            Exception ex,
            [CallerMemberName] string caller = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            try
            {
                string errorBlock = BuildErrorBlock(message, ex, caller, file, line);
                File.AppendAllText(GetErrorLogFilePath(), errorBlock + Environment.NewLine);
            }
            catch (Exception innerEx)
            {
                Console.WriteLine("Error logging failed: " + innerEx.Message);
            }
        }

        private static string BuildErrorBlock(
            string message,
            Exception ex,
            string caller,
            string file,
            int line)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string divider = new string('═', 70);

            return $@"
                {divider}
                🛑 ERROR OCCURRED 🛑
                ⏰ Timestamp: {timestamp}
                🔖 Message: {message}

                👤 User: {(string.IsNullOrEmpty(AppSession.CustomerName) ? "N/A" : AppSession.CustomerName)}
                📱 Mobile: {(string.IsNullOrEmpty(AppSession.CustomerMobile) ? "N/A" : AppSession.CustomerMobile)}
                🏪 KioskID: {(string.IsNullOrEmpty(AppSession.KioskId) ? "N/A" : AppSession.KioskId)}

                📍 Location:
                    Method: {caller}
                    File: {Path.GetFileName(file)}
                    Line: {line}

                💥 Exception:
                {ex.GetType().Name}: {ex.Message}
                📄 StackTrace:
                {ex.StackTrace}
                {divider}";
        }




        public static void LogNewUserStart(string username)
        {
            try
            {
                string separator = new string('═', 50);
                string header = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]   🚀 NEW USER SESSION STARTED: {username}";

                File.AppendAllText(GetLogFilePath(),
                    Environment.NewLine + separator + Environment.NewLine +
                    header + Environment.NewLine +
                    separator + Environment.NewLine);
            }
            catch { }
        }

        public static void LogSeparator(string title = null, char character = '═', int length = 70)
        {
            try
            {
                string separator;

                // If no title is provided, just create a simple line.
                if (string.IsNullOrWhiteSpace(title))
                {
                    separator = new string(character, length);
                }
                else
                {
                    // If a title is provided, center it within the separator line.
                    string paddedTitle = $" {title.ToUpper()} ";
                    int sideLength = (length - paddedTitle.Length) / 2;

                    // Ensure sideLength is not negative if the title is too long.
                    if (sideLength < 0) sideLength = 0;

                    string sides = new string(character, sideLength);
                    separator = $"{sides}{paddedTitle}{sides}";
                }

                // Lock for thread safety, just like your other log methods.
                lock (logLock)
                {
                    // Add new lines before and after for clear visual separation.
                    File.AppendAllText(GetLogFilePath(), Environment.NewLine + separator + Environment.NewLine + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Log separator write failed: " + ex.Message);
            }
        }
    }
}
