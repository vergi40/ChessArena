using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue;
using vergiBlue.BoardModel;
using vergiBlue.Logic;

namespace vergiBlueDesktop
{
    /// <summary>
    /// Shared game session data between users
    /// </summary>
    public class GameSession
    {
        public IBoard Board { get; }

        /// <summary>
        /// Binded to viewmodel - view
        /// </summary>
        public LogicSettings Settings { get; }
        public bool PlayerIsWhite { get; }

        // protect, single point of truth
        private bool _isWhiteTurn;

        public bool IsWhiteTurn => _isWhiteTurn;

        public GameSession(IBoard board, bool playerIsWhite, bool isWhiteTurn, LogicSettings settings)
        {
            Board = board;
            PlayerIsWhite = playerIsWhite;
            _isWhiteTurn = isWhiteTurn;
            Settings = settings;
        }

        public void TurnChanged()
        {
            _isWhiteTurn = !_isWhiteTurn;
        }
    }
}
