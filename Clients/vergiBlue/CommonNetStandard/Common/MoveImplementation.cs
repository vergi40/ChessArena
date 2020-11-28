using CommonNetStandard.Interface;

namespace CommonNetStandard.Common
{
    public class MoveImplementation : IMove
    {
        public string StartPosition { get; set; } = "";
        public string EndPosition { get; set; } = "";
        public bool Check { get; set; }
        public bool CheckMate { get; set; }
        public bool Castling { get; set; }
        public PromotionPieceType PromotionResult { get; set; }

    }
}
