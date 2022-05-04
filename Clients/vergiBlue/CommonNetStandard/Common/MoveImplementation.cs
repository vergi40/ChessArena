using System.Text;
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

        public override string ToString()
        {
            var message = new StringBuilder();
            message.Append($"{StartPosition}{EndPosition}");
            if (PromotionResult != PromotionPieceType.NoPromotion)
            {
                message.Append(ConvertPromotion(PromotionResult));
            }

            if (Check)
            {
                message.Append(" (check)");
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
                'q' => PromotionPieceType.Queen,
                'r' => PromotionPieceType.Rook,
                'b' => PromotionPieceType.Bishop,
                'n' => PromotionPieceType.Knight,
                _ => PromotionPieceType.NoPromotion
            };
            return promotion;
        }
    }
}
