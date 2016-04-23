using System;
using System.Diagnostics;
using System.Reactive.Concurrency;

namespace SimpleVoiceReliverService
{
    public class ServiceStopWatch : IStopwatch
    {
        private Stopwatch stopwatch = Stopwatch.StartNew();
        public TimeSpan Elapsed => stopwatch.Elapsed;
    }
}