using System;
using System.Collections.Generic;
using System.Text;
using CommonNetStandard.Interface;

namespace CommonNetStandard.Local_implementation
{
    public class MoveImplementation : IMove
    {
        public IMove Clone()
        {
            throw new NotImplementedException();
        }

        public string StartPosition { get; set; }
        public string EndPosition { get; set; }
        public bool Check { get; set; }
        public bool CheckMate { get; set; }
        public bool Castling { get; set; }
        public PromotionPieceType PromotionResult { get; set; }

    }
}
