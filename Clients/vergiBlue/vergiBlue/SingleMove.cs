using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonNetStandard.Common;
using CommonNetStandard.Interface;
using CommonNetStandard.LocalImplementation;

namespace vergiBlue
{
    // TODO separate move without additional data
    // public class SingleMoveBase
    
    // public class SingleMoveWithData
    
    public sealed class SingleMove : IEquatable<SingleMove>
    {
        public bool Capture { get; set; }
        public bool Promotion => PromotionType != PromotionPieceType.NoPromotion;
        public PromotionPieceType PromotionType { get; set; }
        public bool Castling { get; set; }
        
        /// <summary>
        /// Produces check-state to other player
        /// </summary>
        public bool Check { get; set; }
        public bool CheckMate { get; set; }

        public (int column, int row) PrevPos { get; }
        public (int column, int row) NewPos { get; }

        public SingleMove((int column, int row) previousPosition, (int column, int row) newPosition,
            bool capture = false, PromotionPieceType promotionType = PromotionPieceType.NoPromotion)
        {
            PrevPos = previousPosition;
            NewPos = newPosition;
            Capture = capture;
            PromotionType = promotionType;
        }

        public SingleMove(string previousPosition, string newPosition, bool capture = false, 
            PromotionPieceType promotionType = PromotionPieceType.NoPromotion)
        {
            PrevPos = previousPosition.ToTuple();
            NewPos = newPosition.ToTuple();
            Capture = capture;
            PromotionType = promotionType;
        }

        /// <summary>
        /// Constructor from interface move data
        /// </summary>
        /// <param name="interfaceMove"></param>
        /// <param name="capture"></param>
        public SingleMove(IMove interfaceMove, bool capture = false)
        {
            PrevPos = interfaceMove.StartPosition.ToTuple();
            NewPos = interfaceMove.EndPosition.ToTuple();
            Capture = capture;
            PromotionType = interfaceMove.PromotionResult;
            Castling = interfaceMove.Castling;
            Check = interfaceMove.Check;
            CheckMate = interfaceMove.CheckMate;
        }

        public IMove ToInterfaceMove()
        {
            var move = new MoveImplementation()
            {
                StartPosition = PrevPos.ToAlgebraic(),
                EndPosition = NewPos.ToAlgebraic(),
                PromotionResult = PromotionType,
                Castling = Castling,
                Check = Check,
                CheckMate = CheckMate
            };
            return move;
        }

        /// <summary>
        /// Move is equal if positions and properties match
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(SingleMove? other)
        {
            if (other == null) return false;
            if (!PrevPos.Equals(other.PrevPos)) return false;
            if (!NewPos.Equals(other.NewPos)) return false;
            if (!Capture.Equals(other.Capture)) return false;
            if (!Castling.Equals(other.Castling)) return false;
            if (!Check.Equals(other.Check)) return false;
            if (!CheckMate.Equals(other.CheckMate)) return false;

            return false;
        }

        /// <summary>
        /// Previous and new positions are equal. Promotion equals. This is because same
        /// movement can happen to multiple promotion variations.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EqualPositions(SingleMove? other)
        {
            if (other == null) return false;
            if (PrevPos.Equals(other.PrevPos) && NewPos.Equals(other.NewPos) && PromotionType == other.PromotionType)
            {
                return true;
            }

            return false;
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
