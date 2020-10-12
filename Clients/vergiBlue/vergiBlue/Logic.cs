using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace vergiBlue
{
    class Logic : LogicBase
    {
        private int _index = 2;
        public Move LatestOpponentMove { get; set; }

        public Logic(GameStartInformation startInformation)
        {

        }

        public override PlayerMove CreateMove()
        {
            // TODO testing
            var move = new PlayerMove()
            {
                Move = new Move()
                {
                    StartPosition = $"a{_index--}",
                    EndPosition = $"a{_index}",
                    PromotionResult = Move.Types.PromotionPieceType.NoPromotion
                },
                Diagnostics = "Search depth = 0."
            };

            return move;
        }

        public override void ReceiveMove(Move opponentMove)
        {
            // TODO testing
            LatestOpponentMove = opponentMove;
        }
    }
}
