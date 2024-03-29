﻿using System.Diagnostics;

namespace vergiBlue.Algorithms
{
    /// <summary>
    /// Thread-safe search timer
    /// </summary>
    public interface ISearchTimer
    {
        bool Exceeded();

        int CurrentElapsed();
    }

    public class SearchTimer : ISearchTimer
    {
        private readonly int _maxMilliseconds;

        protected Stopwatch InternalTimer { get; }

        private SearchTimer(int maxMilliseconds)
        {
            _maxMilliseconds = maxMilliseconds;
            InternalTimer = new Stopwatch();
        }

        public int CurrentElapsed()
        {
            return (int)InternalTimer.ElapsedMilliseconds;
        }

        public static ISearchTimer Start(int maxMilliseconds)
        {
            var timer = new SearchTimer(maxMilliseconds);
            timer.InternalTimer.Start();
            return timer;
        }

        public bool Exceeded()
        {
            return InternalTimer.ElapsedMilliseconds > _maxMilliseconds;
        }
    }
}
