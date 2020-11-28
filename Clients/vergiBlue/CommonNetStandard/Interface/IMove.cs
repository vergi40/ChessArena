namespace CommonNetStandard.Interface
{
    public enum PromotionPieceType
    {
        NoPromotion = 0,
        Queen = 1,
        Rook = 2,
        Knight = 3,
        Bishop = 4,
    }

    public interface IMove
    {
        /// <summary>
        /// Standard chess notation (file, rank), e.g. "f5"
        /// </summary>
        string StartPosition { get; set; }

        /// <summary>
        /// Standard chess notation (file, rank) e.g. "f6"
        /// </summary>
        string EndPosition { get; set; }

        bool Check { get; set; }
        bool CheckMate { get; set; }
        bool Castling { get; set; }

        /// <summary>
        /// Set to non-zero if pawn has reached 8th rank.
        /// </summary>
        PromotionPieceType PromotionResult { get; set; }
    }
}