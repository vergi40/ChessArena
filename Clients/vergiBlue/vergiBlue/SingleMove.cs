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
        public bool CheckMate { get; set; }

        public (int, int) PrevPos { get; }
        public (int, int) NewPos { get; }

        public SingleMove((int column, int row) previousPosition, (int column, int row) newPosition, bool capture = false, bool promotion = false)
        {
            PrevPos = previousPosition;
            NewPos = newPosition;
            Capture = capture;
            Promotion = promotion;
        }

        public SingleMove(string previousPosition, string newPosition, bool capture = false, bool promotion = false)
        {
            PrevPos = previousPosition.ToTuple();
            NewPos = newPosition.ToTuple();
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

        public Move ToInterfaceMove(bool castling, bool check)
        {
            var promotionType = Move.Types.PromotionPieceType.NoPromotion;
            if (Promotion) promotionType = Move.Types.PromotionPieceType.Queen;

            var move = new Move()
            {
                StartPosition = PrevPos.ToAlgebraic(),
                EndPosition = NewPos.ToAlgebraic(),
                PromotionResult = promotionType,
                Castling = castling,
                Check = check,
                CheckMate = CheckMate
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
