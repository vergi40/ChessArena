using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue
{
    public class SingleMove
    {
        public bool Capture { get; set; }
        public bool Promotion { get; set; }

        public (int, int) PrevPos { get; }
        public (int, int) NewPos { get; }

        public SingleMove((int column, int row) previousPosition, (int column, int row) newPosition, bool capture = false, bool promotion = false)
        {
            PrevPos = previousPosition;
            NewPos = newPosition;
            Capture = capture;
            Promotion = promotion;
        }

        /// <summary>
        /// Constructor for interface move data
        /// </summary>
        /// <param name="interfaceMove"></param>
        /// <param name="capture"></param>
        public SingleMove(Move interfaceMove, bool capture = false)
        {
            PrevPos = interfaceMove.StartPosition.ToTuple();
            NewPos = interfaceMove.EndPosition.ToTuple();
            Capture = capture;
            Promotion = interfaceMove.PromotionResult != Move.Types.PromotionPieceType.NoPromotion;
        }

        public Move ToInterfaceMove(bool castling, bool check, bool checkMate)
        {
            // TODO checks etc.
            var move = new Move()
            {
                StartPosition = PrevPos.ToAlgebraic(),
                EndPosition = NewPos.ToAlgebraic(),
                PromotionResult = Move.Types.PromotionPieceType.NoPromotion,
                Castling = castling,
                Check = check,
                CheckMate = checkMate
            };
            return move;
        }

        public override string ToString()
        {
            var info = $"{PrevPos.ToAlgebraic()} to ";
            if (Capture) info += "x";
            info += NewPos.ToAlgebraic();
            return info;
        }
    }
}
