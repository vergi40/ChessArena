using System;
using System.Collections.Generic;
using System.Text;
using CommonNetStandard.Local_implementation;

namespace CommonNetStandard.Interface
{
    public class Mapping
    {
        public static Move ToGrpc(IMove move)
        {
            var grpcMove = new Move()
            {
                Castling = move.Castling,
                Check = move.Check,
                CheckMate = move.CheckMate,
                StartPosition = move.StartPosition,
                EndPosition = move.EndPosition,
                PromotionResult = (Move.Types.PromotionPieceType) move.PromotionResult
            };
            return grpcMove;
        }

        public static PlayerMove ToGrpc(IPlayerMove playerMove)
        {
            var grpcMove = new PlayerMove()
            {
                Diagnostics = playerMove.Diagnostics,
                Move = ToGrpc(playerMove.Move)
            };
            return grpcMove;
        }

        public static IMove ToCommon(Move grpcMove)
        {
            var move = new MoveImplementation()
            {
                Castling = grpcMove.Castling,
                Check = grpcMove.Check,
                CheckMate = grpcMove.CheckMate,
                StartPosition = grpcMove.StartPosition,
                EndPosition = grpcMove.EndPosition,
                PromotionResult = (PromotionPieceType)grpcMove.PromotionResult
            };
            return move;
        }

        
    }
}
