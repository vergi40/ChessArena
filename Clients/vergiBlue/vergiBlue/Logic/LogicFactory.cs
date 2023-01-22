using System;
using CommonNetStandard.Client;
using CommonNetStandard.Interface;

namespace vergiBlue.Logic
{
    /// <summary>
    /// Entry point for using vergiBlue
    /// </summary>
    public static class LogicFactory
    {
        /// <summary>
        /// Create new AI logic to be used in any game
        /// </summary>
        public static Logic Create(IGameStartInformation startInformation, int? overrideMaxDepth = null, BoardModel.IBoard? overrideBoard = null)
        {
            return new Logic(startInformation, overrideMaxDepth, overrideBoard);
        }

        /// <summary>
        /// For tests. Start board known. Test environment handles initializations.
        /// If default board, remember to set Strategic.SkipOpeningChecks = true
        /// </summary>
        public static Logic CreateForTest(bool isPlayerWhite, BoardModel.IBoard board, int? overrideMaxDepth = null)
        {
            return new Logic(isPlayerWhite, board, overrideMaxDepth);
        }

        /// <summary>
        /// For tests. Need to set board explicitly. Test environment handles initializations.
        /// </summary>
        [Obsolete("For tests, use constructor with Board parameter.")]
        public static Logic CreateWithoutBoardInit(bool isPlayerWhite, int? overrideMaxDepth = null)
        {
            return new Logic(isPlayerWhite, overrideMaxDepth);
        }

        public static IUciClient CreateForUci()
        {
            var logic = new Logic();
            logic.InitializeStaticSystems();
            return logic;
        }
    }
}