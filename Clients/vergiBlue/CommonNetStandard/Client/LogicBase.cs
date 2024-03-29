﻿using CommonNetStandard.Interface;

namespace CommonNetStandard.Client
{
    /// <summary>
    /// Inherit custom AI logic from this class and inject it to <see cref="GrpcClientConnection"/>
    /// </summary>
    public abstract class LogicBase
    {
        /// <summary>
        /// Client is white player and starts the game
        /// </summary>
        public bool IsPlayerWhite { get; set; }
        protected LogicBase(bool isPlayerWhite) { IsPlayerWhite = isPlayerWhite; }
        
        public abstract IPlayerMove CreateMove();
        public abstract void ReceiveMove(IMove opponentMove);
    }
}
