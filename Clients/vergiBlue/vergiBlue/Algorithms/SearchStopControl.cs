﻿using System.Threading;

namespace vergiBlue.Algorithms
{
    public interface ISearchStopControl
    {
        bool StopSearch();
        string Reason { get; set; }
    }

    public class SearchStopControl : ISearchStopControl
    {
        private ISearchTimer _searchTimer { get; }
        private CancellationToken _stopSearchToken { get; }

        public string Reason { get; set; } = "";

        /// <summary>
        /// Only timer control
        /// </summary>
        /// <param name="searchTimer"></param>
        public SearchStopControl(ISearchTimer searchTimer)
        {
            _searchTimer = searchTimer;
            _stopSearchToken = CancellationToken.None;
        }

        /// <summary>
        /// Timer control & cancellation support
        /// </summary>
        /// <param name="searchTimer"></param>
        /// <param name="stopSearchToken"></param>
        public SearchStopControl(ISearchTimer searchTimer, CancellationToken stopSearchToken)
        {
            _searchTimer = searchTimer;
            _stopSearchToken = stopSearchToken;
        }

        /// <summary>
        /// Stop search if time exceeded, nodecount exceeded or stop command given
        /// </summary>
        /// <returns></returns>
        public bool StopSearch()
        {
            if (_searchTimer.Exceeded())
            {
                Reason = "Search timer exceeded";
                return true;
            }
            if (_stopSearchToken.IsCancellationRequested)
            {
                Reason = "Search was stopped from uci";
                return true;
            }

            return false;
        }
    }
}
