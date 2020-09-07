using Serilog;
using Serilog.Core;
using System;

namespace AgileLab.Services.Logging
{
    class SerilogFileLogger : ILogger
    {
        private Logger _logger = new LoggerConfiguration().WriteTo.File("log.txt").CreateLogger();

        public void Debug(string message)
        {
            _logger.Debug($"[DEBUG] {message}");
        }

        public void Debug(Exception exception)
        {
            _logger.Debug($"[DEBUG-EXCEPTION] {exception.Message}");

            if(exception.InnerException != null)
            {
                _logger.Debug($"[DEBUG-INNER-EXCEPTION] {exception.InnerException.Message}");
            }
        }

        public void Error(string message)
        {
            _logger.Error($"[ERROR] {message}");
        }

        public void Error(Exception exception)
        {
            _logger.Error($"[ERROR-EXCEPTION] {exception.Message}");

            if (exception.InnerException != null)
            {
                _logger.Error($"[ERROR-INNER-EXCEPTION] {exception.InnerException.Message}");
            }
        }

        public void Fatal(string message)
        {
            _logger.Fatal($"[FATAL] {message}");
        }

        public void Fatal(Exception exception)
        {
            _logger.Fatal($"[FATAL-EXCEPTION] {exception.Message}");

            if (exception.InnerException != null)
            {
                _logger.Fatal($"[FATAL-INNER-EXCEPTION] {exception.InnerException.Message}");
            }
        }

        public void Information(string message)
        {
            _logger.Information($"[INFO] {message}");
        }

        public void Information(Exception exception)
        {
            _logger.Information($"[INFO-EXCEPTION] {exception.Message}");

            if (exception.InnerException != null)
            {
                _logger.Information($"[INFO-INNER-EXCEPTION] {exception.InnerException.Message}");
            }
        }

        public void Warning(string message)
        {
            _logger.Warning($"[WARNING] {message}");
        }

        public void Warning(Exception exception)
        {
            _logger.Warning($"[WARNING-EXCEPTION] {exception.Message}");

            if (exception.InnerException != null)
            {
                _logger.Warning($"[WARNING-INNER-EXCEPTION] {exception.InnerException.Message}");
            }
        }
    }
}
