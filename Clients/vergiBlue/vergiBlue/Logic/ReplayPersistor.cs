using CommonNetStandard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using vergiBlue.Analytics;
using vergiBlue.BoardModel;
using vergiBlue.Database;

namespace vergiBlue.Logic
{
    public interface IReplayPersistor
    {
        /// <summary>
        /// Create new replay record. Save following moves to this record
        /// </summary>
        void InitializeNewGame(bool isWhite, IBoard board);

        /// <summary>
        /// Save ai move that has evaluation records
        /// </summary>
        void SaveMoveWithAnalytics(IDescriptiveTurnData turnData);

        /// <summary>
        /// Save move with minimal data
        /// </summary>
        void SaveMove(IMinimalTurnData turnData);
    }
    /// <summary>
    /// Save each turn to readable format. Load full game for inspection
    /// </summary>
    internal class ReplayPersistor : IReplayPersistor
    {
        private readonly IDatabase _db;


        public ReplayPersistor(IDatabase db)
        {
            _db = db;
        }
        
        public void InitializeNewGame(bool isWhite, IBoard board)
        {
            // 
        }

        public void SaveMoveWithAnalytics(IDescriptiveTurnData turnData)
        {

        }

        public void SaveMove(IMinimalTurnData turnData)
        {

        }
    }

    internal class DebugReplay : IReplayPersistor
    {
        private static readonly ILogger _logger = ApplicationLogging.CreateLogger<DebugReplay>();
        public void InitializeNewGame(bool isWhite, IBoard board)
        {
            _logger.LogDebug($"Init. PlayerWhite: {isWhite}. FEN: {board.GenerateFen()}");
        }

        public void SaveMoveWithAnalytics(IDescriptiveTurnData turnData)
        {
            _logger.LogDebug($"Save descriptive move: {turnData}");
        }

        public void SaveMove(IMinimalTurnData turnData)
        {
            _logger.LogDebug($"Save minimal move: {turnData}");
        }
    }
}
