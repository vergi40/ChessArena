using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Shouldly;
using vergiBlue;
using vergiBlue.BoardModel;
using vergiBlue.BoardModel.Subsystems;
using vergiBlue.Pieces;

namespace UnitTests
{
    [TestFixture]
    public class CastlingTests
    {
        private List<PieceBase> CreateCastlingLayout()
        {
            // 8r   k  r
            // 7p      p
            // 6  
            // 5    
            // 4
            // 3
            // 2P      P
            // 1R   K  R       
            //  ABCDEFGH
            var pieces = new List<PieceBase>
            {
                new King(true, "e1"),
                new King(false, "e8"),
                new Rook(true, "a1"),
                new Rook(true, "h1"),
                new Rook(false, "a8"),
                new Rook(false, "h8"),

                new Pawn(true, "a2"),
                new Pawn(true, "h2"),
                new Pawn(false, "a7"),
                new Pawn(false, "h7"),
            };

            return pieces;
        }

        [Test]
        public void LeftRookCaptured_ShouldRevoke()
        {
            // 8r   k  r
            // 7p      p
            // 6  B  B
            // 5    
            // 4
            // 3
            // 2P      P
            // 1R   K  R       
            //  ABCDEFGH
            var pieces = CreateCastlingLayout();
            pieces.Add(new Bishop(true, "c6"));

            var board = BoardFactory.CreateFromPieces(pieces);
            board.Strategic.BlackLeftCastlingValid.ShouldBeTrue();
            board.ExecuteMove(new SingleMove("c6", "a8", true));

            board.Strategic.BlackLeftCastlingValid.ShouldBeFalse();
        }

        [Test]
        public void RightRookCaptured_ShouldRevoke()
        {
            // 8r   k  r
            // 7p      p
            // 6  B  B
            // 5    
            // 4
            // 3
            // 2P      P
            // 1R   K  R       
            //  ABCDEFGH
            var pieces = CreateCastlingLayout();
            pieces.Add(new Bishop(true, "f6"));

            var board = BoardFactory.CreateFromPieces(pieces);
            board.Strategic.BlackRightCastlingValid.ShouldBeTrue();
            board.ExecuteMove(new SingleMove("f6", "h8", true));

            board.Strategic.BlackRightCastlingValid.ShouldBeFalse();
        }

        [Test]
        public void KingMoved_ShouldRevoke()
        {
            // 8r   k  r
            // 7p      p
            // 6  
            // 5    
            // 4
            // 3
            // 2P      P
            // 1R   K  R       
            //  ABCDEFGH
            var pieces = CreateCastlingLayout();

            var board = BoardFactory.CreateFromPieces(pieces);
            board.Strategic.WhiteLeftCastlingValid.ShouldBeTrue();
            board.Strategic.WhiteRightCastlingValid.ShouldBeTrue();
            board.Strategic.BlackLeftCastlingValid.ShouldBeTrue();
            board.Strategic.BlackRightCastlingValid.ShouldBeTrue();

            board.ExecuteMove(new SingleMove("e1", "e2"));
            board.Strategic.WhiteLeftCastlingValid.ShouldBeFalse();
            board.Strategic.WhiteRightCastlingValid.ShouldBeFalse();

            board.ExecuteMove(new SingleMove("e8", "e7"));
            board.Strategic.BlackLeftCastlingValid.ShouldBeFalse();
            board.Strategic.BlackRightCastlingValid.ShouldBeFalse();
        }

        [Test]
        public void RookMoved_ShouldRevoke()
        {
            // 8r   k  r
            // 7p      p
            // 6  
            // 5    
            // 4
            // 3
            // 2P      P
            // 1R   K  R       
            //  ABCDEFGH
            var pieces = CreateCastlingLayout();

            var board = BoardFactory.CreateFromPieces(pieces);
            board.Strategic.WhiteLeftCastlingValid.ShouldBeTrue();
            board.Strategic.WhiteRightCastlingValid.ShouldBeTrue();
            board.Strategic.BlackLeftCastlingValid.ShouldBeTrue();
            board.Strategic.BlackRightCastlingValid.ShouldBeTrue();

            board.ExecuteMove(new SingleMove("a1", "d1"));
            board.Strategic.WhiteLeftCastlingValid.ShouldBeFalse();

            board.ExecuteMove(new SingleMove("a8", "d8"));
            board.Strategic.BlackLeftCastlingValid.ShouldBeFalse();

            board.ExecuteMove(new SingleMove("h1", "f1"));
            board.Strategic.WhiteRightCastlingValid.ShouldBeFalse();

            board.ExecuteMove(new SingleMove("h8", "f8"));
            board.Strategic.BlackRightCastlingValid.ShouldBeFalse();
        }

        [Test]
        [TestCase("c5", false, true, false)]
        [TestCase("d5", false, true, false)]
        [TestCase("e5", false, false, false)]
        [TestCase("f5", true, false, false)]
        [TestCase("g5", true, false, false)]
        [TestCase("c5", false, true, true)]
        [TestCase("d5", false, true, true)]
        [TestCase("e5", false, false, true)]
        [TestCase("f5", true, false, true)]
        [TestCase("g5", true, false, true)]
        public void PositionThreatened_ShouldNotContainCastlingMove(string rookPos, bool leftOk, bool rightOk, bool whiteMoves)
        {
            // 8r   k  r
            // 7p      p
            // 6  
            // 5  RRRRR  
            // 4
            // 3
            // 2P      P
            // 1R   K  R       
            //  ABCDEFGH
            var pieces = CreateCastlingLayout();
            var rook = new Rook(!whiteMoves, rookPos);
            pieces.Add(rook);

            var board = BoardFactory.CreateFromPieces(pieces);

            var moves = board.MoveGenerator.MovesQuick(whiteMoves, true).ToList();

            var row = Castling.GetRow(whiteMoves);
            if (leftOk && !rightOk)
            {
                var target = (2, row);
                moves.Count(m => m.Castling).ShouldBe(1);
                moves.ShouldContain(m => m.Castling && m.NewPos.Equals(target));
            }
            if (rightOk && !leftOk)
            {
                var target = (6, row);
                moves.Count(m => m.Castling).ShouldBe(1);
                moves.ShouldContain(m => m.Castling && m.NewPos.Equals(target));
            }
            if (!leftOk && !rightOk)
            {
                moves.ShouldNotContain(m => m.Castling);
            }
        }
    }
}
