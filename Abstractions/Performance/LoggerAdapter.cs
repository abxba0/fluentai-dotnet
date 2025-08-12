using Microsoft.Extensions.Logging;

namespace FluentAI.Abstractions.Performance
{
    /// <summary>
    /// Adapter to convert ILogger to ILogger&lt;T&gt; for compatibility with typed logger requirements.
    /// </summary>
    /// <typeparam name="T">The category type for the logger.</typeparam>
    internal class LoggerAdapter<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public LoggerAdapter(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return _logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}