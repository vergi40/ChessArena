using System;
using System.Text;
using CommonNetStandard.Common;
using CommonNetStandard.Interface;

namespace vergiBlue
{
    public interface ISingleMove
    {
        (int column, int row) PrevPos { get; }
        (int column, int row) NewPos { get; }

        bool Capture { get; }
        bool Castling { get; }
        bool EnPassant { get; }
        PromotionPieceType PromotionType { get; }

        (int column, int row) EnPassantOpponentPosition { get; }

        string ToCompactString();

        /// <summary>
        /// Create deep clone object (not ref type).
        /// </summary>
        ISingleMove CreateClone();
    }


    /// <summary>
    /// Minimal move data.
    /// Struct type to enable unmanaged utilization, e.g. stackalloc.
    /// Should be passed as ref struct 
    /// </summary>
    public readonly struct MoveStruct : ISingleMove
    {
        public (int column, int row) PrevPos { get; init; }
        public (int column, int row) NewPos { get; init; }

        public bool Capture { get; init; }
        public bool Castling { get; init; }
        public bool EnPassant { get; init; }
        public PromotionPieceType PromotionType { get; init; }


        // Is needed?

        /// <summary>
        /// Produces check-state to other player
        /// </summary>
        public bool Check { get; init; }
        
        
        // Should these be external functions?
        public (int column, int row) EnPassantOpponentPosition
        {
            get
            {
                if (!EnPassant) return (-1, -1);
                return (NewPos.column, PrevPos.row);
            }
        }
        public bool Promotion => PromotionType != PromotionPieceType.NoPromotion;

        public string ToCompactString()
        {
            var message = new StringBuilder();
            message.Append(PrevPos.ToAlgebraic());
            message.Append(NewPos.ToAlgebraic());
            if (Promotion)
            {
                message.Append(SingleMove.ConvertPromotion(PromotionType));
            }

            return message.ToString();
        }

        public ISingleMove CreateClone()
        {
            // Create class-type singlemove
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            var message = new StringBuilder();
            message.Append($"{PrevPos.ToAlgebraic()} to ");
            if (Capture) message.Append("x");
            message.Append(NewPos.ToAlgebraic());
            if (Promotion)
            {
                message.Append(SingleMove.ConvertPromotion(PromotionType));
            }

            if (Check)
            {
                message.Append(" (check)");
            }

            return message.ToString();
        }
    }

    /// <summary>
    /// Reference type (class) move data
    /// </summary>
    public sealed class SingleMove : IEquatable<SingleMove>, ISingleMove
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

        public bool EnPassant { get; set; }
        public (int column, int row) EnPassantOpponentPosition
        {
            get
            {
                if (!EnPassant) return (-1, -1);
                return (NewPos.column, PrevPos.row);
            }
        }

        public (int column, int row) PrevPos { get; }
        public (int column, int row) NewPos { get; }

        /// <summary>
        /// Internal soft target - aka capture own piece
        /// </summary>
        public bool SoftTarget { get; set; }

        public SingleMove((int column, int row) previousPosition, (int column, int row) newPosition,
            bool capture = false, PromotionPieceType promotionType = PromotionPieceType.NoPromotion)
        {
            PrevPos = previousPosition;
            NewPos = newPosition;
            Capture = capture;
            PromotionType = promotionType;
        }

        /// <summary>
        /// En passant constructor
        /// </summary>
        public SingleMove((int column, int row) previousPosition, (int column, int row) newPosition,
            bool capture, bool enPassant)
        {
            PrevPos = previousPosition;
            NewPos = newPosition;
            Capture = capture;
            EnPassant = enPassant;
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
            if (!EnPassant.Equals(other.EnPassant)) return false;
            if (!SoftTarget.Equals(other.SoftTarget)) return false;

            return true;
        }

        /// <summary>
        /// Previous and new positions are equal. Promotion equals. This is because same
        /// movement can happen to multiple promotion variations.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EqualPositions(in ISingleMove? other)
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
            var message = new StringBuilder();
            message.Append($"{PrevPos.ToAlgebraic()} to ");
            if (Capture) message.Append("x");
            message.Append(NewPos.ToAlgebraic());
            if (Promotion)
            {
                message.Append(ConvertPromotion(PromotionType));
            }

            if (Check)
            {
                message.Append(" (check)");
            }

            return message.ToString();
        }
        
        public string ToCompactString()
        {
            var message = new StringBuilder();
            message.Append(PrevPos.ToAlgebraic());
            message.Append(NewPos.ToAlgebraic());
            if (Promotion)
            {
                message.Append(ConvertPromotion(PromotionType));
            }

            return message.ToString();
        }

        public static char ConvertPromotion(PromotionPieceType type)
        {
            var c = type switch
            {
                PromotionPieceType.Queen => 'q',
                PromotionPieceType.Rook => 'r',
                PromotionPieceType.Bishop => 'b',
                PromotionPieceType.Knight => 'n',
                _ => ' '
            };
            return c;
        }

        public static PromotionPieceType ConvertPromotion(char type)
        {
            type = char.ToLower(type);
            var promotion = type switch
            {
                'q' =>  PromotionPieceType.Queen,
                'r' =>  PromotionPieceType.Rook,
                'b' =>  PromotionPieceType.Bishop,
                'n' =>  PromotionPieceType.Knight,
                _ => PromotionPieceType.NoPromotion
            };
            return promotion;
        }

        public ISingleMove CreateClone()
        {
            // Sufficient as only base type properties
            var clone = this.MemberwiseClone() as SingleMove;
            if (clone == null) throw new InvalidOperationException();

            return clone;
        }
    }
}
