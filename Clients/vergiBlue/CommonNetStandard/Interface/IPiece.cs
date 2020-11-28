using System;
using System.Collections.Generic;
using System.Text;

namespace CommonNetStandard.Interface
{
    public interface IPiece
    {
        bool IsWhite { get; }
        /// <summary>
        /// Upper case K, Q, R, N, B, P
        /// </summary>
        char Identity { get; }
        (int column, int row) CurrentPosition { get; set; }
    }

}
