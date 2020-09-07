using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileLab.Services.Logging
{
    class FileLogger : ILogger
    {
        public string filePath = @"log.txt";

        public void Debug(string message)
        {
            Log($"[DEBUG]\t{message}");
        }

        public void Debug(Exception exception)
        {
            Log($"[DEBUG-EXCEPTION] {exception.Message}");

            if (exception.InnerException != null)
            {
                Log($"[DEBUG-INNER-EXCEPTION] {exception.InnerException.Message}");
            }
        }

        public void Error(string message)
        {
            Log($"[ERROR] {message}");
        }

        public void Error(Exception exception)
        {
            Log($"[ERROR-EXCEPTION] {exception.Message}");

            if (exception.InnerException != null)
            {
                Log($"[ERROR-INNER-EXCEPTION] {exception.InnerException.Message}");
            }
        }

        public void Fatal(string message)
        {
            Log($"[FATAL] {message}");
        }

        public void Fatal(Exception exception)
        {
            Log($"[FATAL-EXCEPTION] {exception.Message}");

            if (exception.InnerException != null)
            {
                Log($"[FATAL-INNER-EXCEPTION] {exception.InnerException.Message}");
            }
        }

        public void Information(string message)
        {
            Log($"[INFO] {message}");
        }

        public void Information(Exception exception)
        {
            Log($"[INFO-EXCEPTION] {exception.Message}");

            if (exception.InnerException != null)
            {
                Log($"[INFO-INNER-EXCEPTION] {exception.InnerException.Message}");
            }
        }

        public void Warning(string message)
        {
            Log($"[WARNING] {message}");
        }

        public void Warning(Exception exception)
        {
            Log($"[WARNING-EXCEPTION] {exception.Message}");

            if (exception.InnerException != null)
            {
                Log($"[WARNING-INNER-EXCEPTION] {exception.InnerException.Message}");
            }
        }

        public void Log(string message)
        {
            try
            {
                string dateTime = DateTime.Now.ToString("hh:mm:ss");
                message = $"{dateTime}\t{message}{Environment.NewLine}";
                File.AppendAllText(filePath, message);
            }
            catch { }
        }
    }
}
