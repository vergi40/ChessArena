using CommonNetStandard.Connection;
using CommonNetStandard.Interface;

namespace CommonNetStandard
{
    /// <summary>
    /// Inherit custom AI logic from this class and inject it to <see cref="ConnectionModule"/>
    /// </summary>
    public abstract class LogicBase
    {
        /// <summary>
        /// Client is white player and starts the game
        /// </summary>
        public bool IsPlayerWhite { get; }
        protected LogicBase(bool isPlayerWhite) { IsPlayerWhite = isPlayerWhite; }
        
        public abstract IPlayerMove CreateMove();
        public abstract void ReceiveMove(IMove opponentMove);
    }
}
