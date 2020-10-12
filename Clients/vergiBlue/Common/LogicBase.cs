using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// Inherit custom AI logic from this class and inject it to <see cref="Common.ConnecionModule"/>
    /// </summary>
    public abstract class LogicBase
    {
        public abstract PlayerMove CreateMove();
        public abstract void ReceiveMove(Move opponentMove);
    }
}
