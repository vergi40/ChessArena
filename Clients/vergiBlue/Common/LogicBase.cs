using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// Inherit custom AI logic from this class and inject it to <see cref="Common.ConnectionModule"/>
    /// </summary>
    public abstract class LogicBase
    {
        /// <summary>
        /// Client is white player and starts the game
        /// </summary>
        public bool IsPlayerWhite { get; }
        protected LogicBase(bool isPlayerWhite) { IsPlayerWhite = isPlayerWhite; }
        
        public abstract PlayerMove CreateMove();
        public abstract void ReceiveMove(Move opponentMove);
    }
}
