using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using vergiBlue;
using vergiBlue.BoardModel;
using vergiBlue.Pieces;

namespace UnitTests.MoveGeneration
{
    [TestClass]
    public class MoveGenerationTests
    {
        [TestMethod]
        public void Pawn()
        {
            // Pawn move generation
            // 7    
            // 6           
            // 5       
            // 4   x           P
            // 3     P         P
            // 2 x       
            // 1   P      
            // 0          
            //   0 1 2 3 4 5 6 7 
            var sut1 = PieceFactory.Create('P', (1, 1));
            var sut2 = PieceFactory.Create('P', (2, 3));
            var sut3 = PieceFactory.Create('P', (7, 3));

            var board = BoardFactory.CreateEmptyBoard();
            var pieces = new List<PieceBase>
            {
                new Pawn(false, (0, 2)),
                new Pawn(false, (1, 4)),
                new Pawn(true, (7, 4)),

                sut1, sut2, sut3
            };

            board.AddNew(pieces);

            var m1 = sut1.Moves(board).ToList();
            m1.Count.ShouldBe(3);
            CommonAsserts.Assert_ContainsPositions(m1.Select(m => m.NewPos), (0,2), (1,2), (1,3));
            CommonAsserts.Assert_ContainsCaptures(m1, (0,2));

            var m2 = sut2.Moves(board).ToList();
            m2.Count.ShouldBe(2);
            CommonAsserts.Assert_ContainsPositions(m2.Select(m => m.NewPos), (1, 4), (2,4));
            CommonAsserts.Assert_ContainsCaptures(m2, (1,4));

            var m3 = sut3.Moves(board).ToList();
            m3.Count.ShouldBe(0);
        }

        [TestMethod]
        public void Pawn_EnPassant()
        {

        }

        [TestMethod]
        public void Bishop()
        {
            var (board, sut) = CreateQRBTestBoard('B');

            var moves = sut.Moves(board).ToList();
            moves.Count.ShouldBe(9);

            AssertBishopMoves(moves);
            CommonAsserts.Assert_ContainsCaptures(moves, (6,5));
        }

        [TestMethod]
        public void Rook()
        {
            var (board, sut) = CreateQRBTestBoard('R');

            var moves = sut.Moves(board).ToList();
            moves.Count.ShouldBe(10);

            AssertRookMoves(moves);
            CommonAsserts.Assert_ContainsCaptures(moves, (3,5));
        }

        [TestMethod]
        public void Queen()
        {
            var (board, sut) = CreateQRBTestBoard('Q');

            var moves = sut.Moves(board).ToList();
            moves.Count.ShouldBe(19);

            AssertBishopMoves(moves);
            AssertRookMoves(moves);
            CommonAsserts.Assert_ContainsCaptures(moves, (3,5), (6,5));
        }

        [TestMethod]
        public void Knight()
        {
            var (board, sut) = CreateKnightTestBoard();

            var moves = sut.Moves(board).ToList();
            var positions = moves.Select(m => m.NewPos).ToList();

            positions.Count.ShouldBe(7);
            var expected = new List<(int, int)>
            {
                (5,1), (3,1), 
                (2,2), (2,4),
                (3,5), (5,5),
                (6,4)
            };

            CommonAsserts.Assert_ContainsPositions(positions, expected);
            CommonAsserts.Assert_ContainsCaptures(moves, (3,5));
        }

        // Generation tests bishop/rook/queen
        // sut (3,2)
        // Own pieces: (5,0) (6,2)
        // Opponent (3,5) (6,5)
        // 
        // 7       Q    
        // 6               p
        // 5       p     p
        // 4         
        // 3         
        // 2       x     P  
        // 1         
        // 0           B
        //   0 1 2 3 4 5 6 7  
        private (IBoard, PieceBase) CreateQRBTestBoard(char pieceIdentity)
        {

            var sut = PieceFactory.Create(pieceIdentity, (3, 2));

            var board = BoardFactory.CreateEmptyBoard();
            var pieces = new List<PieceBase>
            {
                new Bishop(true, (5, 0)),
                new Pawn(true, (6, 2)),
                new Pawn(false, (3, 5)),
                new Pawn(false, (6, 5)),
                new Pawn(false, (7, 6)),
                new Queen(false, (3,7)),

                sut
            };

            board.AddNew(pieces);
            return (board, sut);
        }

        private void AssertBishopMoves(IEnumerable<SingleMove> moves)
        {
            var positions = moves.Select(m => m.NewPos).ToList();

            var expected = new List<(int, int)>
            {
                (2, 1), (1,0),
                (2,3), (1,4), (0,5),
                (4,3), (5,4), (6,5),
                (4,1)
            };

            CommonAsserts.Assert_ContainsPositions(positions, expected);
        }

        private void AssertRookMoves(IEnumerable<SingleMove> moves)
        {
            var positions = moves.Select(m => m.NewPos).ToList();

            var expected = new List<(int, int)>
            {
                (3,1), (3,0),
                (2,2), (1,2), (0,2),
                (3,3), (3,4), (3,5),
                (4,2), (5,2)
            };

            CommonAsserts.Assert_ContainsPositions(positions, expected);
        }

        // Generation tests knight
        // sut (4,3)
        // Own pieces: (6,2)
        // Opponent (3,5)
        // 
        // 7       Q    
        // 6               p
        // 5       p     p
        // 4         
        // 3         N
        // 2             P  
        // 1         
        // 0           B
        //   0 1 2 3 4 5 6 7  
        private (IBoard, PieceBase) CreateKnightTestBoard()
        {

            var sut = PieceFactory.Create('N', (4, 3));

            var board = BoardFactory.CreateEmptyBoard();
            var pieces = new List<PieceBase>
            {
                new Bishop(true, (5, 0)),
                new Pawn(true, (6, 2)),
                new Pawn(false, (3, 5)),
                new Pawn(false, (6, 5)),
                new Pawn(false, (7, 6)),
                new Queen(false, (3,7)),

                sut
            };

            board.AddNew(pieces);
            return (board, sut);
        }


        
    }
}
