using System;

namespace vergiBlue
{
    public static class SingleMoveFactory
    {
        /// <summary>
        /// Capture own piece. For internal attack squares
        /// </summary>
        public static SingleMove CreateSoftTarget((int column, int row) previousPosition,
            (int column, int row) newPosition)
        {
            return new SingleMove(previousPosition, newPosition, true)
            {
                SoftTarget = true
            };
        }

        public static SingleMove CreateCastling((int column, int row) previousPosition,
            (int column, int row) newPosition)
        {
            return new SingleMove(previousPosition, newPosition)
            {
                Castling = true
            };
        }

        public static SingleMove CreateEmpty()
        {
            return new SingleMove((-1, -1), (-1, -1));
        }

        /// <summary>
        /// Move with compact parameter e.g. "a1b1" or "c4f1" or "b7b8q"
        /// </summary>
        public static SingleMove Create(string compact, bool capture = false)
        {
            if (compact.Length < 4 || compact.Length > 5)
                throw new ArgumentException($"Compact move string {compact} should have characters or 5 when promotion.");

            var prev = compact.Substring(0, 2);
            var next = compact.Substring(2, 2);

            if (compact.Length == 5)
            {
                var promotionChar = compact[4];
                return new SingleMove(prev, next, capture, SingleMove.ConvertPromotion(promotionChar));
            }

            return new SingleMove(prev, next, capture);
        }

        public static MoveStruct Create(in SingleMove moveReference)
        {
            return new MoveStruct()
            {
                PrevPos = moveReference.PrevPos,
                NewPos = moveReference.NewPos,
                Capture = moveReference.Capture,
                Castling = moveReference.Castling,
                Check = moveReference.Check,
                EnPassant = moveReference.EnPassant,
                PromotionType = moveReference.PromotionType
            };
        }
    }
}
