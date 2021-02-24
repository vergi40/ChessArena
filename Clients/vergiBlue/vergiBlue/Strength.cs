using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue
{
    static class PieceBaseStrength
    {
        public static int Pawn { get; } = 100;
        public static int Bishop { get; } = 330;
        public static int Knight { get; } = 320;
        public static int Rook { get; } = 500;
        public static int Queen { get; } = 900;
        public static int King { get; } = 200000;

        public static double CheckMateThreshold => King * 0.5;
    }
    
    /// <summary>
    /// 
    /// </summary>
    static class PositionStrength
    {
        // https://www.dailychess.com/rival/programming/evaluation.php
        // http://www.frayn.net/beowulf/theory.html#history

        // Main referenence:
        // https://www.chessprogramming.org/Simplified_Evaluation_Function
        
        public static double Pawn(bool white, (int column, int row) position)
        {
            if (white) return PawnTable[7 - position.row, position.column];
            return -PawnTable[position.row, position.column];
        }

        public static double Bishop(bool white, (int column, int row) position)
        {
            if (white) return BishopTable[7 - position.row, position.column];
            return -BishopTable[position.row, position.column];
        }

        public static double Knight(bool white, (int column, int row) position)
        {
            if (white) return KnightTable[7 - position.row, position.column];
            return -KnightTable[position.row, position.column];
        }

        public static double Rook(bool white, (int column, int row) position)
        {
            if (white) return RookTable[7 - position.row, position.column];
            return -RookTable[position.row, position.column];
        }

        public static double Queen(bool white, (int column, int row) position)
        {
            if (white) return QueenTable[7 - position.row, position.column];
            return -QueenTable[position.row, position.column];
        }

        public static double KingStartToMiddleGame(bool white, (int column, int row) position)
        {
            if (white) return KingTable[7 - position.row, position.column];
            return -KingTable[position.row, position.column];
        }

        public static double KingEndGame(bool white, (int column, int row) position)
        {
            if (white) return KingEndGameTable[7 - position.row, position.column];
            return -KingEndGameTable[position.row, position.column];
        }

        private static double[,] PawnTable { get; } = new double[8, 8]
        {
            { 0,  0,  0,  0,  0,  0,  0,  0},// Index 0,n
            {50, 50, 50, 50, 50, 50, 50, 50},// Index 1,n
            {10, 10, 20, 30, 30, 20, 10, 10},
            { 5,  5, 10, 25, 25, 10,  5,  5},
            { 0,  0,  0, 20, 20,  0,  0,  0},
            { 5, -5,-10,  0,  0,-10, -5,  5},
            { 5, 10, 10,-20,-20, 10, 10,  5},
            { 0,  0,  0,  0,  0,  0,  0,  0}// Index 7,0
        };

        private static double[,] BishopTable { get; } = new double[8, 8]
        {
            {-20,-10,-10,-10,-10,-10,-10,-20},
            {-10,  0,  0,  0,  0,  0,  0,-10},
            {-10,  0,  5, 10, 10,  5,  0,-10},
            {-10,  5,  5, 10, 10,  5,  5,-10},
            {-10,  0, 10, 10, 10, 10,  0,-10},
            {-10, 10, 10, 10, 10, 10, 10,-10},
            {-10,  5,  0,  0,  0,  0,  5,-10},
            {-20,-10,-10,-10,-10,-10,-10,-20}
        };

        private static double[,] KnightTable { get; } = new double[8, 8]
        {
            {-50,-40,-30,-30,-30,-30,-40,-50},
            {-40,-20,  0,  0,  0,  0,-20,-40},
            {-30,  0, 10, 15, 15, 10,  0,-30},
            {-30,  5, 15, 20, 20, 15,  5,-30},
            {-30,  0, 15, 20, 20, 15,  0,-30},
            {-30,  5, 10, 15, 15, 10,  5,-30},
            {-40,-20,  0,  5,  5,  0,-20,-40},
            {-50,-40,-30,-30,-30,-30,-40,-50}
        };

        private static double[,] RookTable { get; } = new double[8, 8]
        {
            { 0,  0,  0,  0,  0,  0,  0,  0},
            { 5, 10, 10, 10, 10, 10, 10,  5},
            {-5,  0,  0,  0,  0,  0,  0, -5},
            {-5,  0,  0,  0,  0,  0,  0, -5},
            {-5,  0,  0,  0,  0,  0,  0, -5},
            {-5,  0,  0,  0,  0,  0,  0, -5},
            {-5,  0,  0,  0,  0,  0,  0, -5},
            { 0,  0,  0,  5,  5,  0,  0,  0}
        };

        private static double[,] QueenTable { get; } = new double[8, 8]
        {
            {-20,-10,-10, -5, -5,-10,-10,-20},
            {-10,  0,  0,  0,  0,  0,  0,-10},
            {-10,  0,  5,  5,  5,  5,  0,-10},
            { -5,  0,  5,  5,  5,  5,  0, -5},
            {  0,  0,  5,  5,  5,  5,  0, -5},
            {-10,  5,  5,  5,  5,  5,  0,-10},
            {-10,  0,  5,  0,  0,  0,  0,-10},
            {-20,-10,-10, -5, -5,-10,-10,-20}
        };

        private static double[,] KingTable { get; } = new double[8, 8]
        {
            {-30,-40,-40,-50,-50,-40,-40,-30},
            {-30,-40,-40,-50,-50,-40,-40,-30},
            {-30,-40,-40,-50,-50,-40,-40,-30},
            {-30,-40,-40,-50,-50,-40,-40,-30},
            {-20,-30,-30,-40,-40,-30,-30,-20},
            {-10,-20,-20,-20,-20,-20,-20,-10},
            { 20, 20,  0,  0,  0,  0, 20, 20},
            { 20, 30, 10,  0,  0, 10, 30, 20}
        };

        private static double[,] KingEndGameTable { get; } = new double[8, 8]
        {
            {-50,-40,-30,-20,-20,-30,-40,-50},
            {-30,-20,-10,  0,  0,-10,-20,-30},
            {-30,-10, 20, 30, 30, 20,-10,-30},
            {-30,-10, 30, 40, 40, 30,-10,-30},
            {-30,-10, 30, 40, 40, 30,-10,-30},
            {-30,-10, 20, 30, 30, 20,-10,-30},
            {-30,-30,  0,  0,  0,  0,-30,-30},
            {-50,-30,-30,-30,-30,-30,-30,-50}
        };
    }
    
    
}
