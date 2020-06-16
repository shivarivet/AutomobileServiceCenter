using ASC.Business.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.Web.Logger
{
    public static class LoggerExtensions
    {
        public static ILoggerFactory AddAzureTableStorageLog(this ILoggerFactory loggerFactory, ILogDataOperations logDataOperations, Func<string, LogLevel, bool> filter = null)
        {
            loggerFactory.AddProvider(new AzureStorageLoggerProvider(filter, logDataOperations));
            return loggerFactory;
        }
    }

    public class AzureStorageLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly Func<string, LogLevel, bool> _filter;
        private readonly ILogDataOperations _logDataOperations;

        public AzureStorageLogger(string categoryName, Func<string, LogLevel, bool> filter, ILogDataOperations logDataOperations)
        {
            this._categoryName = categoryName;
            this._filter = filter;
            this._logDataOperations = logDataOperations;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return (_filter == null || _filter(_categoryName, logLevel));
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (exception == null)
                this._logDataOperations.CreateLogAsync(logLevel.ToString(), formatter(state, exception));
            else
                this._logDataOperations.CreateExceptionLogAsync(eventId.Name, exception.Message, exception.StackTrace);
        }
    }

    public class AzureStorageLoggerProvider : ILoggerProvider
    {
        private readonly Func<string, LogLevel, bool> _filter;
        private readonly ILogDataOperations _logDataOperations;

        public AzureStorageLoggerProvider(Func<string, LogLevel, bool> filter, ILogDataOperations logDataOperations)
        {
            this._filter = filter;
            this._logDataOperations = logDataOperations;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new AzureStorageLogger(categoryName, _filter, _logDataOperations);
        }

        public void Dispose()
        {
            
        }
    }
}
