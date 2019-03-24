using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace HostMerger
{
    public static class LoggerExtensions
    {
        public static IDisposable Measure(this ILogger logger, string scopeName)
        {
            var ts = new Stopwatch();
            ts.Start();
            return new Timer(() =>
            {
                ts.Stop();
                logger.LogMetric(scopeName, ts.Elapsed.TotalMilliseconds);
                var time = ts.Elapsed.TotalMilliseconds > 5000 ? $"{ts.Elapsed.TotalSeconds}s" : $"{ts.Elapsed.TotalMilliseconds}ms";
                logger.LogInformation($"Scope {scopeName} finished in {time}");
            });
        }

        private class Timer : IDisposable
        {
            private readonly Action _onDispose;

            public Timer(Action onDispose)
            {
                _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
            }

            public void Dispose()
            {
                _onDispose();
            }
        }
    }
}
