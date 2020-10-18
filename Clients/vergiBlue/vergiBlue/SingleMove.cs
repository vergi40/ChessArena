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
            PrevPos = Logic.ToTuple(interfaceMove.StartPosition);
            NewPos = Logic.ToTuple(interfaceMove.EndPosition);
            Capture = capture;
            Promotion = interfaceMove.PromotionResult != Move.Types.PromotionPieceType.NoPromotion;
        }

        public string ToAlgebraic((int, int) position)
        {
            return Logic.ToAlgebraic(position);
        }

        public Move ToInterfaceMove()
        {
            // TODO checks etc.
            var move = new Move()
            {
                StartPosition = ToAlgebraic(PrevPos),
                EndPosition = ToAlgebraic(NewPos),
                PromotionResult = Move.Types.PromotionPieceType.NoPromotion
            };
            return move;
        }

        public override string ToString()
        {
            var info = $"{Logic.ToAlgebraic(PrevPos)} to ";
            if (Capture) info += "x";
            info += Logic.ToAlgebraic(NewPos);
            return info;
        }
    }
}
